#region 控件拖放

#region 拖放所需数据
//获取全屏鼠标位置所需数据结构
[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
    public POINT(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
}

private static IntPtr hWnd;                     //窗口句柄
private const int GWL_EXSTYLE = -20;            //window消息类型----样式
private const int WS_EX_TRANSPARENT = 0x20;     //窗口windows样式
private uint exStyle;                           //原窗口windows样式
private CustomWindow newWindow;                 //移动所需临时窗口    ps：也可能不是临时窗口
private Control SourceHost;                     //用于临时存放数据的容器
private FrameworkElement temp;                  //拖放视觉效果临时控件
private Point StartPoint;                       //鼠标对操作控件的相对位置

[DllImport("user32.dll")]
public static extern bool GetCursorPos(out POINT pt);
[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
public static extern uint SetWindowLong(IntPtr hwnd, int nindex, uint dwNewLong);
[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
public static extern uint GetWindowLong(IntPtr hwnd, int nindex);
#endregion

private void Root_PreviewMouseMove(object sender, MouseEventArgs e)
{
    if (e.LeftButton == MouseButtonState.Pressed)
    {
        if (e.Source is Button)
            return;
        //获取鼠标在操作控件的相对坐标
        StartPoint = e.GetPosition((UIElement)sender);
        //为移动的Items相生成容器
        TabControl tabControl = new TabControl();
        SourceHost = tabControl;
        //设置容器的样式与原来的一致
        tabControl.ItemContainerStyle = xesds.ItemContainerStyle;
        tabControl.Template = xesds.Template;
        tabControl.Width = xesds.ActualWidth;
        tabControl.Height = xesds.ActualHeight;
        tabControl.Background = System.Windows.Media.Brushes.Transparent;

        TabItem tabItem = ((FrameworkElement)sender).TemplatedParent as TabItem;
        TabControl tabControlSource = (TabControl)tabItem.Parent;
        //从原Item容器移除Item
        tabControlSource.Items.Remove(tabItem);
        //为新增的容器添加Item
        tabControl.Items.Add(tabItem);
        //判断原Item容器是否没有了元素
        if (tabControlSource.Items.Count == 0)
        {
            //没有元素那么移除该容器
            Panel panel = tabControlSource.Parent as Panel;
            panel.Children.Remove(tabControlSource);
            //判断该容器的容器的名字是不是CustomWindow的ContentGrid元素，并判断该元素是否还有其他子元素
            if (panel.Name == "Workaer" && panel.Children.Count == 0)
            {
                //没有子元素将关闭该窗口
                Window window = Window.GetWindow(panel);
                window.Close();
            }
        }
        //新增窗口
        //新增窗口的目的是，单个控件是无法在窗口之外进行移动，需要以窗口为媒介
        newWindow = new CustomWindow();
        //  newWindow.ResizeAction = ResizeModes.None;

        //需要设置此项，否则要实现窗口的鼠标穿透将要花费很多的精力
        newWindow.AllowsTransparency = true;
        //需要执行以下操作使得窗口能穿透
        newWindow.SourceInitialized += NewWindow_SourceInitialized;
        //需要在窗口完成布局测量后才能获取Item容器在窗口的位置数据
        newWindow.Loaded += NewWindow_Loaded;


        //将窗口的大小设置为原Item容器的大小并高度上增加一点（预留窗口控制栏）
        newWindow.Width = tabControl.Width;
        newWindow.Height = tabControl.Height + 40;
        //将新建容器添加到新窗口的ContentGrid容器里
        newWindow.ContentGrid.Children.Add(tabControl);

        //隐藏除Item容器的其他部分，该设置为用户友好，避免视觉上其他内容影响移动判断
        newWindow.ContorlBar.Visibility = Visibility.Hidden;
        newWindow.Show();

        //启动拖放操作，传入对象是该控件-------------该控件不是Item整体只是其中用于显示Header的StackPanel控件
        //数据则是Item本身
        DragDrop.DoDragDrop((UIElement)sender, tabItem, DragDropEffects.Move);

    }
}

private void NewWindow_Loaded(object sender, RoutedEventArgs e)
{
    //获取Item容器在窗口的相对位置
    Point pointNewWindow = SourceHost.TranslatePoint(new Point(0, 0), (UIElement)sender);
    //该计算原理是，二者都是相对位置，因此，坐标数据实际上是距离数据，将二者相加获得的是控件相对窗口的相对坐标
    //前提是控件与容器属于包含关系才能进行该操作，且没有其他的特殊的带有位置偏移的布局设置例如Margin
    StartPoint.X += pointNewWindow.X;
    StartPoint.Y += pointNewWindow.Y;

    POINT SC;
    //该函数的所返回的坐标实际是像素位置，与WPF的大小计量不同，所以仍需要对此数据进行修正
    GetCursorPos(out SC);
    //设置窗口位置--------------实际效果是鼠标位置将会在原来点击控件的相对位置上
    newWindow.Left = SC.X - StartPoint.X;
    newWindow.Top = SC.Y - StartPoint.Y;
}

//设置窗口穿透
//实现窗口穿透的实际原理是将该窗口的样式添加WS_EX_TRANSPARENT
private void NewWindow_SourceInitialized(object sender, EventArgs e)
{
    hWnd = new WindowInteropHelper((Window)sender).Handle;
    exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
    SetWindowLong(hWnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT);
}

//设置窗口位置随鼠标位置
//该函数是WPF拖放功能的定时函数，所以并不是鼠标移动一次就返回一次，因此会有点影响性能
private void Root_GiveFeedback(object sender, GiveFeedbackEventArgs e)
{
    //设置窗口位置--------------实际效果是鼠标位置将会在原来点击控件的相对位置上
    POINT ppt;
    GetCursorPos(out ppt);
    newWindow.Left = ppt.X - StartPoint.X;
    newWindow.Top = ppt.Y - StartPoint.Y;
    e.Handled = true;
}

//设置当鼠标弹起时，结束拖放操作
//需要注意Drop操作也是由该函数更改e.Action引起的，且drop事件的引发由系统写的该函数引起
//因此不应该将该QueryContinueDragEventArgs设置为已处理，否者会导致无法触发Drop;
private void Root_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
{
    if (e.KeyStates == DragDropKeyStates.None)
    {
        //退出拖放
        e.Action = DragAction.Cancel;
        //还原窗口样式--------取消窗口穿透
        SetWindowLong(hWnd, GWL_EXSTYLE, exStyle);
        //将隐藏的内容重新显现
        SourceHost.Background = System.Windows.Media.Brushes.White;
        newWindow.ContorlBar.Visibility = Visibility.Visible;

    }

}

//该函数用于拖放进入后的视觉效果

private void headerPanel_DragEnter(object sender, DragEventArgs e)
{
    //该视觉效果是新能一个item背景色为蓝色，无内容
    TabItem tab = (TabItem)e.Data.GetData(typeof(TabItem));
    StackPanel stackPanel = tab.Template.FindName("Root", tab) as StackPanel;
    temp = new TabItem() { Header = new Grid() { Width = 40 }, IsHitTestVisible = false, Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x33, 0xAD, 0xFF)) };
    //将拖放源的透明度更改为0.4
    //这也是用户友好，避免不透明引起的视觉干扰
    stackPanel.Opacity = 0.4;
    (((FrameworkElement)sender).TemplatedParent as TabControl).Items.Add(temp);
}

//该函数用于拖放离开后的视觉效果
private void headerPanel_DragLeave(object sender, DragEventArgs e)
{
    //移除添加的临时Item
    TabItem tab = (TabItem)e.Data.GetData(typeof(TabItem));
    StackPanel stackPanel = tab.Template.FindName("Root", tab) as StackPanel;
    (((FrameworkElement)sender).TemplatedParent as TabControl).Items.Remove(temp);
    //恢复拖放源的透明度
    stackPanel.Opacity = 1;
}


//拖放操作的放操作
//该功能仍需优化
//问题拖放目标不应该只是一个TabControl，也可能是一个其他的容器，例如Grid，DockPanel等
//所以放入时需要根据实际情况来准备容器
private void headerPanel_Drop(object sender, DragEventArgs e)
{

    TabItem tab = (TabItem)e.Data.GetData(typeof(TabItem));
    StackPanel stackPanel = tab.Template.FindName("Root", tab) as StackPanel;
    //需要恢复拖放源透明度-----------DragEnter将透明度降低了
    stackPanel.Opacity = 1;
    //将Item从临时容器中移除
    ((ItemsControl)SourceHost).Items.Remove(tab);
    //移除用于视觉效果的临时Item
    (((FrameworkElement)sender).TemplatedParent as TabControl).Items.Remove(temp);
    //添加Item到目标容器
    (((FrameworkElement)sender).TemplatedParent as TabControl).Items.Add(tab);
    //关闭用于移动的窗口
    newWindow.Close();

}
    }
    #endregion