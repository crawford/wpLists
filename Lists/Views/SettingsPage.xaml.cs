using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Lists.Data;

namespace Lists.Views
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void Subscribe_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ApiService web = new ApiService();
            Guid listGuid;

            try
            {
                listGuid = new Guid(txtListGuid.Text);
            }
            catch (Exception error)
            {
                ListSubscribeFailed(error);
                return;
            }

            ListViewModel list = web.CreateList(listGuid, "?");

            web.UpdateListItemsCompleted += () =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    App.ViewModel.Lists.Add(list);
                    txtListGuid.Text = "";
                    txtListGuid.IsEnabled = true;
                    btnSubscribe.IsEnabled = true;
                });
            };
            web.UpdateListItemsFailed += ListSubscribeFailed;

            txtListGuid.IsEnabled = false;
            btnSubscribe.IsEnabled = false;

            web.UpdateListAsync(list);
        }

        void ListSubscribeFailed(Exception error)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                txtListGuid.IsEnabled = true;
                btnSubscribe.IsEnabled = true;
                MessageBox.Show(error.Message);
            });
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            ApiService api = new ApiService();
            ListViewModel list = api.CreateList(Guid.NewGuid(), txtListName.Text);
            App.ViewModel.Lists.Add(list);
            txtListName.Text = "";
        }
    }
}