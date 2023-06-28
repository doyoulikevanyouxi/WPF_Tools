#region �ؼ��Ϸ�

#region �Ϸ���������
//��ȡȫ�����λ���������ݽṹ
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

private static IntPtr hWnd;                     //���ھ��
private const int GWL_EXSTYLE = -20;            //window��Ϣ����----��ʽ
private const int WS_EX_TRANSPARENT = 0x20;     //����windows��ʽ
private uint exStyle;                           //ԭ����windows��ʽ
private CustomWindow newWindow;                 //�ƶ�������ʱ����    ps��Ҳ���ܲ�����ʱ����
private Control SourceHost;                     //������ʱ������ݵ�����
private FrameworkElement temp;                  //�Ϸ��Ӿ�Ч����ʱ�ؼ�
private Point StartPoint;                       //���Բ����ؼ������λ��

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
        //��ȡ����ڲ����ؼ����������
        StartPoint = e.GetPosition((UIElement)sender);
        //Ϊ�ƶ���Items����������
        TabControl tabControl = new TabControl();
        SourceHost = tabControl;
        //������������ʽ��ԭ����һ��
        tabControl.ItemContainerStyle = xesds.ItemContainerStyle;
        tabControl.Template = xesds.Template;
        tabControl.Width = xesds.ActualWidth;
        tabControl.Height = xesds.ActualHeight;
        tabControl.Background = System.Windows.Media.Brushes.Transparent;

        TabItem tabItem = ((FrameworkElement)sender).TemplatedParent as TabItem;
        TabControl tabControlSource = (TabControl)tabItem.Parent;
        //��ԭItem�����Ƴ�Item
        tabControlSource.Items.Remove(tabItem);
        //Ϊ�������������Item
        tabControl.Items.Add(tabItem);
        //�ж�ԭItem�����Ƿ�û����Ԫ��
        if (tabControlSource.Items.Count == 0)
        {
            //û��Ԫ����ô�Ƴ�������
            Panel panel = tabControlSource.Parent as Panel;
            panel.Children.Remove(tabControlSource);
            //�жϸ������������������ǲ���CustomWindow��ContentGridԪ�أ����жϸ�Ԫ���Ƿ���������Ԫ��
            if (panel.Name == "Workaer" && panel.Children.Count == 0)
            {
                //û����Ԫ�ؽ��رոô���
                Window window = Window.GetWindow(panel);
                window.Close();
            }
        }
        //��������
        //�������ڵ�Ŀ���ǣ������ؼ����޷��ڴ���֮������ƶ�����Ҫ�Դ���Ϊý��
        newWindow = new CustomWindow();
        //  newWindow.ResizeAction = ResizeModes.None;

        //��Ҫ���ô������Ҫʵ�ִ��ڵ���괩͸��Ҫ���Ѻܶ�ľ���
        newWindow.AllowsTransparency = true;
        //��Ҫִ�����²���ʹ�ô����ܴ�͸
        newWindow.SourceInitialized += NewWindow_SourceInitialized;
        //��Ҫ�ڴ�����ɲ��ֲ�������ܻ�ȡItem�����ڴ��ڵ�λ������
        newWindow.Loaded += NewWindow_Loaded;


        //�����ڵĴ�С����ΪԭItem�����Ĵ�С���߶�������һ�㣨Ԥ�����ڿ�������
        newWindow.Width = tabControl.Width;
        newWindow.Height = tabControl.Height + 40;
        //���½�������ӵ��´��ڵ�ContentGrid������
        newWindow.ContentGrid.Children.Add(tabControl);

        //���س�Item�������������֣�������Ϊ�û��Ѻã������Ӿ�����������Ӱ���ƶ��ж�
        newWindow.ContorlBar.Visibility = Visibility.Hidden;
        newWindow.Show();

        //�����ϷŲ�������������Ǹÿؼ�-------------�ÿؼ�����Item����ֻ������������ʾHeader��StackPanel�ؼ�
        //��������Item����
        DragDrop.DoDragDrop((UIElement)sender, tabItem, DragDropEffects.Move);

    }
}

private void NewWindow_Loaded(object sender, RoutedEventArgs e)
{
    //��ȡItem�����ڴ��ڵ����λ��
    Point pointNewWindow = SourceHost.TranslatePoint(new Point(0, 0), (UIElement)sender);
    //�ü���ԭ���ǣ����߶������λ�ã���ˣ���������ʵ�����Ǿ������ݣ���������ӻ�õ��ǿؼ���Դ��ڵ��������
    //ǰ���ǿؼ����������ڰ�����ϵ���ܽ��иò�������û������������Ĵ���λ��ƫ�ƵĲ�����������Margin
    StartPoint.X += pointNewWindow.X;
    StartPoint.Y += pointNewWindow.Y;

    POINT SC;
    //�ú����������ص�����ʵ��������λ�ã���WPF�Ĵ�С������ͬ����������Ҫ�Դ����ݽ�������
    GetCursorPos(out SC);
    //���ô���λ��--------------ʵ��Ч�������λ�ý�����ԭ������ؼ������λ����
    newWindow.Left = SC.X - StartPoint.X;
    newWindow.Top = SC.Y - StartPoint.Y;
}

