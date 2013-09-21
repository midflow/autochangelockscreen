#define DEBUG_AGENT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using AutoChangeLockScreen.Resources;
using Microsoft.Phone.Scheduler;
using Windows.Phone.System.UserProfile;
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.IO;
using AutoChangeLockScreen.Models;
using System.Windows.Media;


namespace AutoChangeLockScreen
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        //PeriodicTask periodicTask;
        //string periodicTaskName = "PeriodicAgent";
        public bool agentsAreEnabled = true;
        public static int NumberImage = 0;

        public MainPage()
        {
            InitializeComponent();
            //LoadImages_Loaded();
            //AppTitle.Text = AutoChangeLockScreen.Resources.AppResources.ApplicationTitle;
            // Sample code to localize the ApplicationBar
            BuildLocalizedApplicationBar();

            Loaded += new System.Windows.RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            adrotator.Invalidate();
        }
        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = App.GetColorFromHexString("FF08317B");
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 0.5;
            LocalizedButtonBar("/Assets/AppBar/favs.png", AppResources.Review, ReviewButton_Click);
            LocalizedButtonBar("/Assets/AppBar/appbar.share.rest.png", AppResources.BuyApp, BuyAppButton_Click);
            LocalizedButtonBar("/Assets/AppBar/appbar.questionmark.rest.png", AppResources.Help, HelpButton_Click);
            LocalizedButtonBar("/Assets/AppBar/appbar.status.rest.png", AppResources.About, AboutButton_Click);

            LocalizedMenuBar(AppResources.ShareThis, ShareThis_Click);
            LocalizedMenuBar(AppResources.SMSThis, SMSThis_Click);
            LocalizedMenuBar(AppResources.EmailThis, EmailThis_Click);
        }

        protected static Color GetColorFromHexString(string s)
        {
            // remove artifacts
            s = s.Trim().TrimStart('#');

            // only 8 (with alpha channel) or 6 symbols are allowed
            if (s.Length != 8 && s.Length != 6)
                throw new ArgumentException("Unknown string format!");

            int startParseIndex = 0;
            bool alphaChannelExists = s.Length == 8; // check if alpha canal exists            

            // read alpha channel value
            byte a = 255;
            if (alphaChannelExists)
            {
                a = System.Convert.ToByte(s.Substring(0, 2), 16);
                startParseIndex += 2;
            }

            // read r value
            byte r = System.Convert.ToByte(s.Substring(startParseIndex, 2), 16);
            startParseIndex += 2;
            // read g value
            byte g = System.Convert.ToByte(s.Substring(startParseIndex, 2), 16);
            startParseIndex += 2;
            // read b value
            byte b = System.Convert.ToByte(s.Substring(startParseIndex, 2), 16);

            return Color.FromArgb(a, r, g, b);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            while (this.NavigationService.BackStack.Any())
            {
                this.NavigationService.RemoveBackEntry();
            }
        }
        private void LocalizedButtonBar(string imgpath, string text, EventHandler function)
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton =
                new ApplicationBarIconButton(new
                Uri(imgpath, UriKind.Relative));
            appBarButton.Text = text;
            appBarButton.Click += function;
            ApplicationBar.Buttons.Add(appBarButton);
        }
        private void btnDefault_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/LoadDefaultImages.xaml", UriKind.Relative));
            });
        }
        private void btnYour_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/LoadImages.xaml", UriKind.Relative));
            });
        }
        private void btnRSS_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/LoadRssImages.xaml", UriKind.Relative));
            });
        }
        private void LocalizedMenuBar(string text, EventHandler function)
        {
            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItem =
                new ApplicationBarMenuItem(text);
            appBarMenuItem.Click += function;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }
        private void ReviewButton_Click(object sender, EventArgs e)
        {
            MarketplaceReviewTask review = new MarketplaceReviewTask();
            review.Show();
        }
        private void HelpButton_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/Help.xaml", UriKind.Relative));
            });
        }
        private void AboutButton_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
            });
        }
        private void MyAppsButton_Click(object sender, EventArgs e)
        {
            MarketplaceSearchTask mkpSearch = new MarketplaceSearchTask();
            mkpSearch.ContentType = MarketplaceContentType.Applications;
            mkpSearch.SearchTerms = "trunglt";

            mkpSearch.Show();
        }
        private void BuyAppButton_Click(object sender, EventArgs e)
        {
            MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();

            marketplaceDetailTask.ContentIdentifier = "ee8e9449-61b1-4049-9ca4-5407995234ab";
            marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;

            marketplaceDetailTask.Show();
        }
        private void ShareThis_Click(object sender, EventArgs e)
        {
            ShareLinkTask shareLinkTask = new ShareLinkTask();

            shareLinkTask.Title = "Auto change wallpaper";
            shareLinkTask.LinkUri = new Uri("http://www.windowsphone.com/en-us/store/app/ac-wallpaper-free/7cf1bb63-69f0-4280-9484-f09c8586f4ca", UriKind.Absolute);
            shareLinkTask.Message = "Here are a nice app to set dynamic wallpaper for Windows Phone.";

            shareLinkTask.Show();
        }
        private void SMSThis_Click(object sender, EventArgs e)
        {
            SmsComposeTask smsComposeTask = new SmsComposeTask();
            smsComposeTask.Body = "Here are a nice app to set dynamic wallpaper for Windows Phone. Click for detail: http://www.windowsphone.com/en-us/store/app/ac-wallpaper-free/7cf1bb63-69f0-4280-9484-f09c8586f4ca";
            smsComposeTask.Show();
        }
        private void EmailThis_Click(object sender, EventArgs e)
        {
            EmailComposeTask emailtask = new EmailComposeTask();
            emailtask.Subject = "Pin your image to home screen";
            emailtask.Body = "Here are a nice app to set dynamic wallpaper for Windows Phone. Click for detail: http://www.windowsphone.com/en-us/store/app/ac-wallpaper-free/7cf1bb63-69f0-4280-9484-f09c8586f4ca";
            emailtask.Bcc = "lttrungbk@yahoo.com";
            emailtask.Show();
        }

    }
}