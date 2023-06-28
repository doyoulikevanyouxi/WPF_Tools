using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PCUH.Controls.TableConrol
{
    #region 自定义数据，存储二维有序数据
    public class ValuePair<T, T2>
    {
        T first;
        T2 second;
        public T Frist
        {
            get => first;
            set => first = value;
        }
        public T2 Second
        {
            get => second;
            set => second = value;
        }
        public ValuePair() { }
        public ValuePair(T valueF, T2 valueS)
        {
            first = valueF;
            second = valueS;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator ==(ValuePair<T, T2> valuePair, ValuePair<T, T2> valuePair2)
        {
            if ((object)valuePair == null && (object)valuePair2 == null)
                return true;
            if (((object)valuePair == null && (object)valuePair2 != null) || (object)valuePair != null && (object)valuePair2 == null)
                return false;
            if (valuePair.first.Equals(valuePair2.first) && valuePair.second.Equals(valuePair2.second))
                return true;
            return false;
        }

        public static bool operator !=(ValuePair<T, T2> valuePair, ValuePair<T, T2> valuePair2)
        {
            if (valuePair == valuePair2)
                return false;
            return true;
        }
    }
    public class ValuePairs<T, T2> : IEnumerable, IEnumerable<ValuePair<T, T2>>, IList, ICollection
    {

        List<ValuePair<T, T2>> lvs = new List<ValuePair<T, T2>>();

        public int Count
        {
            get => lvs.Count;
        }

        public bool IsReadOnly => ((IList)lvs).IsReadOnly;

        public bool IsFixedSize => ((IList)lvs).IsFixedSize;

        public object SyncRoot => ((IList)lvs).SyncRoot;

        public bool IsSynchronized => ((IList)lvs).IsSynchronized;

        object IList.this[int index] { get => ((IList)lvs)[index]; set => ((IList)lvs)[index] = value; }
        public ValuePair<T, T2> this[int index]
        {
            get => lvs[index];
        }
        public void Add(T valueF, T2 valueS)
        {
            lvs.Add(new ValuePair<T, T2>(valueF, valueS));
        }

        public void Add(ValuePair<T, T2> value)
        {
            lvs.Add(value);
        }

        public bool Has(ValuePair<T, T2> value)
        {
            foreach (var item in lvs)
            {
                if (value == item)
                    return true;
            }
            return false;
        }

        public IEnumerator GetEnumerator()
        {
            return lvs.GetEnumerator();
        }

        IEnumerator<ValuePair<T, T2>> IEnumerable<ValuePair<T, T2>>.GetEnumerator()
        {
            return ((IEnumerable<ValuePair<T, T2>>)lvs).GetEnumerator();
        }



        public int Add(object value)
        {
            return ((IList)lvs).Add(value);
        }

        public void Clear()
        {
            ((IList)lvs).Clear();
        }
        public bool Contains(object obj)
        {
            return ((IList)lvs).Contains(obj);
        }

        public int IndexOf(object value)
        {
            return ((IList)lvs).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)lvs).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)lvs).Remove(value);
        }

        public void RemoveAt(int index)
        {
            ((IList)lvs).RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            ((IList)lvs).CopyTo(array, index);
        }
    }
    #endregion

    public enum ListControlAction
    {
        RowColumnChange = 1,
        ChildrenChange = 2,
        ClearBorder = 3,
        CreateBoder = 4,
        None = 5
    }

    class TableInner : Grid
    {

        public static readonly DependencyProperty ShowBorderProperty = DependencyProperty.Register("ShowBorder", typeof(bool), typeof(TableInner), new PropertyMetadata(OnShowBorderPropertyChanged));

        private int rows;
        private int columns;
        private int addLocation;
        private int addCount;
        private List<GridSplitter> GridSplitterCollection;
        private List<Border> BoderCollection;

        private delegate int ReflecFunGet(UIElement uIElement);
        private delegate void ReflecFunSet(UIElement uIElement, int idex);
        public delegate void CRChangedDelegate(int index, int setValue, bool isHorizontal, bool isRemove = false);
        public delegate void ChildChangedDelegate();
        public event CRChangedDelegate CRChanged;
        private ReflecFunGet GetValueRC;
        private ReflecFunSet SetValueRC;
        private ReflecFunGet GetValueRCS;
        private ReflecFunSet SetValueRCS;
        public bool ShowBorder
        {
            get => (bool)base.GetValue(ShowBorderProperty);
            set => SetValue(ShowBorderProperty, value);
        }
        public List<ValuePairs<int, int>> CombinedArea { get; private set; }
        public ListControlAction ControlAction { get;  set; }
        public ObjectCollection<UIElement> DataElements { get; }

        //该数据隐藏了Grid的三个数据
        //隐藏原因:该数据类型集合未提供更改通知
        public new ObjectCollection<RowDefinition> RowDefinitions { get; }
        public new ObjectCollection<ColumnDefinition> ColumnDefinitions { get; }
        public new ObjectCollection<UIElement> Children { get; }
        public TableInner()
        {

            addLocation = -1;
            addCount = 0;
            ControlAction = ListControlAction.None;
            SnapsToDevicePixels = true;
            BoderCollection = new List<Border>();
            GridSplitterCollection = new List<GridSplitter>();
            RowDefinitions = new ObjectCollection<RowDefinition>();
            ColumnDefinitions = new ObjectCollection<ColumnDefinition>();
            Children = new ObjectCollection<UIElement>();
            DataElements = new ObjectCollection<UIElement>();

            CRChanged += CRChangedCallBack;
            Loaded += ListControl_Loaded;
            RowDefinitions.CollectionChanged += RowDefinitions_CollectionChanged;
            ColumnDefinitions.CollectionChanged += ColumnDefinitions_CollectionChanged;
            Children.CollectionChanged += Children_CollectionChanged;
            DataElements.CollectionChanged += DataElements_CollectionChanged;
        }

        #region 计算边框数据并返回边框
        /// <summary>
        /// 求合并区域
        /// 该方法的计算方式要求由该类添加的非数据元素（不存储在DataElement）连续
        /// </summary>
        /// <returns></returns>
        private List<ValuePairs<int, int>> GetEffecArea()
        {
            List<ValuePairs<int, int>> CombinedArea = new List<ValuePairs<int, int>>();
            ValuePairs<int, int> RvalueParis = new ValuePairs<int, int>();
            ValuePairs<int, int> CvalueParis = new ValuePairs<int, int>();
            for (int i = 0; i < Children.Count; ++i)
            {
                if (i >= addLocation && i < addLocation + addCount)
                    continue;
                var r = Grid.GetRow(Children[i]);
                var c = Grid.GetColumn(Children[i]);

                var rs = Grid.GetRowSpan(Children[i]);
                var cs = Grid.GetColumnSpan(Children[i]);
                if (rs == 1 && cs == 1)
                    continue;
                RvalueParis.Add(r, rs);
                CvalueParis.Add(c, cs);

            }

            CombinedArea.Add(RvalueParis);//为什么可以分开存储R与C的信息：r与c是一对，1对1的，不存在数据量差
            CombinedArea.Add(CvalueParis);
            return CombinedArea;
        }
        //获取边框
        private List<Border> GetBorders(int Rows, int Columns, bool isHorizental)
        {

            List<Border> borders1 = new List<Border>();
            CombinedArea = GetEffecArea();
            int valueA = Columns;
            int valueB = Rows;
            ValuePairs<int, int> valueC = CombinedArea[1]; //存储相同方向的合并数据  ex:如果要得到水平方向的线，那么该数据就是水平方向合并格子的水平数据：受影响的index&&水平方向影响的行数
            ValuePairs<int, int> valueD = CombinedArea[0]; //存储另一个方向的数据

            if (isHorizental)
            {
                valueA = Rows;
                valueB = Columns;
                valueC = CombinedArea[0];
                valueD = CombinedArea[1];
            }
            List<ValuePairs<int, int>> llPoints = new List<ValuePairs<int, int>>();

            for (int i = 0; i < valueA - 1; ++i)
            {
                bool isAdd = false;
                for (int j = 0; j < valueC.Count; ++j)
                {

                    if (i >= valueC[j].Frist && i < (valueC[j].Frist + valueC[j].Second))
                    {

                        if (valueB == 0)
                        {
                            break;
                        }
                        else
                        {
                            isAdd = true;
                            ValuePairs<int, int> valuePairs = new ValuePairs<int, int>();
                            if (i == (valueC[j].Frist + valueC[j].Second - 1))  //如果该行或列是合并格最后一个，那么在就添加该全长行/列
                                valuePairs.Add(0, valueB - 1);  //起点，终点  
                            else
                            {
                                //如果合并格初始是在行/列的头部，那么添加合并格之后的长度
                                //为什么采用另一组数据，各线长度与相反方向有关 ex:如果是计算水平方向线的数据，那么需要计算水平方向跨过了多长的列，所以要用第二组列数据来计算
                                if (valueD[j].Frist == 0)
                                {
                                    valuePairs.Add(valueD[j].Frist + valueD[j].Second, valueB - 1);  //起点，终点  

                                }
                                else
                                {
                                    valuePairs.Add(0, valueD[j].Frist - 1);
                                    if (valueD[j].Frist + valueD[j].Second != valueB)//当在最右边时，右边border超出了边界
                                        valuePairs.Add(valueD[j].Frist + valueD[j].Second, valueB - 1);

                                }
                            }
                            llPoints.Add(valuePairs);
                        }
                    }
                }
                if (isAdd)
                {
                    if (llPoints.Count > 1)
                    {
                        ValuePairs<int, int> ppp;
                        ppp = CommonBorderInfo(llPoints[0], llPoints[1]);

                        for (int k = 2; k < llPoints.Count; ++k)
                        {
                            ppp = CommonBorderInfo(ppp, llPoints[k]);
                        }
                        foreach (ValuePair<int, int> it in ppp)
                        {

                            if (isHorizental)
                            {
                                Border borders = new Border() { BorderThickness = new Thickness(0, 0, 0, 1), BorderBrush = new SolidColorBrush(Colors.Black) };
                                Grid.SetRow(borders, i);
                                Grid.SetColumn(borders, it.Frist);
                                Grid.SetColumnSpan(borders, (it.Second - it.Frist + 1));
                                borders.Tag = "H";
                                borders1.Add(borders);
                            }
                            else
                            {
                                Border borders = new Border() { BorderThickness = new Thickness(0, 0, 1, 0), BorderBrush = new SolidColorBrush(Colors.Black) };
                                Grid.SetColumn(borders, i);
                                Grid.SetRow(borders, it.Frist);
                                Grid.SetRowSpan(borders, it.Second - it.Frist + 1);
                                borders.Tag = "V";
                                borders1.Add(borders);
                            }

                        }
                    }
                    else if (llPoints.Count != 0)
                    {
                        foreach (ValuePair<int, int> it in llPoints[0])
                        {
                            if (isHorizental)
                            {
                                Border borders = new Border() { BorderThickness = new Thickness(0, 0, 0, 1), BorderBrush = new SolidColorBrush(Colors.Black) };
                                Grid.SetRow(borders, i);
                                Grid.SetColumn(borders, it.Frist);
                                Grid.SetColumnSpan(borders, (it.Second - it.Frist + 1));
                                borders.Tag = "H";
                                borders1.Add(borders);
                            }
                            else
                            {
                                Border borders = new Border() { BorderThickness = new Thickness(0, 0, 1, 0), BorderBrush = new SolidColorBrush(Colors.Black) };
                                Grid.SetColumn(borders, i);
                                Grid.SetRow(borders, it.Frist);
                                Grid.SetRowSpan(borders, it.Second - it.Frist + 1);
                                borders.Tag = "V";
                                borders1.Add(borders);
                            }
                        }
                    }
                    llPoints.Clear();
                    continue;
                }
                if (isHorizental)
                {
                    Border border = new Border() { BorderThickness = new Thickness(0, 0, 0, 1), BorderBrush = new SolidColorBrush(Colors.Black) };
                    Grid.SetRow(border, i);
                    Grid.SetColumnSpan(border, valueB == 0 ? 1 : valueB);
                    border.Tag = "H";
                    borders1.Add(border);
                }
                else
                {
                    Border border = new Border() { BorderThickness = new Thickness(0, 0, 1, 0), BorderBrush = new SolidColorBrush(Colors.Black) };
                    Grid.SetColumn(border, i);
                    Grid.SetRowSpan(border, valueB == 0 ? 1 : valueB);
                    border.Tag = "V";
                    borders1.Add(border);
                }
            }
            return borders1;
        }
        //添加网格边框
        private void AddGridBorder()
        {

            ControlAction = ListControlAction.CreateBoder;

            addLocation = Children.Count;
            foreach (var item in GetBorders(rows, columns, true))
            {
                Children.Add(item);
                BoderCollection.Add(item);
            }
            //由于合并区域是多次计算的所以要在该处表明添加的非数据元素的个数
            addCount = GridSplitterCollection.Count + BoderCollection.Count;
            foreach (var item in GetBorders(rows, columns, false))
            {
                Children.Add(item);
                BoderCollection.Add(item);
            }

            foreach (var border in BoderCollection)
            {
                int r = Grid.GetRow(border);
                int c = Grid.GetColumn(border);
                int rs = Grid.GetRowSpan(border);
                int cs = Grid.GetColumnSpan(border);
                if (r == rows - 1 && border.Tag.Equals("H"))
                    continue;
                if (c == columns - 1 && border.Tag.Equals("V"))
                    continue;
                GridSplitter gridSplitter;
                if (border.Tag.Equals("H"))
                    gridSplitter = new GridSplitter() { Height = 5, Background = new SolidColorBrush(Colors.Transparent), VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Stretch };
                else
                    gridSplitter = new GridSplitter() { Width = 5, Background = new SolidColorBrush(Colors.Transparent) };
                Grid.SetRow(gridSplitter, r);
                Grid.SetRowSpan(gridSplitter, rs);
                Grid.SetColumn(gridSplitter, c);
                Grid.SetColumnSpan(gridSplitter, cs);
                GridSplitterCollection.Add(gridSplitter);
            }

            Border border1 = new Border() { BorderThickness = new Thickness(1), BorderBrush = new SolidColorBrush(Colors.Black) };
            Grid.SetRowSpan(border1, rows == 0 ? 1 : rows);
            Grid.SetColumnSpan(border1, columns == 0 ? 1 : columns);
            BoderCollection.Add(border1);
            Children.Add(border1);
         
            foreach (var item in GridSplitterCollection)
            {
                Children.Add(item);
            }
            addCount = GridSplitterCollection.Count + BoderCollection.Count;
            ControlAction = ListControlAction.None;
        }

        //求集合的交集
        private static ValuePairs<int, int> CommonBorderInfo<T, T2>(ValuePairs<T, T2> VP1, ValuePairs<T, T2> VP2)
        {
            ValuePairs<int, int> points = new ValuePairs<int, int>();
            if (!typeof(int).Equals(typeof(T)))
                return null;
            foreach (ValuePair<int, int> po in VP1)
            {
                foreach (ValuePair<int, int> po2 in VP2)
                {
                    if (po2.Frist > po.Second || po2.Second < po.Frist)
                        continue;
                    ValuePair<int, int> point = new ValuePair<int, int>();
                    point.Frist = po.Frist;
                    point.Second = po2.Second;
                    if (po2.Frist >= po.Frist)
                    {
                        point.Frist = po2.Frist;
                    }
                    if (po2.Second >= po.Second)
                        point.Second = po.Second;
                    points.Add(point);
                }
            }
            return points;
        }
        #endregion


        public new void SetColumnSpan(UIElement element, int value)
        {

            Grid.SetColumnSpan(element, value);
            OnShowBorderPropertyChanged(this, new DependencyPropertyChangedEventArgs(ShowBorderProperty, false, true));
        }
        public new void SetRowSpan(UIElement element, int value)
        {

            Grid.SetRowSpan(element, value);
            OnShowBorderPropertyChanged(this, new DependencyPropertyChangedEventArgs(ShowBorderProperty, false, true));

        }
        //移动所有受影响的非该类添加的控件
        private List<UIElement> MoveUI(int index, int setValue, bool isHorizontal)
        {
            GetValueRC = Grid.GetColumn;
            SetValueRC = Grid.SetColumn;
            GetValueRCS = Grid.GetColumnSpan;
            SetValueRCS = Grid.SetColumnSpan;


            if (isHorizontal)
            {
                GetValueRC = Grid.GetRow;
                SetValueRC = Grid.SetRow;
                GetValueRCS = Grid.GetRowSpan;
                SetValueRCS = Grid.SetRowSpan;
            }

            List<UIElement> uIElements = new List<UIElement>();
            for (int i = 0; i < addLocation; ++i)
            {
                int itemL = GetValueRC(Children[i]);
                if (itemL == index)
                    uIElements.Add(Children[i]);
                if (itemL >= index)
                {
                    int d = itemL + setValue;
                    if (d < 0)
                        d = 0;
                    SetValueRC(Children[i], d);
                }
                else
                {
                    int span = GetValueRCS(Children[i]);
                    if (span + GetValueRC(Children[i]) - 1 >= index)
                        SetValueRCS(Children[i], span + setValue);
                }

            }
            for (int i = addLocation + addCount; i < Children.Count; ++i)
            {
                int itemL = GetValueRC(Children[i]);
                if (itemL == index)
                    uIElements.Add(Children[i]);
                if (itemL >= index)
                {
                    int d = itemL + setValue;
                    if (d < 0)
                        d = 0;
                    SetValueRC(Children[i], d);
                }
                else
                {
                    int span = GetValueRCS(Children[i]);
                    if (span + GetValueRC(Children[i]) - 1 >= index)
                        SetValueRCS(Children[i], span + setValue);
                }
            }
            return uIElements;
        }
        private void ListControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ShowBorder)
                return;
            rows = RowDefinitions.Count;
            columns = ColumnDefinitions.Count;
            //重写每列/行的大小，初始为*，当GridSplitter移动后会更改列/行大小的定义从-----  "*"  到-------  "实际大小*"
            foreach (var item in ColumnDefinitions)
                item.Width = new GridLength(ActualWidth / columns, GridUnitType.Star);
            foreach (var item in RowDefinitions)
                item.Height = new GridLength(ActualHeight / rows, GridUnitType.Star);
            Upate();
        }
        //坐标转换为RC坐标，如果坐标大于或小于范围，那么该值置为-1；
        public ValuePair<int, int> GetRCPoint(Point point)
        {
            ValuePair<int, int> valuePair = new ValuePair<int, int>(-1, -1);
            double leng = 0f;
            if (!(point.X < 0 || point.X > ActualWidth))
            {
                for (int i = 0; i < ColumnDefinitions.Count; ++i)
                {
                    leng += ColumnDefinitions[i].ActualWidth;
                    if (point.X < leng)
                    {
                        valuePair.Second = i;
                        break;
                    }
                }
            }
            leng = 0f;
            if (!(point.Y < 0 || point.Y > ActualHeight))
            {
                for (int i = 0; i < RowDefinitions.Count; ++i)
                {
                    leng += RowDefinitions[i].ActualHeight;
                    if (point.Y < leng)
                    {
                        valuePair.Frist = i;
                        break;
                    }
                }
            }

            return valuePair;
        }
        //返回单元格右下角坐标
        //如果RC其中一个为复数，那么该对应的坐标值为-1
        public Point GetPoint(ValuePair<int, int> valuePair)
        {
            Point point = new Point(-1, -1);
            double leng = 0f;
            if (!(valuePair.Frist < 0))
            {

                for (int i = 0; i <= valuePair.Frist; ++i)
                {
                    leng += RowDefinitions[i].ActualHeight;
                }
                point.Y = leng;

            }
            leng = 0f;
            if (!(valuePair.Second < 0))
            {
                for (int i = 0; i <= valuePair.Second; ++i)
                {
                    leng += ColumnDefinitions[i].ActualWidth;
                }
                point.X = leng;
            }
            return point;
        }
        protected void ClearBorder()
        {
            if (addLocation != -1)
            {
                ControlAction = ListControlAction.ClearBorder;
                Children.RemoveRange(addLocation, addCount);
                GridSplitterCollection.Clear();
                BoderCollection.Clear();
                addLocation = -1;
                addCount = 0;
                ControlAction = ListControlAction.None;
            }
        }
        protected void Upate()
        {
            ClearBorder();
            if (rows == 0 || columns == 0)
                return;
            AddGridBorder();
        }
        private void DataElements_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ControlAction = ListControlAction.ChildrenChange;
                    Children.Add((UIElement)e.NewItems[0]);
                    ControlAction = ListControlAction.None;
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ControlAction = ListControlAction.ChildrenChange;
                    //if()
                    //addLocation;
                    Children.Remove(e.OldItems[0]);
                    ControlAction = ListControlAction.None;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
            }
        }
        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    base.Children.Add((UIElement)e.NewItems[0]);
                    if (ControlAction != ListControlAction.ChildrenChange)
                        break;
                    if (!IsLoaded)
                        break;
                    Upate();
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    base.Children.RemoveAt(e.OldStartingIndex);
                    if (ControlAction != ListControlAction.ChildrenChange)
                        break;
                    if (!IsLoaded)
                        break;
                    if (e.OldStartingIndex < addLocation)
                        addLocation--;
                    Upate();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
            }
        }
        private void ColumnDefinitions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ListControl_CRChanged(e, false);

        }
        private void RowDefinitions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ListControl_CRChanged(e, true);

        }
        public void ListControl_CRChanged(NotifyCollectionChangedEventArgs e, bool isHorizental)
        {

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (isHorizental)
                        base.RowDefinitions.Insert(e.NewStartingIndex, (RowDefinition)e.NewItems[0]);
                    else
                        base.ColumnDefinitions.Insert(e.NewStartingIndex, (ColumnDefinition)e.NewItems[0]);
                    CRChanged(e.NewStartingIndex, 1, isHorizental);

                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (isHorizental)
                        base.RowDefinitions.RemoveAt(e.OldStartingIndex);
                    else
                        base.ColumnDefinitions.RemoveAt(e.OldStartingIndex);
                    CRChanged(e.OldStartingIndex, -1, isHorizental, true);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
            }


        }
        private void CRChangedCallBack(int index, int setValue, bool isHorizontal, bool isRemove = false)
        {
            if (!IsLoaded)
                return;
            rows = RowDefinitions.Count;
            columns = ColumnDefinitions.Count;

            if (isRemove)
            {
                UIElement uIElement = Children[addLocation];
                foreach (var item in MoveUI(index, setValue, isHorizontal))
                {
                    Children.Remove(item);
                }
                addLocation = Children.IndexOf(uIElement);
            }
            else
            {
                MoveUI(index, setValue, isHorizontal);
            }

            Upate();
        }
        protected static void OnShowBorderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TableInner ListControl = d as TableInner;
            if (!ListControl.IsLoaded)
                return;
            ListControl.ClearBorder();
            if ((bool)e.NewValue)
            {
                if (ListControl.rows == 0 || ListControl.columns == 0)
                    return;
                ListControl.AddGridBorder();
            }

        }
    }
}
