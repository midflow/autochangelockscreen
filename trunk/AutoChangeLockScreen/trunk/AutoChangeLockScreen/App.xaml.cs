
#define DEBUG_AGENT
using System;
using System.Diagnostics;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using AutoChangeLockScreen.Resources;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections.Generic;
using AutoChangeLockScreen.Models;
using Microsoft.Phone.Scheduler;
using Windows.Phone.System.UserProfile;
using System.Windows.Media;
using Windows.ApplicationModel;

namespace AutoChangeLockScreen
{
    public partial class App : Application
    {
        public static int NumberImage = 0;
        public static string imgName = "";
        public static string FullImgName = "";
        public static bool blLoadIamge = false;
        public static int isDefault = 0; //0 = defaul, 1 yours, 2 rss
        public static List<myImages> imageList = new List<myImages>();
        static PeriodicTask periodicTask;
        static string periodicTaskName = "PeriodicAgent";
        public static bool agentsAreEnabled = true;

        //user
        //load file from storage
        public static Stream LoadFile(string file)
        {
            using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isoStore.OpenFile(file, FileMode.Open, FileAccess.Read);
            }
        }

        //write image
        public static void saveImage(BitmapSource bmpsource, string imgName)
        {
            try
            {
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isf.FileExists(imgName))
                        isf.DeleteFile(imgName);
                    using (IsolatedStorageFileStream isfs = isf.CreateFile(imgName))
                    {
                        var bmp = new WriteableBitmap(bmpsource);
                        bmp.SaveJpeg(isfs, bmp.PixelWidth, bmp.PixelHeight, 0, 100);
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        public static void StartAgent()
        {
            string strSource = isDefault == 0 ? "Default" : (isDefault == 2 ? "Rss" : "Your");
            string[] files;
            IsolatedStorageFile isoStore;
            isoStore = IsolatedStorageFile.GetUserStoreForApplication();

            IsolatedStorageFileStream fileStream = isoStore.OpenFile("SetSource.ini", FileMode.Create, FileAccess.Write);

            switch (strSource)
            {
                case "Default":
                    LockScreenChange("wallpaper/Wallpaper_0.jpg", true);
                    //string path = Path.Combine(Environment.CurrentDirectory, "wallpaper");
                    string folder = Package.Current.InstalledLocation.Path;
                    string path = folder + "\\wallpaper";
                    files = Directory.GetFiles(path);
                    NumberImage = files.Length;
                    break;
                case "Your":
                    LockScreenChange(App.imageList[0].ImageName, false);
                    isoStore = IsolatedStorageFile.GetUserStoreForApplication();
                    files = isoStore.GetFileNames("*");
                    NumberImage = files.Length - 1;
                    break;
                case "Rss":
                    LockScreenChange("download/Wallpaper_0.jpg", false);
                    isoStore = IsolatedStorageFile.GetUserStoreForApplication();
                    files = isoStore.GetFileNames("download/*");
                    NumberImage = files.Length;
                    break;
            }

            strSource += " " + NumberImage.ToString();

            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                writer.Write(strSource);
                writer.Close();
            }

            StartPeriodicAgent();
        }

        public static void StartPeriodicAgent()
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
                //        ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(10));
                //        System.Diagnostics.Debug.WriteLine("Periodic task is started: " + periodicTaskName);
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

        public static async void LockScreenChange(string filePathOfTheImage, bool isAppResource)
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
                    //System.Diagnostics.Debug.WriteLine("The new lock screen background image is set to {0}", currentImage.ToString());
                    MessageBox.Show(AppResources.LockScreenChanged);
                }
                else
                {
                    MessageBox.Show(AppResources.ClickNo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static Color GetColorFromHexString(string s)
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

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }
    }
}