//���ô��ڴ�͸
//ʵ�ִ��ڴ�͸��ʵ��ԭ���ǽ��ô��ڵ���ʽ���WS_EX_TRANSPARENT
private void NewWindow_SourceInitialized(object sender, EventArgs e)
{
    hWnd = new WindowInteropHelper((Window)sender).Handle;
    exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
    SetWindowLong(hWnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT);
}

//���ô���λ�������λ��
//�ú�����WPF�ϷŹ��ܵĶ�ʱ���������Բ���������ƶ�һ�ξͷ���һ�Σ���˻��е�Ӱ������
private void Root_GiveFeedback(object sender, GiveFeedbackEventArgs e)
{
    //���ô���λ��--------------ʵ��Ч�������λ�ý�����ԭ������ؼ������λ����
    POINT ppt;
    GetCursorPos(out ppt);
    newWindow.Left = ppt.X - StartPoint.X;
    newWindow.Top = ppt.Y - StartPoint.Y;
    e.Handled = true;
}

//���õ���굯��ʱ�������ϷŲ���
//��Ҫע��Drop����Ҳ���ɸú�������e.Action����ģ���drop�¼���������ϵͳд�ĸú�������
//��˲�Ӧ�ý���QueryContinueDragEventArgs����Ϊ�Ѵ������߻ᵼ���޷�����Drop;
private void Root_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
{
    if (e.KeyStates == DragDropKeyStates.None)
    {
        //�˳��Ϸ�
        e.Action = DragAction.Cancel;
        //��ԭ������ʽ--------ȡ�����ڴ�͸
        SetWindowLong(hWnd, GWL_EXSTYLE, exStyle);
        //�����ص�������������
        SourceHost.Background = System.Windows.Media.Brushes.White;
        newWindow.ContorlBar.Visibility = Visibility.Visible;

    }

}

//�ú��������ϷŽ������Ӿ�Ч��

private void headerPanel_DragEnter(object sender, DragEventArgs e)
{
    //���Ӿ�Ч��������һ��item����ɫΪ��ɫ��������
    TabItem tab = (TabItem)e.Data.GetData(typeof(TabItem));
    StackPanel stackPanel = tab.Template.FindName("Root", tab) as StackPanel;
    temp = new TabItem() { Header = new Grid() { Width = 40 }, IsHitTestVisible = false, Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x33, 0xAD, 0xFF)) };
    //���Ϸ�Դ��͸���ȸ���Ϊ0.4
    //��Ҳ���û��Ѻã����ⲻ͸��������Ӿ�����
    stackPanel.Opacity = 0.4;
    (((FrameworkElement)sender).TemplatedParent as TabControl).Items.Add(temp);
}

//�ú��������Ϸ��뿪����Ӿ�Ч��
private void headerPanel_DragLeave(object sender, DragEventArgs e)
{
    //�Ƴ���ӵ���ʱItem
    TabItem tab = (TabItem)e.Data.GetData(typeof(TabItem));
    StackPanel stackPanel = tab.Template.FindName("Root", tab) as StackPanel;
    (((FrameworkElement)sender).TemplatedParent as TabControl).Items.Remove(temp);
    //�ָ��Ϸ�Դ��͸����
    stackPanel.Opacity = 1;
}


//�ϷŲ����ķŲ���
//�ù��������Ż�
//�����Ϸ�Ŀ�겻Ӧ��ֻ��һ��TabControl��Ҳ������һ������������������Grid��DockPanel��
//���Է���ʱ��Ҫ����ʵ�������׼������
private void headerPanel_Drop(object sender, DragEventArgs e)
{

    TabItem tab = (TabItem)e.Data.GetData(typeof(TabItem));
    StackPanel stackPanel = tab.Template.FindName("Root", tab) as StackPanel;
    //��Ҫ�ָ��Ϸ�Դ͸����-----------DragEnter��͸���Ƚ�����
    stackPanel.Opacity = 1;
    //��Item����ʱ�������Ƴ�
    ((ItemsControl)SourceHost).Items.Remove(tab);
    //�Ƴ������Ӿ�Ч������ʱItem
    (((FrameworkElement)sender).TemplatedParent as TabControl).Items.Remove(temp);
    //���Item��Ŀ������
    (((FrameworkElement)sender).TemplatedParent as TabControl).Items.Add(tab);
    //�ر������ƶ��Ĵ���
    newWindow.Close();

}
    }
    #endregion