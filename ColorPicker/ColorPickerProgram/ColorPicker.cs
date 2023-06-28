using ColorPickerPluys.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorPickerPluys
{

    //注意目前全局取色器会影响系统性能
    class ColorPicker : ViewModelBase
    {
        private bool isStart = false;
        private Brush brush = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
      
        public Brush Brush
        {
            get => brush;
            set
            {
                brush = value;
                RaisePropertyChanged("Brush");
            }
        }
        public ColorPicker()
        {
            MouseMoveEvent += ColorPicker_MouseMoveEvent;
            MouseClickEvent += ColorPicker_MouseClickEvent;
        }

        
        private void ColorPicker_MouseClickEvent(object sender, MouseEventArgs e)
        {
            if (isStart)
            {
                Stop();
            }
        }

        private void ColorPicker_MouseMoveEvent(object sender, int x, int y)
        {
            Brush = new SolidColorBrush(GetPixeColor(x, y));
        }

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        //安装钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        //卸载钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        //调用下一个钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int x;
            public int y;

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MouseLParam
        {
            public POINT pt;
            public IntPtr hWnd;
            public int wHitTestCode;
            public int dwExtraInfo;

        }

        private int hHook;
        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDWON = 0x0201;
        private const int WM_RBUTTONDWON = 0x0204;

        public HookProc hookProc;
        public delegate void MouseMoveHandler(object sender, int x, int y);
        public event MouseMoveHandler MouseMoveEvent;

        public delegate void MouseClickHandler(object sender, MouseEventArgs e);
        public event MouseClickHandler MouseClickEvent;
        public int Start()
        {
            if (isStart)
                return -1;
            hookProc = new HookProc(MouseHookcProc);
            hHook = SetWindowsHookEx(WH_MOUSE_LL, hookProc, IntPtr.Zero, 0);        //全局钩子
            isStart = true;
            return hHook;
        }

        public void Stop()
        {
            UnhookWindowsHookEx(hHook);
            isStart = false;
        }

        //该钩子函数对外屏蔽了鼠标点击，仍然传递鼠标移动事件
        private int MouseHookcProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            MouseLParam mouseLParam = (MouseLParam)Marshal.PtrToStructure(lParam, typeof(MouseLParam));
            if (nCode < 0)
                goto next;
            switch ((int)wParam)
            {
                case WM_RBUTTONDWON:
                    {
                        if (MouseClickEvent != null)
                        {
                            var e = new MouseEventArgs(Mouse.PrimaryDevice, 0);
                            MouseClickEvent(this, e);
                            return 1;
                        }
                    }
                    break;
                case WM_LBUTTONDWON:
                    {
                        if (MouseClickEvent != null)
                        {
                            var e = new MouseEventArgs(Mouse.PrimaryDevice, 0);
                            MouseClickEvent(this, e);
                            return 1;
                        }
                    }
                    break;
                case WM_MOUSEMOVE:
                    {
                        if (MouseMoveEvent != null)
                        {
                            MouseMoveEvent(this, mouseLParam.pt.x, mouseLParam.pt.y);

                        }
                    }
                    break;
            }
            next:
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        private static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hwnd, int xPos, int yPos);
        public static Color GetPixeColor(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, x, y);
            ReleaseDC(IntPtr.Zero, hdc);
            Color color = Color.FromRgb((byte)(pixel & 0x000000FF), (byte)((pixel & 0x0000FF00) >> 8), (byte)((pixel & 0x00FF00F00) >> 16));
            return color;
        }
        struct MPoint
        {
            public int X;
            public int Y;
            public MPoint(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        public static Color GetColor(Point point)
        {
            return GetPixeColor((int)point.X, (int)point.Y);
        }
    }
}
