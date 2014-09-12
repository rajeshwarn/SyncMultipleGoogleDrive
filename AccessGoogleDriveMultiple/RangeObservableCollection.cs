using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SyncMultipleGoogleDrives
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification;
        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }
        protected virtual void OnCollectionChangedMultiItem(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handlers = CollectionChanged;
            if (handlers != null)
            {
                foreach (NotifyCollectionChangedEventHandler handler in handlers.GetInvocationList())
                {
                    if (handler.Target is CollectionView)
                        ((CollectionView)handler.Target).Refresh();
                    else
                        handler(this, e);
                }
            }
        }
        public void AddRange(IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            _suppressNotification = true;
            foreach (T item in list)
            {
                Add(item);
            }
            _suppressNotification = false;

            NotifyCollectionChangedEventArgs obEvtArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                list as System.Collections.IList);
            OnCollectionChangedMultiItem(obEvtArgs);
        }
    }
}
