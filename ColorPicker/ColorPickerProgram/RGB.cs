using ColorPickerPluys.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorPickerPluys
{
    class RGB: ViewModelBase
    {
        private int r;
        private int g;
        private int b;
        public int R { 
            get => r;
            set { r = value; RaisePropertyChanged("R"); }
        }
        public int G {
            get => g;
            set { g = value; RaisePropertyChanged("G"); }
        }
        public int B {
            get => b;
            set { b = value; RaisePropertyChanged("B"); }
        }
    }
}
