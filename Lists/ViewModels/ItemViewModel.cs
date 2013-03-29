using System;
using System.ComponentModel;
using System.Data.Linq.Mapping;

namespace Lists
{
    [Table]
    public class ItemViewModel : INotifyPropertyChanged
    {
        private ulong _id;
        private string _name;
        private bool _needed;
        private bool _deleted;

        public ItemViewModel() { }

        public ItemViewModel(ulong id, string name, bool needed, bool deleted)
        {
            _id = id;
            _name = name;
            _needed = needed;
            _deleted = deleted;
        }

        #region Getters and Setters

        [Column(IsPrimaryKey = true, CanBeNull = false)]
        public ulong Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        [Column]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        [Column]
        public bool Needed
        {
            get
            {
                return _needed;
            }
            set
            {
                if (value != _needed)
                {
                    _needed = value;
                    NotifyPropertyChanged("Needed");
                }
            }
        }

        [Column]
        public bool Deleted
        {
            get
            {
                return _deleted;
            }
            set
            {
                if (value != _deleted)
                {
                    _deleted = value;
                    NotifyPropertyChanged("Deleted");
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