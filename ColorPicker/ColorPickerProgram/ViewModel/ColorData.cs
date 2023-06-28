using ColorPickerPluys.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ColorPicker.ViewModel
{
    class VM : ViewModelBase, ICustomTypeDescriptor
    {
        Dictionary<string, object> propertysClass = new Dictionary<string, object>();
        Dictionary<string, object> propertyUnul = new Dictionary<string, object>();
        public object this[string name]
        {
            get => propertyUnul[name];
            set
            {
                propertyUnul[name] = value;
                RaisePropertyChanged(name);
            }
        }

        //向viewmodel添加model
        //第一个参数为数据所属类class或者数据本身
        //第二参数为数据名称：名称需要与属性名称相同
        //本类会数据的更改进行通知
        public void Add(object target, string name)
        {
            if(!(target is ViewModelBase))
                propertyUnul.Add(name, target);
            else
            {
                ((ViewModelBase)target).PropertyChanged += Target_PropertyChanged;
                propertysClass.Add(name, target);
            }
        }

        //将各个数据更新事件统一到viewModel上
        private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        #region 以下为了重写，用不到
        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);

        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return null;
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }
        #endregion

        //将model数据映射到PropertyDescriptor上
        public PropertyDescriptorCollection GetProperties()
        {
            List<PropertyDescriptor> proList = new List<PropertyDescriptor>();

            foreach (var key in propertysClass.Keys)
            {
                proList.Add(new VMPropertyDescriptor(key, propertysClass[key], new Attribute[1]));
            }
            foreach (var key in propertyUnul.Keys)
            {
                proList.Add(new VMPropertyDescriptor(key, propertyUnul[key], new Attribute[1], true));
            }
            return new PropertyDescriptorCollection(proList.ToArray());
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {

            List<PropertyDescriptor> proList = new List<PropertyDescriptor>();
            foreach (var key in propertysClass.Keys)
            {
                proList.Add(new VMPropertyDescriptor(key, propertysClass[key], attributes));
            }
            foreach (var key in propertyUnul.Keys)
            {
                proList.Add(new VMPropertyDescriptor(key, propertyUnul[key], attributes, true));
            }
            return new PropertyDescriptorCollection(proList.ToArray());

        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
    }

    //该类用于绑定，更新上
    internal sealed class VMPropertyDescriptor : PropertyDescriptor
    {
        readonly string Namex;
        object target;
        Type type;
        bool isLocalVM = false;
        //构造函数参数中的name需要与xaml中binding字符串相同，也需要与model的raiseproperty参数中字符串相同
        //isLocalVM代表是vm类中本地数据还是外部类数据，false代表外部类数据，true代表vm本地数据
        public VMPropertyDescriptor(string name, object target, Attribute[] attributes, bool isLocalVM = false) : base(name, attributes)
        {
            this.isLocalVM = isLocalVM;
            if (isLocalVM)
                type = null;
            else
                type = target.GetType();
            this.Namex = name;
            this.target = target;
        }
        public override Type ComponentType => null;

        public override bool IsReadOnly => false;

        public override Type PropertyType => isLocalVM ? target.GetType() : type.GetProperty(Namex).PropertyType;

        public override bool CanResetValue(object component)
        {
            return false;
        }

        //该方法会引起componentChanging和该方法会引起componentChanged事件
        public override object GetValue(object component)
        {
            return isLocalVM ? (component as VM)[Namex] : type.GetProperty(Namex).GetValue(target);
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            if (isLocalVM)
                (component as VM)[Namex] = value;
            else
                type.GetProperty(Namex).SetValue(target, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

    }
   
}
