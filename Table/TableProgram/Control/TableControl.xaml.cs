using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Table.Control
{
    /// <summary>
    /// TableControl.xaml 的交互逻辑
    /// </summary>
    public partial class TableControl : UserControl
    { //内部表格的宽度
        public static readonly DependencyProperty InWidthProperty = DependencyProperty.Register("InWidth", typeof(double), typeof(TableControl), new PropertyMetadata((double)0));
        //内部表格的高度
        public static readonly DependencyProperty InHeightProperty = DependencyProperty.Register("InHeight", typeof(double), typeof(TableControl), new PropertyMetadata((double)0));
        //是否显示边框
        public static readonly DependencyProperty ShowBorderProperty = DependencyProperty.Register("ShowBorder", typeof(bool), typeof(TableControl), new PropertyMetadata(true));


        bool IsDecorate = false;
        //是否点击
        bool iPress = false;
        //是否选择区域
        bool iSelect = false;
        //是否选择行列
        bool iSelectRC = false;
        //鼠标点击坐标 R/C坐标系
        int RStart = -1;
        int CStart = -1;
        //记录选择区域最大的单元格区域坐标（右下角） R/C坐标系
        Point startPoint;

        //行列选择装饰
        private UIElement SelectCR = new Grid() { Background = new SolidColorBrush(Colors.LightGray), Opacity = 0.5, Focusable = true };
        //区域选择装饰
        private UIElement SelectAerea = new Grid() { Background = new SolidColorBrush(Color.FromRgb(0x00, 0x61, 0xff)), Opacity = 0.6, Focusable = true };
        //附加Adorner装饰器，具有layer的元素
        private AdornerLayer adornerLayer = null;

        public double InWidth
        {
            get => (double)GetValue(InWidthProperty);
            set => SetValue(InWidthProperty, value);
        }
        public double InHeight
        {
            get => (double)GetValue(InHeightProperty);
            set => SetValue(InHeightProperty, value);
        }
        public bool ShowBorder
        {
            get => (bool)GetValue(ShowBorderProperty);
            set => SetValue(ShowBorderProperty, value);
        }

        public TableControl()
        {
            InitializeComponent();
            Loaded += TableControl_Loaded;
        }

        private void TableControl_Loaded(object sender, RoutedEventArgs e)
        {
            adornerLayer = AdornerLayer.GetAdornerLayer(TableAerea);
            this.LostKeyboardFocus += TableControl_LostKeyboardFocus;
        
        }
        private void TableControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (TableAerea.Children.Contains(SelectCR))
                TableAerea.Children.Remove(SelectCR);
            if (TableAerea.Children.Contains(SelectAerea))
                TableAerea.Children.Remove(SelectAerea);
        }
        private void AssistAerea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).Focus();
            e.Handled = true;
            if (iSelectRC)
            {
                TableAerea.Children.Remove(SelectCR);
                iSelectRC = false;
            }
            if (iSelect)
            {
                TableAerea.Children.Remove(SelectAerea);
                iSelectRC = false;
            }
            Point point = e.GetPosition(TableAerea);
            if ((point.X > 0 && point.Y > 0))
                return;
            if (SelectCR != null)
                TableAerea.Children.Remove(SelectCR);
            if (point.X < 0)
            {
                if (point.Y < 0)
                    return;
                int d = (int)(point.Y / (TableAerea.ActualHeight / TableAerea.RowDefinitions.Count));
                if (d >= TableAerea.RowDefinitions.Count)
                    return;
                Grid.SetRow(SelectCR, d);
                Grid.SetColumn(SelectCR, 0);
                Grid.SetColumnSpan(SelectCR, TableAerea.ColumnDefinitions.Count);
                Grid.SetRowSpan(SelectCR, 1);
                TableAerea.Children.Add(SelectCR);


                iSelectRC = true;

            }
            else
            {
                if (point.X < 0)
                    return;
                int d = (int)(point.X / (TableAerea.ActualWidth / TableAerea.ColumnDefinitions.Count));
                if (d >= TableAerea.ColumnDefinitions.Count)
                    return;
                Grid.SetColumn(SelectCR, d);
                Grid.SetRow(SelectCR, 0);

                Grid.SetRowSpan(SelectCR, TableAerea.RowDefinitions.Count);
                Grid.SetColumnSpan(SelectCR, 1);

                TableAerea.Children.Add(SelectCR);
                iSelectRC = true;

            }

        }
        private void AssistAerea_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TableSelection.IsOpen = true;
            deleteButtonS.IsEnabled = false;
            deleteButtonRC.IsEnabled = false;
            if (iSelect)
                deleteButtonS.IsEnabled = true;
            if (iSelectRC)
                deleteButtonRC.IsEnabled = true;
            e.Handled = true;
        }


        //计算添加提示出现位置，可优化，单元格大小与实际不匹配，响应范围太广
        //已优化
        private void AssistAerea_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!TableAerea.ShowBorder)
                return;

            if (IsDecorate)
            {
                foreach (var item in adornerLayer.GetAdorners(TableAerea))
                    adornerLayer.Remove(item);
                IsDecorate = false;
            }
            Point point = e.GetPosition(TableAerea);
            ValuePair<int, int> valuePair = TableAerea.GetRCPoint(point);
            if ((valuePair.Frist < 0 && valuePair.Second < 0) || (valuePair.Frist >= 0 && valuePair.Second >= 0))
                return;
            //横向
            if (point.X < 0 && point.X > -20f)
            {
                if (valuePair.Frist == -1)
                    return;
                if (valuePair.Frist == TableAerea.RowDefinitions.Count - 1)
                    return;
                Point point1 = TableAerea.GetPoint(valuePair);
                double lc = point1.Y - point.Y;

                if (lc < TableAerea.RowDefinitions[valuePair.Frist].ActualHeight / 4f && lc < 10f)
                {
                    TableAdorner tableAdor = new TableAdorner(TableAerea, true);
                    tableAdor.SetOffsetY(point1.Y);
                    tableAdor.Row = valuePair.Frist;
                    tableAdor.Click += AddRC;
                    adornerLayer.Add(tableAdor);
                    IsDecorate = true;
                }
                if (valuePair.Frist == 0)
                    return;
                double XP = point1.Y - TableAerea.RowDefinitions[valuePair.Frist].ActualHeight;
                double lc2 = point.Y - XP;
                if (lc > TableAerea.RowDefinitions[valuePair.Frist].ActualHeight / 4f * 3 && lc2 < 10f)
                {
                    TableAdorner tableAdor = new TableAdorner(TableAerea, true);
                    tableAdor.SetOffsetY(XP);
                    tableAdor.Row = valuePair.Frist - 1;
                    tableAdor.Click += AddRC;
                    adornerLayer.Add(tableAdor);
                    IsDecorate = true;
                }

            }
            //垂直
            if (point.Y < 0 && point.Y > -20f)
            {
                if (valuePair.Second == -1)
                    return;
                if (valuePair.Second == TableAerea.ColumnDefinitions.Count - 1)
                    return;
                Point point1 = TableAerea.GetPoint(valuePair);
                double lc = point1.X - point.X;

                if (lc < TableAerea.ColumnDefinitions[valuePair.Second].ActualWidth / 4f && lc < 10f)
                {
                    TableAdorner tableAdor = new TableAdorner(TableAerea);
                    tableAdor.SetOffsetX(point1.X);
                    tableAdor.Column = valuePair.Second;
                    tableAdor.Click += AddRC;
                    adornerLayer.Add(tableAdor);
                    IsDecorate = true;
                }
                if (valuePair.Second == 0)
                    return;
                double YP = point1.X - TableAerea.ColumnDefinitions[valuePair.Second].ActualWidth;
                double lc2 = point.X - YP;
                if (lc > TableAerea.ColumnDefinitions[valuePair.Second].ActualWidth / 4f * 3 && lc2 < 10f)
                {
                    TableAdorner tableAdor = new TableAdorner(TableAerea);
                    tableAdor.SetOffsetX(YP);
                    tableAdor.Column = valuePair.Second - 1;
                    tableAdor.Click += AddRC;
                    adornerLayer.Add(tableAdor);
                    IsDecorate = true;
                }

            }
        }
        private void TableAerea_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            (sender as UIElement).Focus();
            if (TableAerea.Children.Contains(SelectAerea))
                TableAerea.Children.Remove(SelectAerea);
            iSelect = false;
            startPoint = e.GetPosition(TableAerea);

            ValuePair<int, int> valuePair = TableAerea.GetRCPoint(startPoint);
            RStart = valuePair.Frist;
            CStart = valuePair.Second;

            if (e.ClickCount == 2)
            {

                for (int i = 0; i < TableAerea.CombinedArea[0].Count; ++i)
                {
                    if (RStart >= TableAerea.CombinedArea[0][i].Frist && RStart < TableAerea.CombinedArea[0][i].Frist + TableAerea.CombinedArea[0][i].Second)
                    {
                        if (CStart >= TableAerea.CombinedArea[1][i].Frist && CStart < TableAerea.CombinedArea[1][i].Frist + TableAerea.CombinedArea[1][i].Second)
                        {
                            if (IsReapte(TableAerea.CombinedArea[0][i].Frist, TableAerea.CombinedArea[1][i].Frist))
                                goto FinishAddTextBox;
                            TextBox textBox = new TextBox() { MinWidth = TableAerea.ColumnDefinitions[CStart].ActualWidth, Text = "The text for test", TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            Grid.SetRow(textBox, TableAerea.CombinedArea[0][i].Frist);
                            Grid.SetColumn(textBox, TableAerea.CombinedArea[1][i].Frist);
                            Grid.SetRowSpan(textBox, TableAerea.CombinedArea[0][i].Second);
                            Grid.SetColumnSpan(textBox, TableAerea.CombinedArea[1][i].Second);
                            TableAerea.DataElements.Add(textBox);
                            textBox.Focus();

                            goto FinishAddTextBox;
                        }

                    }
                }
                if (IsReapte(RStart, CStart))
                    goto FinishAddTextBox;
                TextBox textBox2 = new TextBox() { MinWidth = TableAerea.ColumnDefinitions[CStart].ActualWidth, Text = "The text for test", TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(textBox2, RStart);
                Grid.SetColumn(textBox2, CStart);
                TableAerea.DataElements.Add(textBox2);
                textBox2.Focus();
                FinishAddTextBox:
                return;
            }

            //判定点击对象是否是GridSplitter,如果是就不再添加选择框并将这个鼠标事件继续路由下去
            DependencyObject frameworkElement = ((FrameworkElement)e.MouseDevice.DirectlyOver).TemplatedParent;
            if (frameworkElement != null && frameworkElement.GetType() == typeof(GridSplitter))
                return;
            TableAerea.Children.Add(SelectAerea);
            Grid.SetRow(SelectAerea, RStart);
            Grid.SetColumn(SelectAerea, CStart);
            Grid.SetRowSpan(SelectAerea, 1);
            Grid.SetColumnSpan(SelectAerea, 1);
            for (int i = 0; i < TableAerea.CombinedArea[0].Count; ++i)
            {
                if (RStart >= TableAerea.CombinedArea[0][i].Frist && RStart < TableAerea.CombinedArea[0][i].Frist + TableAerea.CombinedArea[0][i].Second)
                {
                    if (CStart >= TableAerea.CombinedArea[1][i].Frist && CStart < TableAerea.CombinedArea[1][i].Frist + TableAerea.CombinedArea[1][i].Second)
                    {
                        RStart = TableAerea.CombinedArea[0][i].Frist;
                        CStart = TableAerea.CombinedArea[1][i].Frist;
                        Grid.SetRow(SelectAerea, RStart);
                        Grid.SetColumn(SelectAerea, CStart);
                        Grid.SetRowSpan(SelectAerea, TableAerea.CombinedArea[0][i].Second);
                        Grid.SetColumnSpan(SelectAerea, TableAerea.CombinedArea[1][i].Second);
                        break;
                    }
                }
            }

            iPress = true;
            e.Handled = true;
        }
        ///计算选择区域 
        ///原理：计算鼠标移动选择区域，然后计算该区域与合并过单元格的数据比较，计算是否有交集，如果有那么改变选择区域左上角坐标，右下角坐标至二者最小，最大值
        ///
        private void TableAerea_MouseMove(object sender, MouseEventArgs e)
        {
            if (!iPress)
                return;
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            Point point = e.GetPosition(TableAerea);
            iSelect = true;
            double leng = 0f;
            //鼠标坐标 R/C坐标系
            int Rmouse = -1;
            int Cmouse = -1;
            for (int i = 0; i < TableAerea.ColumnDefinitions.Count; ++i)
            {
                leng += TableAerea.ColumnDefinitions[i].ActualWidth;
                if (point.X < leng)
                {
                    Cmouse = i;
                    leng = 0f;
                    break;
                }
            }

            for (int i = 0; i < TableAerea.RowDefinitions.Count; ++i)
            {
                leng += TableAerea.RowDefinitions[i].ActualHeight;
                if (point.Y < leng)
                {
                    Rmouse = i;
                    break;
                }
            }
            if (Rmouse == -1)
                Rmouse = 0;
            if (Cmouse == -1)
                Cmouse = 0;

            //选择区域坐标 左上角 R/C坐标系
            int RSecletTopLeft = Rmouse;
            int CSecletTopLeft = Cmouse;

            //根据鼠标与原点的关系调整 选择区域左上角坐标
            if (Rmouse >= RStart)
                RSecletTopLeft = RStart;
            if (Cmouse >= CStart)
                CSecletTopLeft = CStart;
            //临时选择区域坐标 右下角 R/C坐标系
            int Rt1 = -1;
            int Ct1 = -1;
            //选择区域大小
            int Rspan = Math.Abs(Rmouse - RStart) + 1;
            int Cspan = Math.Abs(Cmouse - CStart) + 1;

            for (int i = 0; i < TableAerea.CombinedArea[0].Count; ++i)
            {
                //选择区域R的最大值
                int Rt2 = Rspan + RSecletTopLeft - 1;
                //合并区域R的最大值
                int Rt3 = TableAerea.CombinedArea[0][i].Frist + TableAerea.CombinedArea[0][i].Second - 1;
                //R交集
                ValuePair<int, int> pairR = Intersection(new ValuePair<int, int>(RSecletTopLeft, Rt2), new ValuePair<int, int>(TableAerea.CombinedArea[0][i].Frist, Rt3));
                if (pairR == null)
                    continue;
                //鼠标选择区域C的最大值
                int Ct2 = Cspan + CSecletTopLeft - 1;
                //合并区域C的最大值
                int Ct3 = TableAerea.CombinedArea[1][i].Frist + TableAerea.CombinedArea[1][i].Second - 1;
                //C交集
                ValuePair<int, int> pairC = Intersection(new ValuePair<int, int>(CSecletTopLeft, Ct2), new ValuePair<int, int>(TableAerea.CombinedArea[1][i].Frist, Ct3));
                if (pairC == null)
                    continue;

                ///如果有交叉范围
                ///重新设定新区域
                ///计算新区域左上坐标 R/C坐标系
                ///取二者最小值
                RSecletTopLeft = Math.Min(RSecletTopLeft, TableAerea.CombinedArea[0][i].Frist);
                CSecletTopLeft = Math.Min(CSecletTopLeft, TableAerea.CombinedArea[1][i].Frist);
                ///计算新区域右下坐标 R/C坐标系
                ///取二者最大值
                Rt1 = Math.Max(Rt2, Rt3);
                Ct1 = Math.Max(Ct2, Ct3);
                //重新计算区域大小
                Rspan = Rt1 - RSecletTopLeft + 1;
                Cspan = Ct1 - CSecletTopLeft + 1;
            }
            if (Rt1 != -1)
            {
                Rspan = Rt1 - RSecletTopLeft + 1;
                Cspan = Ct1 - CSecletTopLeft + 1;
            }

            Grid.SetRow(SelectAerea, RSecletTopLeft);
            Grid.SetColumn(SelectAerea, CSecletTopLeft);
            Grid.SetRowSpan(SelectAerea, Rspan);
            Grid.SetColumnSpan(SelectAerea, Cspan);



        }
        private void TableAerea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (iPress)
                iSelect = true;
            iPress = false;
        }
        private void CombainedSelect(object sender, RoutedEventArgs e)
        {
            Combined();
            TableSelection.IsOpen = false;
        }
        private void DeleteSelectRC(object sender, RoutedEventArgs e)
        {
            DeleteRC();
            TableSelection.IsOpen = false;
        }
        private void AddRC(AdornerMouseAction adornerMouseAction, object param)
        {
            if (IsDecorate)
            {
                foreach (var item in adornerLayer.GetAdorners(TableAerea))
                    adornerLayer.Remove(item);
                IsDecorate = false;
            }
            ValuePair<int, bool> valuePair = param as ValuePair<int, bool>;
            if (valuePair.Second)
                TableAerea.RowDefinitions.Insert(valuePair.Frist + 1, new RowDefinition() { Height = new GridLength(TableAerea.ActualHeight / (TableAerea.RowDefinitions.Count + 1), GridUnitType.Star) });
            else
                TableAerea.ColumnDefinitions.Insert(valuePair.Frist + 1, new ColumnDefinition() { Width = new GridLength(TableAerea.ActualWidth / (TableAerea.ColumnDefinitions.Count + 1), GridUnitType.Star) });
        }
        private void DeleteRC()
        {

            int d = Grid.GetRowSpan(SelectCR);

            if (d == 1)
            {
                d = Grid.GetRow(SelectCR);
                TableAerea.RowDefinitions.RemoveAt(d);
                return;
            }
            d = Grid.GetColumn(SelectCR);
            TableAerea.ColumnDefinitions.RemoveAt(d);
            TableSelection.IsOpen = false;
        }
        private void Combined()
        {
            int r = Grid.GetRow(SelectAerea);
            int c = Grid.GetColumn(SelectAerea);
            int rs = Grid.GetRowSpan(SelectAerea);
            int cs = Grid.GetColumnSpan(SelectAerea);
            List<UIElement> uIElements = TableAerea.DataElements.Where(x => (Grid.GetRow(x) >= r && Grid.GetRow(x) < r + rs) && (Grid.GetColumn(x) >= c && Grid.GetColumn(x) < c + cs)).ToList();
            if (uIElements.Count != 0)
            {
                UIElement uIElement = uIElements[0];
                if (uIElements.Count == 1)
                {
                    Grid.SetRow(uIElements[0], r);
                    Grid.SetColumn(uIElements[0], c);
                    TableAerea.SetColumnSpan(uIElements[0], cs);
                    TableAerea.SetRowSpan(uIElements[0], rs);
                    return;
                }
                else
                {
                    int minR = Grid.GetRow(uIElements[0]);
                    int inde = 0;
                    for (int i = 1; i < uIElements.Count; ++i)
                    {
                        int temp = Grid.GetRow(uIElements[i]);
                        if (temp < minR)
                        {
                            minR = temp;
                            inde = i;
                        }
                    }
                    UIElement uIElementTopLeft = uIElements[inde];
                    int minC = Grid.GetColumn(uIElementTopLeft);
                    foreach (var it in uIElements.Where(x => Grid.GetRow(x) == minR).ToList())
                    {
                        int temp = Grid.GetColumn(it);
                        if (temp < minC)
                        {
                            minC = temp;
                            uIElementTopLeft = it;
                        }
                    }

                    Grid.SetRow(uIElementTopLeft, r);
                    Grid.SetColumn(uIElementTopLeft, c);
                    TableAerea.SetColumnSpan(uIElementTopLeft, cs);
                    TableAerea.SetRowSpan(uIElementTopLeft, rs);
                    foreach (var it in uIElements)
                    {
                        if (it.Equals(uIElementTopLeft))
                            continue;
                        TableAerea.DataElements.Remove(it);
                    }
                }
            }
            else
            {
                Grid grid = new Grid();
                Grid.SetRow(grid, r);
                Grid.SetColumn(grid, c);
                Grid.SetRowSpan(grid, rs);
                Grid.SetColumnSpan(grid, cs);
                TableAerea.ControlAction = ListControlAction.ChildrenChange;
                TableAerea.Children.Add(grid);
                TableAerea.ControlAction = ListControlAction.None;
            }


        }

        //去除用于区域选择和行列选择的装饰元素，在进行任何的数据合并，行/列增删都需要调用该方法
        public void ResetSelection()
        {
            if (TableAerea.Children.Contains(SelectCR))
                TableAerea.Children.Remove(SelectCR);
            if (TableAerea.Children.Contains(SelectAerea))
                TableAerea.Children.Remove(SelectAerea);
        }

        //检测坐标区域是否已经存在数据
        private bool IsReapte(int row, int column)
        {
            var valuePair1 = TableAerea.DataElements.Where(x => Grid.GetRow(x) == row && Grid.GetColumn(x) == column);
            if (valuePair1.Count() != 0)
                return true;
            return false;
        }

        //求交集
        ValuePair<int, int> Intersection(ValuePair<int, int> pair1, ValuePair<int, int> pair2)
        {
            if (pair2.Frist > pair1.Second || pair1.Frist > pair2.Second)
                return null;//无交集
            ValuePair<int, int> pair = new ValuePair<int, int>();
            pair.Frist = pair1.Frist;
            pair.Second = pair1.Second;
            if (pair1.Frist <= pair2.Frist)
                pair.Frist = pair2.Frist;
            if (pair2.Second <= pair1.Second)
                pair.Second = pair2.Second;
            return pair;
        }

        
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "ActualWidth":
                    InWidth = ActualWidth - 50f;
                    if (InWidth < 0)
                        InWidth = 0;
                    break;
                case "ActualHeight":
                    InHeight = ActualHeight - 50f;
                    if (InHeight < 0)
                        InHeight = 0;
                    break;
                default:
                    break;
            }
            base.OnPropertyChanged(e);
        }
    }
}
