using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace SimpleMVVM.Collections
{
    public partial class BulkObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, PropertyChangedEventArgsCache.GetOrCreate(name));
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, T item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, T item, T oldItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, oldItem, index));
        }

        private void OnCollectionReset()
        {
            OnCountChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnCountChanged()
        {
            OnPropertyChanged("Count");
            OnItemsChanged();
        }

        private void OnItemsChanged()
        {
            OnPropertyChanged("Items[]");
        }

        protected override void ClearItems()
        {  
            base.ClearItems();
            OnCollectionReset();
        }

        public void BatchInsertItems(int index, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                InsertItem(index, item);
                index++;
            }
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            base.RemoveItem(index);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }



        public void BatchRemoveItems(int index, int count)
        {
            // Delete from the back to the front, less interface refresh
            foreach (int delete_index in Enumerable.Range(index, count).Reverse())
            {
                RemoveItem(delete_index);
            }
        }


        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}