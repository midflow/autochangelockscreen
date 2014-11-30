using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace AutoChangeLockScreen
{
    public partial class Help : PhoneApplicationPage
    {
        public Help()
        {
            InitializeComponent();
            if (!Utils.ShowAds)
            {
                //ApplicationBar.MenuItems.RemoveAt(0);
                RowAds.Height = new GridLength(0);
            }
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UpdateAd();
        }
        public void UpdateAd()
        {
            AdView.Visibility = Utils.ShowAds ? Visibility.Visible : Visibility.Collapsed;

            // if we add more of these, we'll need to be more clever here
            if (!Utils.ShowAds)
            {
                //ApplicationBar.MenuItems.RemoveAt(0);
                RowAds.Height = new GridLength(0);
                btnBuy.Visibility = Visibility.Collapsed;
            }
            //else
            //{
            //    //Render_Ad();
            //    Display_Ad();
            //}
        }
        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();

            marketplaceDetailTask.ContentIdentifier = "ee8e9449-61b1-4049-9ca4-5407995234ab";
            marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;

            marketplaceDetailTask.Show();
        }
    }
}