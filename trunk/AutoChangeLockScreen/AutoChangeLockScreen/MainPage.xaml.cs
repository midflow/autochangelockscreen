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


namespace AutoChangeLockScreen
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        PeriodicTask periodicTask;
        string periodicTaskName = "PeriodicAgent";
        public bool agentsAreEnabled = true;

        public MainPage()
        {
            InitializeComponent();

            //AppTitle.Text = AutoChangeLockScreen.Resources.AppResources.ApplicationTitle;
            // Sample code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
           
        }
        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            LocalizedButtonBar("/Assets/AppBar/check.png", AppResources.Start);
            LocalizedButtonBar("/Assets/AppBar/edit.png", AppResources.SetSource);
            LocalizedButtonBar("/Assets/AppBar/favs.png", AppResources.RateIt);
            LocalizedButtonBar("/Assets/AppBar/appbar.share.rest.png", AppResources.BuyApp);

            LocalizedMenuBar(AppResources.Start);
            LocalizedMenuBar(AppResources.MyApps);
            LocalizedMenuBar(AppResources.BuyApp);
        }

        private void LocalizedButtonBar(string imgpath, string text)
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
           

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton =
                new ApplicationBarIconButton(new
                Uri(imgpath, UriKind.Relative));
            appBarButton.Text = text;
            appBarButton.Click += ApplicationBarIconButton_Click;
            ApplicationBar.Buttons.Add(appBarButton);
        }

        private void LocalizedMenuBar(string text)
        {
            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItem =
                new ApplicationBarMenuItem(text);
            appBarMenuItem.Click += ApplicationBarMenuItem_Click;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }
        
        private void StartPeriodicAgent()
        {
            // is old task running, remove it
            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;
            if (periodicTask != null)
            {
                try
                {
                    ScheduledActionService.Remove(periodicTaskName);
                }
                catch (Exception)
                {
                }
            }
            // create a new task
            periodicTask = new PeriodicTask(periodicTaskName);
            // load description from localized strings
            periodicTask.Description = "This is Lockscreen image provider app.";
            // set expiration days
            periodicTask.ExpirationTime = DateTime.Now.AddDays(14);
            try
            {
                // add thas to scheduled action service
                ScheduledActionService.Add(periodicTask);
                // debug, so run in every 30 secs
//#if(DEBUG_AGENT)

                ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(10));
                System.Diagnostics.Debug.WriteLine("Periodic task is started: " + periodicTaskName);
//#endif

            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    // load error text from localized strings
                    MessageBox.Show("Background agents for this application have been disabled by the user.");
                }
                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                    // No user action required. The system prompts the user when the hard limit of periodic tasks has been reached.
                }
            }
            catch (SchedulerServiceException)
            {
                // No user action required.
            }
        }

        private async void LockScreenChange(string filePathOfTheImage, bool isAppResource)
        {
            try
            {
                if (!LockScreenManager.IsProvidedByCurrentApplication)
                {
                    // If you're not the provider, this call will prompt the user for permission.
                    // Calling RequestAccessAsync from a background agent is not allowed.
                    await LockScreenManager.RequestAccessAsync();
                }

                // Only do further work if the access is granted.
                if (LockScreenManager.IsProvidedByCurrentApplication)
                {
                    // At this stage, the app is the active lock screen background provider.
                    // The following code example shows the new URI schema.
                    // ms-appdata points to the root of the local app data folder.
                    // ms-appx points to the Local app install folder, to reference resources bundled in the XAP package
                    var schema = isAppResource ? "ms-appx:///" : "ms-appdata:///Local/";
                    var uri = new Uri(schema + filePathOfTheImage, UriKind.Absolute);

                    // Set the lock screen background image.
                    LockScreen.SetImageUri(uri);

                    // Get the URI of the lock screen background image.
                    var currentImage = LockScreen.GetImageUri();
                    System.Diagnostics.Debug.WriteLine("The new lock screen background image is set to {0}", currentImage.ToString());
                    MessageBox.Show("Lock screen changed. Click F12 or go to lock screen.");
                }
                else
                {
                    MessageBox.Show("Background cant be updated as you clicked no!!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            try
            {
                ApplicationBarIconButton appBIB = (ApplicationBarIconButton)sender;
                if (appBIB.Text == AppResources.Start)
                {
                    LockScreenChange("wallpaper/CustomizedPersonalWalleper_0.jpg", true);
                    StartPeriodicAgent();
                }
                else if (appBIB.Text == AppResources.RateIt)
                {
                    MarketplaceReviewTask review = new MarketplaceReviewTask();
                    review.Show();
                }
                else if (appBIB.Text == AppResources.BuyApp)
                {
                    MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();                    

                    marketplaceDetailTask.ContentIdentifier = "c743bdf5-620a-42ef-a493-4793aa400668";
                    marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;

                    marketplaceDetailTask.Show();
                }
                else if (appBIB.Text == AppResources.SetSource)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        NavigationService.Navigate(new Uri("/LoadImages.xaml", UriKind.Relative));
                    });
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ApplicationBarMenuItem appMnuItem = (ApplicationBarMenuItem)sender;
                if (appMnuItem.Text == AppResources.Start)
                 {
                     LockScreenChange("wallpaper/CustomizedPersonalWalleper_0.jpg", true);
                     StartPeriodicAgent();
                 }
                else if (appMnuItem.Text == AppResources.MyApps)
                {
                    MarketplaceSearchTask mkpSearch = new MarketplaceSearchTask();
                    mkpSearch.ContentType = MarketplaceContentType.Applications;
                    mkpSearch.SearchTerms = "trunglt";

                    mkpSearch.Show();
                }
                else if (appMnuItem.Text == AppResources.BuyApp)
                {
                    MarketplaceDetailTask marketplaceDetailTask = new MarketplaceDetailTask();

                    marketplaceDetailTask.ContentIdentifier = "c743bdf5-620a-42ef-a493-4793aa400668";
                    marketplaceDetailTask.ContentType = MarketplaceContentType.Applications;

                    marketplaceDetailTask.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

       

    }
}