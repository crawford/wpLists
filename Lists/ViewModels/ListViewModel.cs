using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Linq.Mapping;
//TODO Add one to many relation to list items
namespace Lists
{
    [Table]
    public class ListViewModel : INotifyPropertyChanged
    {
        private Guid _id;
        private string _title;
        private ObservableCollection<ItemViewModel> _items;

        public ListViewModel() { }

        public ListViewModel(Guid id, string title, ObservableCollection<ItemViewModel> items)
        {
            _id = id;
            _title= title;
            _items = items;
        }

        #region Getters and Setters

        [Column(IsPrimaryKey = true, CanBeNull = false)]
        public Guid Id
        {
            get {  return _id; }
            set { _id = value; }
        }

        [Column]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        public ObservableCollection<ItemViewModel> Items
        {
            get
            {
                return _items;
            }
            set
            {
                if (value != _items)
                {
                    _items = value;
                    NotifyPropertyChanged("Items");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}