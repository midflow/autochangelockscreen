﻿//#define DEBUG_AGENT

using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using Windows.Phone.System.UserProfile;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;
using System.Threading;

namespace ScheduledTaskAgent1
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        public static string strPath = "Wallpaper/*";
        public static bool isAppResource = false;
        public static bool isRandom = false;
        public static int intInterval = 30;
        public static int NoImage = 0;
        public static string strSource = "";
        public static int counter = 0;
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });

            //if (IsolatedStorageSettings.ApplicationSettings.Contains("Interval"))
            //{
            //    is30 = bool.Parse(IsolatedStorageSettings.ApplicationSettings["Interval"] as string);
            //}

            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            if (iso.FileExists("SetSource.ini"))
            {
                IsolatedStorageFileStream fileStream = iso.OpenFile("SetSource.ini", FileMode.Open, FileAccess.Read);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    String strContent = reader.ReadLine();
                    strSource = strContent.Split(' ')[0];
                    NoImage = int.Parse("0" + strContent.Split(' ')[1]);
                    isRandom = bool.Parse(strContent.Split(' ')[2]);
                    intInterval = int.Parse(strContent.Split(' ')[3]);
                    counter = int.Parse(strContent.Split(' ')[4]);

                    switch (strSource)
                    {
                        case "Default":
                            strPath = "wallpaper/*";
                            isAppResource = true;
                            break;
                        case "Your":
                            strPath = "*";
                            isAppResource = false;
                            break;
                        case "Rss":
                            strPath = "download/*";
                            isAppResource = false;
                            break;
                    }
                    //LoadImages_Loaded(strPath);
                }
            }
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            try
            {
                //if (intInterval != 30)
                //{
                counter++;

                IsolatedStorageFile isoStore;
                isoStore = IsolatedStorageFile.GetUserStoreForApplication();
                IsolatedStorageFileStream fileStream = isoStore.OpenFile("SetSource.ini", FileMode.Create, FileAccess.Write);
                string strContent = strSource;
                strContent += " " + NoImage.ToString();
                strContent += " " + isRandom.ToString();
                strContent += " " + intInterval.ToString();
                strContent += " " + counter.ToString();
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(strContent);
                    writer.Close();
                }
                if (counter % (intInterval / 30) == 0)
                {
                    // Get the URI of the lock screen background image.
                    var currentImage = LockScreen.GetImageUri();
                    string Imagename = string.Empty;

                    Imagename = GetNextImage(currentImage.ToString());

                    LockScreenChange(Imagename, isAppResource);
                }
                // If debugging is enabled, launch the agent again in one minute.
                // debug, so run in every 30 secs
                //#if(DEBUG_AGENT)
                //                        ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(10));
                //                        System.Diagnostics.Debug.WriteLine("Periodic task is started again: " + task.Name);
                //#endif

                // Call NotifyComplete to let the system know the agent is done working.
                NotifyComplete();

                //}
                //else
                //{
                //    // Get the URI of the lock screen background image.
                //    var currentImage = LockScreen.GetImageUri();
                //    string Imagename = string.Empty;

                //    Imagename = GetNextImage(currentImage.ToString());

                //    LockScreenChange(Imagename, isAppResource);

                //    // If debugging is enabled, launch the agent again in one minute.
                //    // debug, so run in every 30 secs
                //    //#if(DEBUG_AGENT)
                //    //                    ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(10));
                //    //                    System.Diagnostics.Debug.WriteLine("Periodic task is started again: " + task.Name);
                //    //#endif

                //    // Call NotifyComplete to let the system know the agent is done working.
                //    NotifyComplete();
                //}
            }
            finally { }
        }

        private string GetNextImage(string currentImage)
        {
            string Imagename = "";
            string imgCount = currentImage.ToString().Substring(currentImage.ToString().IndexOf('_') + 1, currentImage.ToString().Length - (currentImage.ToString().IndexOf('_') + 1)).Replace(".jpg", "");

            if (isRandom)
            {
                Random ran = new Random();
                int iran = ran.Next(0, NoImage);
                switch (strSource)
                {
                    case "Default":
                        //if (imgCount != (NoImage - 1).ToString())
                        //    Imagename = "wallpaper/Wallpaper_" + Convert.ToString(Convert.ToUInt32(imgCount) + 1) + ".jpg";
                        //else
                        //    Imagename = "wallpaper/Wallpaper_0.jpg";
                        //break;
                        Imagename = "wallpaper/Wallpaper_" + Convert.ToString(iran) + ".jpg";
                        break;
                    case "Your":
                        //if (imgCount != (NoImage - 1).ToString())
                        //    Imagename = "image_" + Convert.ToString(Convert.ToUInt32(imgCount) + 1) + ".jpg";
                        //else
                        //    Imagename = "image_0.jpg";
                        Imagename = "image_" + Convert.ToString(iran) + ".jpg";
                        break;
                    case "Rss":
                        //if (imgCount != (NoImage - 1).ToString())
                        //    Imagename = "download/Wallpaper_" + Convert.ToString(Convert.ToUInt32(imgCount) + 1) + ".jpg";
                        //else
                        //    Imagename = "download/Wallpaper_0.jpg";
                        Imagename = "download/Wallpaper_" + Convert.ToString(iran) + ".jpg";
                        break;
                }
            }
            else
            {
                switch (strSource)
                {
                    case "Default":
                        if (imgCount != (NoImage - 1).ToString())
                            Imagename = "wallpaper/Wallpaper_" + Convert.ToString(Convert.ToUInt32(imgCount) + 1) + ".jpg";
                        else
                            Imagename = "wallpaper/Wallpaper_0.jpg";
                        break;
                    case "Your":
                        if (imgCount != (NoImage - 1).ToString())
                            Imagename = "image_" + Convert.ToString(Convert.ToUInt32(imgCount) + 1) + ".jpg";
                        else
                            Imagename = "image_0.jpg";
                        break;
                    case "Rss":
                        if (imgCount != (NoImage - 1).ToString())
                            Imagename = "download/Wallpaper_" + Convert.ToString(Convert.ToUInt32(imgCount) + 1) + ".jpg";
                        else
                            Imagename = "download/Wallpaper_0.jpg";
                        break;
                }
            }
            return Imagename;
        }
        //private static void LoadImages_Loaded(string path)
        //{
        //    IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        //    string[] files = isoStore.GetFileNames(path);
        //    imageList = new List<myImages>();
        //    foreach (string dirfile in files)
        //    {
        //        if (dirfile.ToString() != "SetSource.ini")
        //            imageList.Add(new myImages(dirfile.ToString(), false));
        //    }
        //}

        private async void LockScreenChange(string filePathOfTheImage, bool isAppResource)
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
            }
        }
    }

}