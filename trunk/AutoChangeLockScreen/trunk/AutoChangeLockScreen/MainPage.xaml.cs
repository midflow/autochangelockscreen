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
        //static bool blRatingShow = false;

        public MainPage()
        {
            InitializeComponent();
            //LoadImages_Loaded();
            //AppTitle.Text = AutoChangeLockScreen.Resources.AppResources.ApplicationTitle;
            // Sample code to localize the ApplicationBar
            BuildLocalizedApplicationBar();

            if (IsolatedStorageSettings.ApplicationSettings.Contains("Random"))
            {
                chkRandom.IsChecked = bool.Parse(IsolatedStorageSettings.ApplicationSettings["Random"] as string);
            }
            if (IsolatedStorageSettings.ApplicationSettings.Contains("Interval"))
            {
                int interval = int.Parse(IsolatedStorageSettings.ApplicationSettings["Interval"] as string);
                switch (interval)
                {
                    case 30:
                        //rbt30.IsChecked = true;
                        lstInterval.SelectedIndex = 0;
                        break;
                    case 60:
                        //rbt60.IsChecked = true;
                        lstInterval.SelectedIndex = 1;
                        break;
                    case 120: // 2hours
                        //rbt60.IsChecked = true;
                        lstInterval.SelectedIndex = 2;
                        break;
                    case 720: // 1/2 day
                        //rbt60.IsChecked = true;
                        lstInterval.SelectedIndex = 3;
                        break;
                    case 1440: // a day
                        //rbt60.IsChecked = true;
                        lstInterval.SelectedIndex = 4;
                        break;
                }
                
            }           
        }
        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = App.GetColorFromHexString("FF08317B");
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 0.5;
            LocalizedButtonBar("/Assets/AppBar/favs.png", AppResources.Review, ReviewButton_Click);
            LocalizedButtonBar("/Assets/AppBar/appbar.questionmark.rest.png", AppResources.Help, HelpButton_Click);
            LocalizedButtonBar("/Assets/AppBar/appbar.status.rest.png", AppResources.About, AboutButton_Click);
            LocalizedButtonBar("/Assets/AppBar/folder.png", AppResources.MyApps, MyAppsButton_Click);

            LocalizedMenuBar(AppResources.ShareThis, ShareThis_Click);
            LocalizedMenuBar(AppResources.SMSThis, SMSThis_Click);
            LocalizedMenuBar(AppResources.EmailThis, EmailThis_Click);
        }
        private void LocalizedMenuBar(string text, EventHandler function)
        {
            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItem =
                new ApplicationBarMenuItem(text);
            appBarMenuItem.Click += function;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
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
        private void LoadImages_Loaded()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            string[] files = isoStore.GetFileNames("*");
            App.imageList = new List<myImages>();
            foreach (string dirfile in files)
            {
                if (dirfile.ToString() != "SetSource.ini")
                    App.imageList.Add(new myImages(dirfile.ToString(), false));
            }
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
        private void ShareThis_Click(object sender, EventArgs e)
        {
            ShareLinkTask shareLinkTask = new ShareLinkTask();

            shareLinkTask.Title = "Auto change wallpaper";
            shareLinkTask.LinkUri = new Uri("http://www.windowsphone.com/s?appid=ee8e9449-61b1-4049-9ca4-5407995234ab", UriKind.Absolute);
            shareLinkTask.Message = "Here are a nice app to set dynamic wallpaper for Windows Phone.";

            shareLinkTask.Show();
        }
        private void SMSThis_Click(object sender, EventArgs e)
        {
            SmsComposeTask smsComposeTask = new SmsComposeTask();
            smsComposeTask.Body = "Here are a nice app to set dynamic wallpaper for Windows Phone. Click for detail: http://www.windowsphone.com/s?appid=ee8e9449-61b1-4049-9ca4-5407995234ab";
            smsComposeTask.Show();
        }
        private void EmailThis_Click(object sender, EventArgs e)
        {
            EmailComposeTask emailtask = new EmailComposeTask();
            emailtask.Subject = "Auto change wallpaper";
            emailtask.Body = "Here are a nice app to set dynamic wallpaper for Windows Phone. Click for detail: http://www.windowsphone.com/s?appid=ee8e9449-61b1-4049-9ca4-5407995234ab";
            emailtask.Bcc = "lttrungbk@yahoo.com";
            emailtask.Show();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            // txtInput is a TextBox defined in XAML.
            if (!settings.Contains("Random"))
            {
                settings.Add("Random", chkRandom.IsChecked == true ? "True" : "False");
            }
            else
            {
                settings["Random"] = chkRandom.IsChecked == true ? "True" : "False";
            }

            if (!settings.Contains("Interval"))
            {
                switch (lstInterval.SelectedIndex)
                {
                    case 0:
                        settings.Add("Interval", "30");
                        break;
                    case 1:
                        settings.Add("Interval", "60");
                        break;
                    case 2:
                        settings.Add("Interval", "120");
                        break;
                    case 3:
                        settings.Add("Interval", "720");
                        break;
                    case 4:
                        settings.Add("Interval", "1440");
                        break;
                }
                
            }
            else
            {
                switch (lstInterval.SelectedIndex)
                {
                    case 0:
                        settings["Interval"] = "30";
                        break;
                    case 1:
                        settings["Interval"] = "60";
                        break;
                    case 2:
                        settings["Interval"] = "120";
                        break;
                    case 3:
                        settings["Interval"] = "720";
                        break;
                    case 4:
                        settings["Interval"] = "1440";
                        break;
                }                
            }
            settings.Save();
        }
        //private void btnSaveContent_Click(object sender, RoutedEventArgs e)
        //{
        //    IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        //    // txtInput is a TextBox defined in XAML.
        //    if (!settings.Contains("Title"))
        //    {
        //        settings.Add("Title", txtText.Text);
        //    }
        //    else
        //    {
        //        settings["Title"] = txtText.Text;
        //    }

        //    if (!settings.Contains("SourceNews"))
        //    {
        //        settings.Add("SourceNews", lstNews.SelectedIndex);
        //    }
        //    else
        //    {
        //        settings["SourceNews"] = lstNews.SelectedIndex;
        //    }
        //    settings.Save();
        //}

    }
}