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
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();
            App.imgName = "";
            App.blLoadIamge = true;
        }

        private void btnMyApps_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceSearchTask mkpSearch = new MarketplaceSearchTask();
            mkpSearch.ContentType = MarketplaceContentType.Applications;
            mkpSearch.SearchTerms = "trunglt";

            mkpSearch.Show();
        }

        private void btnRate_Click(object sender, RoutedEventArgs e)
        {
            //MarketplaceReviewTask review = new MarketplaceReviewTask();
            //review.Show();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("http://www.facebook.com/midflow");
            webBrowserTask.Show(); 
        }

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("http://www.mid-news.com");
            webBrowserTask.Show(); 
        }

        private void hlbEmail_Click_1(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "Send from App";
            emailComposeTask.Body = "";
            emailComposeTask.To = "lttrungbk@yahoo.com";
            emailComposeTask.Cc = "trunglt@live.com";

            emailComposeTask.Show();
        }

        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {
            //MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();

            //marketplaceDetailTask.ContentIdentifier = "ee8e9449-61b1-4049-9ca4-5407995234ab";
            //marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;

            //marketplaceDetailTask.Show();
        }
    }
}