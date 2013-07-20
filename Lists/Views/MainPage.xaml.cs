using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Lists
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
                if (App.ViewModel.Lists.Count > 0)
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
            }
        }

        private void RefreshList_Click(object sender, System.EventArgs e)
        {
            App.ViewModel.UpdateData();
        }

        private void NewItem_Click(object sender, EventArgs e)
        {
            ListViewModel list = (ListViewModel)pvtLists.SelectedItem;
            list.CreateItem("test");
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/SettingsPage.xaml", UriKind.Relative));
        }
    }
}