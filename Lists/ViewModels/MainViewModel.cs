using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Lists.Data;

namespace Lists
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            Lists = new ObservableCollection<ListViewModel>();
            Lists.CollectionChanged += (s, o) => { NotifyPropertyChanged("Lists"); };
        }

        public ObservableCollection<ListViewModel> Lists { get; private set; }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public void LoadData()
        {
            ApiService api = new ApiService();
            api.GetLists(Lists);
            UpdateData();
        }

        public void UpdateData()
        {
            ApiService web = new ApiService();
            web.UpdateListItemsFailed += (error) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(error.Message);
                });
            };

            //Create UpdateAllListsAsync()
            foreach (ListViewModel list in Lists)
                web.UpdateListAsync(list);

            IsDataLoaded = true;
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
    }
}