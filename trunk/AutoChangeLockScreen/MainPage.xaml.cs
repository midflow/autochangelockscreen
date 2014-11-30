#define DEBUG_AGENT

using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Windows.ApplicationModel.Store;
using AutoChangeLockScreen.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using vservWindowsPhone;

namespace AutoChangeLockScreen
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        //PeriodicTask periodicTask;
        //string periodicTaskName = "PeriodicAgent";
        public static int NumberImage = 0;
        private readonly VservAdControl VAC = VservAdControl.Instance;
        public bool agentsAreEnabled = true;

        public MainPage()
        {
            InitializeComponent();
            //LoadImages_Loaded();
            //AppTitle.Text = AutoChangeLockScreen.Resources.AppResources.ApplicationTitle;
            // Sample code to localize the ApplicationBar
            VAC.SetRequestTimeOut(30);
            //VAC.VservAdClosed += new EventHandler(VACCallback_OnVservAdClosing);
            VAC.VservAdError += new EventHandler(VACCallback_OnVservAdNetworkError);
            VAC.VservAdClosed += new EventHandler(VACCallback_OnVservAdClosing);
            VAC.VservAdNoFill += new EventHandler(VACCallback_OnVservAdNoFill);
            BuildLocalizedApplicationBar();
            if (!Utils.ShowAds)
            {
                if (ApplicationBar.Buttons.Count > 3)
                    ApplicationBar.Buttons.RemoveAt(1);
                if (ApplicationBar.MenuItems.Count > 3)
                    ApplicationBar.MenuItems.RemoveAt(0);
                txtPaidMsg.Text = "";
                RowAds1.Height = new GridLength(0);
                RowAds.Height = new GridLength(0);
                btnSave.Content = AppResources.Save;
            }
            else
            {
                //Render_Ad();
                txtPaidMsg.Text = AppResources.PaidWarning;
                btnSave.Content = AppResources.SavePaid;
                if (App.ShowVserv_Main) Display_Ad();
            }

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
            LocalizedButtonBar("/Assets/AppBar/appbar.share.rest.png", AppResources.BuyApp, BuyAppButton_Click);
            LocalizedButtonBar("/Assets/AppBar/appbar.questionmark.rest.png", AppResources.Help, HelpButton_Click);
            LocalizedButtonBar("/Assets/AppBar/appbar.status.rest.png", AppResources.About, AboutButton_Click);

            LocalizedMenuBar(AppResources.RemoveAds, RemoveAds_Click);
            LocalizedMenuBar(AppResources.ShareThis, ShareThis_Click);
            LocalizedMenuBar(AppResources.SMSThis, SMSThis_Click);
            LocalizedMenuBar(AppResources.EmailThis, EmailThis_Click);
        }

        private async void RemoveAds_Click(object sender, EventArgs e)
        {
            try
            {
                await CurrentApp
                    .RequestProductPurchaseAsync("NoAds", false);
                UpdateAd();
            }
            catch (Exception)
            {
                // oh well
            }
        }

        public void UpdateAd()
        {
            AdView.Visibility = Utils.ShowAds ? Visibility.Visible : Visibility.Collapsed;
            AdView1.Visibility = Utils.ShowAds ? Visibility.Visible : Visibility.Collapsed;

            // if we add more of these, we'll need to be more clever here
            if (!Utils.ShowAds)
            {
                if (ApplicationBar.Buttons.Count > 3)
                    ApplicationBar.Buttons.RemoveAt(1);
                if (ApplicationBar.MenuItems.Count > 3)
                    ApplicationBar.MenuItems.RemoveAt(0);
                txtPaidMsg.Text = "";
                RowAds1.Height = new GridLength(0);
                RowAds.Height = new GridLength(0);
                btnSave.Content = AppResources.Save;
            }
            else
            {
                //Render_Ad();
                txtPaidMsg.Text = AppResources.PaidWarning;
                btnSave.Content = AppResources.SavePaid;
                if (App.ShowVserv_Main) Display_Ad();
            }
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
                a = Convert.ToByte(s.Substring(0, 2), 16);
                startParseIndex += 2;
            }

            // read r value
            byte r = Convert.ToByte(s.Substring(startParseIndex, 2), 16);
            startParseIndex += 2;
            // read g value
            byte g = Convert.ToByte(s.Substring(startParseIndex, 2), 16);
            startParseIndex += 2;
            // read b value
            byte b = Convert.ToByte(s.Substring(startParseIndex, 2), 16);

            return Color.FromArgb(a, r, g, b);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UpdateAd();
            while (NavigationService.BackStack.Any())
            {
                NavigationService.RemoveBackEntry();
            }
        }

        private void LocalizedButtonBar(string imgpath, string text, EventHandler function)
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            // Create a new button and set the text value to the localized string from AppResources.
            var appBarButton =
                new ApplicationBarIconButton(new
                    Uri(imgpath, UriKind.Relative));
            appBarButton.Text = text;
            appBarButton.Click += function;
            ApplicationBar.Buttons.Add(appBarButton);
        }

        private void btnDefault_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(
                () => { NavigationService.Navigate(new Uri("/LoadDefaultImages.xaml", UriKind.Relative)); });
        }

        private void btnYour_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() => { NavigationService.Navigate(new Uri("/LoadImages.xaml", UriKind.Relative)); });
        }

        private void btnRSS_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(
                () => { NavigationService.Navigate(new Uri("/LoadRssImages.xaml", UriKind.Relative)); });
        }

        private void LocalizedMenuBar(string text, EventHandler function)
        {
            // Create a new menu item with the localized string from AppResources.
            var appBarMenuItem =
                new ApplicationBarMenuItem(text);
            appBarMenuItem.Click += function;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        private void ReviewButton_Click(object sender, EventArgs e)
        {
            var review = new MarketplaceReviewTask();
            review.Show();
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() => { NavigationService.Navigate(new Uri("/Help.xaml", UriKind.Relative)); });
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() => { NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative)); });
        }

        private void MyAppsButton_Click(object sender, EventArgs e)
        {
            var mkpSearch = new MarketplaceSearchTask();
            mkpSearch.ContentType = MarketplaceContentType.Applications;
            mkpSearch.SearchTerms = "trunglt";

            mkpSearch.Show();
        }

        private void BuyAppButton_Click(object sender, EventArgs e)
        {
            var marketplaceDetailTask = new MarketplaceDetailTask();

            marketplaceDetailTask.ContentIdentifier = "ee8e9449-61b1-4049-9ca4-5407995234ab";
            marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;

            marketplaceDetailTask.Show();
        }

        private void ShareThis_Click(object sender, EventArgs e)
        {
            var shareLinkTask = new ShareLinkTask();

            shareLinkTask.Title = "Auto change wallpaper";
            shareLinkTask.LinkUri = new Uri("http://www.windowsphone.com/s?appid=7cf1bb63-69f0-4280-9484-f09c8586f4ca",
                UriKind.Absolute);
            shareLinkTask.Message = "This is a nice app to set dynamic wallpaper for Windows Phone.";

            shareLinkTask.Show();
        }

        private void SMSThis_Click(object sender, EventArgs e)
        {
            var smsComposeTask = new SmsComposeTask();
            smsComposeTask.Body =
                "This is a nice app to set dynamic wallpaper for Windows Phone. Check it: http://www.windowsphone.com/s?appid=7cf1bb63-69f0-4280-9484-f09c8586f4ca";
            smsComposeTask.Show();
        }

        private void EmailThis_Click(object sender, EventArgs e)
        {
            var emailtask = new EmailComposeTask();
            emailtask.Subject = "Auto change wallpaper";
            emailtask.Body =
                "This is a nice app to set dynamic wallpaper for Windows Phone. Click for detail: http://www.windowsphone.com/s?appid=7cf1bb63-69f0-4280-9484-f09c8586f4ca";
            emailtask.Bcc = "lttrungbk@yahoo.com";
            emailtask.Show();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Utils.ShowAds)
            {
                MessageBox.Show(AppResources.PaidWarning);
                return;
            }
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
            txtPaidMsg.Text = AppResources.SaveMsg;
        }

        private void VACCallback_OnVservAdClosing(object sender, EventArgs e)
        {
            BuildLocalizedApplicationBar();
            App.ShowVserv_Main = false;
        }

        private void VACCallback_OnVservAdNetworkError(object sender, EventArgs e)
        {
            MessageBox.Show("Data connection not available", "No Data", MessageBoxButton.OKCancel);
        }

        private void VACCallback_OnVservAdNoFill(object sender, EventArgs e)
        {
            BuildLocalizedApplicationBar();
            App.ShowVserv_Main = false;
        }

        private void Display_Ad()
        {
            //     BuildLocalizedApplicationBar();
            //// This Method is called for showing Interstitial Ad
            try
            {
                VAC.DisplayAd("9c51ebf0" /* Zone Id*/, LayoutRoot /* Layout over which the Ad will be displayed*/);
                App.ShowVserv_Main = false;
            }
            catch
            {
            }
            //Thickness sz = LayoutRoot.Margin;
        }

        private void LstInterval_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Utils.ShowAds)
            {
                txtPaidMsg.Text = AppResources.PaidWarning;
            }
            else
            {
                txtPaidMsg.Text = "";
            }
        }

        private void chkRandom_Checked(object sender, RoutedEventArgs e)
        {
            if (Utils.ShowAds)
            {
                txtPaidMsg.Text = AppResources.PaidWarning;
            }
            else
            {
                txtPaidMsg.Text = "";
            }
        }

        private void chkRandom_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Utils.ShowAds)
            {
                txtPaidMsg.Text = AppResources.PaidWarning;
            }
            else
            {
                txtPaidMsg.Text = "";
            }
        }
    }
}