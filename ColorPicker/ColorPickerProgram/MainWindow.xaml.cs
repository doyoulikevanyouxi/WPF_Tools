using ColorPicker.ViewModel;
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

namespace ColorPickerPluys
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        VM vM = new VM();
        ColorPicker CP = new ColorPicker();
        RGB colorRgb = new RGB();
        Brush ColorCurrent = Brushes.Transparent;
        Brush ColorOrigin = Brushes.Transparent;
        //该值固定
        double colorRectHeight;//颜色渐变矩形高度
        double colorRectWidth; //颜色渐变矩形宽度


        //颜色选择器选择的颜色
        byte Rr = 0xff;
        byte Gg = 0x00;
        byte Bb = 0x00;
        byte Aa = 0xff;

        //原始RGB---12色相
        byte Ro = 0xff;
        byte Go = 0x00;
        byte Bo = 0x00;
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            vM.Add(Rr, "ColorR");
            vM.Add(Gg, "ColorG");
            vM.Add(Bb, "ColorB");
            vM.Add(Aa, "ColorA");
            vM.Add(ColorCurrent, "ColorCurrent");
            vM.Add(ColorCurrent, "ColorOrigin");
           
            this.DataContext = vM;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            colorRectHeight = xfd.ActualHeight;
            colorRectWidth = xfd.ActualWidth;
            CreateColors();
        }

        #region 取色器
        //颜色样本r,g,b原点在右上角，向下减少，向左增加，参照VS的颜色选择器
        //实际上创建的是一个高度为colorRectHeight，宽度为colorRectWidth的矩形，背景色为渐变
        private void CreateColors()
        {
             
            double hei = colorRectHeight / 256.0; //位数是256，因为12色相作为初始颜色，RGB中其中一个值必定是255，那么一定有0-255的变化，因此一定有256个矩形渐变区域
            byte r = 0xff;
            byte g = 0xff;
            byte b = 0xff;
            byte Rn = Ro;
            byte Gn = Go;
            byte Bn = Bo;

            xfd.Children.Clear();

            for (int i = 1; i <= 256; ++i)
            {

                System.Windows.Shapes.Rectangle grid = new System.Windows.Shapes.Rectangle()
                {
                    Height = hei,
                    Fill = new System.Windows.Media.LinearGradientBrush(new System.Windows.Media.Color() { R = r--, G = g--, B = b--, A = Aa }, new System.Windows.Media.Color() { R = Rn, G = Gn, B = Bn, A = Aa }, new System.Windows.Point(0, 0.5), new System.Windows.Point(1, 0.5))
                };

                if (Rn != 0)
                    Rn--;
                if (Gn != 0)
                    Gn--;
                if (Bn != 0)
                    Bn--;
                xfd.Children.Add(grid);

            }
        }

        #region 与12色相图有关
        //使slider控件随鼠标移动而改变值
        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            if ((sender as UIElement).IsMouseCaptured)
            {
                var value = ColorSelectorSlider.Maximum - (e.GetPosition(ColorSelectorSlider).Y / ColorSelectorSlider.ActualHeight * ColorSelectorSlider.Maximum);
                ColorSelectorSlider.Value = value;
            }
        }

        private void ColorSelectorSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var value = ColorSelectorSlider.Maximum - (e.GetPosition(ColorSelectorSlider).Y / ColorSelectorSlider.ActualHeight * ColorSelectorSlider.Maximum);
            ColorSelectorSlider.Value = (int)value;
            (sender as UIElement).CaptureMouse();
            e.Handled = true;
        }

        private void ColorSelectorSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).ReleaseMouseCapture();
            e.Handled = true;
        }


        //12色相主变换以及slider控件区域和位置计算
        private void SliderColorS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int tem = (int)ColorSelectorSlider.Value;
            int area = tem / 255;
            int loca = tem % 255;
            switch (area)
            {
                case 0:
                    {
                        Ro = 0xff;
                        Go = 0x00;
                        Bo = (byte)loca;
                    }
                    break;
                case 1:
                    {

                        Ro = (byte)(0xff - loca);
                        Go = 0x00;
                        Bo = 0xff;

                    }
                    break;
                case 2:
                    {
                        Ro = 0x00;
                        Go = (byte)loca;
                        Bo = 0xff;

                    }
                    break;
                case 3:
                    {
                        Ro = 0x00;
                        Go = 0xff;
                        Bo = (byte)(0xff - loca);

                    }
                    break;
                case 4:
                    {
                        Ro = (byte)loca;
                        Go = 0xff;
                        Bo = 0x00;

                    }
                    break;
                case 5:
                    {
                        Ro = 0xff;
                        Go = (byte)(0xff - loca);
                        Bo = 0x00;

                    }
                    break;
                default:
                    {

                        Ro = 0xff;
                        Go = 0x00;
                        Bo = 0x00;

                    }
                    break;
            }

            Canvas.SetLeft(ColorSelector, xfd.ActualWidth - 5);
            Canvas.SetTop(ColorSelector, 0 - 5);
           
            vM["ColorCurrent"] = new SolidColorBrush(Color.FromArgb(Aa, Ro, Go, Bo));
            vM["ColorOrigin"] = new SolidColorBrush(Color.FromArgb(Aa, Ro, Go, Bo));
            vM["ColorR"] = Ro;
            vM["ColorG"] = Go;
            vM["ColorB"] = Bo;
            CreateColors();
        }

        #endregion
        //设置透明度
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Aa = (byte)((float)vM["ColorA"] * 255f);
            CreateColors();
        }

        #region 颜色双向渐变选择器

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            Canvas canvas = sender as Canvas;
            System.Windows.Point point = Mouse.GetPosition(canvas);
         
            Canvas.SetLeft(ColorSelector, point.X - 5);
            Canvas.SetTop(ColorSelector, point.Y - 5);
            SetCurrentColor(point);
            (sender as UIElement).CaptureMouse();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {

            if (!((sender as UIElement).IsMouseCaptured))
                return;
       
            Canvas canvas = sender as Canvas;
            System.Windows.Point point = Mouse.GetPosition(canvas);
            if (point.X < 0)
                point.X = 0;
            if (point.Y < 0)
                point.Y = 0;
            if (point.X > colorRectWidth)
                point.X = colorRectWidth;
            if (point.Y > colorRectHeight)
                point.Y = colorRectHeight;
            Canvas.SetLeft(ColorSelector, point.X - 5);
            Canvas.SetTop(ColorSelector, point.Y - 5);
            SetCurrentColor(point);
        }
        private void Canvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).ReleaseMouseCapture();
        }
        private void SetCurrentColor(System.Windows.Point point)
        {
            //下列公式原理如下 计算y坐标下占比--计算y坐标下rgb值的减少值-----计算x坐标占比-----在y坐标减少值下计算x坐标rgb值
            //255 -------------------------- Rr
            //254 -------------------------- Rr--
            // |                              |     \
            // |                              |      \   y/height*Rr
            // |                              |      /        |
            // |                              |     /         |
            // |  -----------(x,y)----------  |    ------>(1-y/height)*Rr R的右边Y值边界，左边计算同理
            // |                              |
            // |                              |
            // 0                              0
            //    \          /
            //     \        /
            //      \      /
            //       \    /
            //        \  /
            //         \/
            //         x/width*(左边界-右边界) -------------->那么该点的值=左边界- x/width*(左边界-右边界)


            //逆向考虑的边界问题太多就暂不会实现
            double Yradio = 1f - point.Y / colorRectHeight;
            double Xradio = point.X / colorRectWidth;
            Rr = (byte)(Yradio * 255f - Yradio * (255f - Ro) * Xradio);
            Gg = (byte)(Yradio * 255f - Yradio * (255f - Go) * Xradio);
            Bb = (byte)(Yradio * 255f - Yradio * (255f - Bo) * Xradio);
            vM["ColorCurrent"] = new SolidColorBrush(Color.FromArgb(Aa, Rr, Gg, Bb));
            vM["ColorR"] = Rr;
            vM["ColorG"] = Gg;
            vM["ColorB"] = Bb;
        }
        #endregion

        private void OriginColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Canvas.SetLeft(ColorSelector, xfd.ActualWidth - 5);
            Canvas.SetTop(ColorSelector, 0 - 5);
            vM["ColorCurrent"] = vM["ColorOrigin"];
        }

        #endregion

        private void StartColorPick(object sender, RoutedEventArgs e)
        {
            CP.MouseMoveEvent += CP_MouseMoveEvent;
            CP.Start();
        }

       

        private void CP_MouseMoveEvent(object sender, int x, int y)
        {
           
            vM["ColorCurrent"] = CP.Brush;
        }
    }
}
