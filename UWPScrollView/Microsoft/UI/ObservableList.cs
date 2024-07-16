using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Foundation.Collections;

namespace Microsoft.UI;

internal sealed class ObservableList<T> : ObservableCollection<T>, IList<T>, IObservableVector<T>
{
    public ObservableList()
    {
        this.CollectionChanged += ObservableList_CollectionChanged;
    }

    private void ObservableList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        var args = new VectorChangedEventArgs()
        {
            CollectionChange = e.Action switch
            {
                System.Collections.Specialized.NotifyCollectionChangedAction.Add => CollectionChange.ItemInserted,
                System.Collections.Specialized.NotifyCollectionChangedAction.Remove => CollectionChange.ItemRemoved,
                System.Collections.Specialized.NotifyCollectionChangedAction.Replace => CollectionChange.ItemChanged,
                System.Collections.Specialized.NotifyCollectionChangedAction.Move => CollectionChange.ItemChanged,
                System.Collections.Specialized.NotifyCollectionChangedAction.Reset => CollectionChange.Reset,
                _ => CollectionChange.Reset,
            },
            Index = (uint)e.NewStartingIndex
        };

        this.VectorChanged?.Invoke(this, null);
    }

    public event VectorChangedEventHandler<T> VectorChanged;

    public class VectorChangedEventArgs : IVectorChangedEventArgs
    {
        public CollectionChange CollectionChange { get; set; }

        public uint Index { get; set; }
    }
}