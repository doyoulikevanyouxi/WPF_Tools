using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Table.Control
{
    public class ObjectCollection<T> : INotifyCollectionChanged, IList<T>, IList
    {

        private ObservableCollection<T> RCCollection;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public ObjectCollection()
        {

            RCCollection = new ObservableCollection<T>();
            RCCollection.CollectionChanged += RCCollection_CollectionChanged;
        }
        private void RCCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged(this, e);
        }
        public T this[int index] { get => ((IList<T>)RCCollection)[index]; set => ((IList<T>)RCCollection)[index] = value; }

        public int Count => ((IList<T>)RCCollection).Count;

        public bool IsReadOnly => ((IList<T>)RCCollection).IsReadOnly;

        public bool IsFixedSize => ((IList)RCCollection).IsFixedSize;

        public object SyncRoot => ((IList)RCCollection).SyncRoot;

        public bool IsSynchronized => ((IList)RCCollection).IsSynchronized;

        object IList.this[int index] { get => ((IList)RCCollection)[index]; set => ((IList)RCCollection)[index] = value; }


        //删除从index开始，总共count个数据
        public virtual bool RemoveRange(int index, int count)
        {

            if (index >= RCCollection.Count || index + count > RCCollection.Count)
                return false;
            for (int i = 0; i < count; ++i)
                RCCollection.RemoveAt(index);
            return true;
        }


        #region 泛型
        public void Add(T item)
        {
            ((IList<T>)RCCollection).Add(item);
        }

        public void Clear()
        {
            ((IList<T>)RCCollection).Clear();
        }

        public bool Contains(T item)
        {
            return ((IList<T>)RCCollection).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((IList<T>)RCCollection).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>)RCCollection).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)RCCollection).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)RCCollection).Insert(index, item);
        }

        public bool Remove(T item)
        {
            return ((IList<T>)RCCollection).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)RCCollection).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<T>)RCCollection).GetEnumerator();
        }
        #endregion

        #region 非泛型
        public int Add(object value)
        {
            return ((IList)RCCollection).Add(value);
        }

        public bool Contains(object value)
        {
            return ((IList)RCCollection).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)RCCollection).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            ((IList)RCCollection).Insert(index, value);
        }

        public void Remove(object value)
        {
            ((IList)RCCollection).Remove(value);
        }

        public void CopyTo(Array array, int index)
        {
            ((IList)RCCollection).CopyTo(array, index);
        }
        #endregion
    }
}
