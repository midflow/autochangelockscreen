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
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.Windows.Resources;
using Windows.Phone.System.UserProfile;
using Microsoft.Phone.Scheduler;
using System.Threading;
using System.IO;
using AutoChangeLockScreen.Models;

namespace AutoChangeLockScreen
{
    public partial class RssPage : PhoneApplicationPage
    {
        PeriodicTask periodicTask;
        string periodicTaskName = "PeriodicAgent";
        public bool agentsAreEnabled = true;

        // Global count
        public int imageCount = 0;
        public string[] imgarray;
        public string NoImage = "10";

        // Constructor
        public RssPage()
        {
            InitializeComponent();
            btnChangeImg.IsEnabled = false;            
        }

        #region Download image Methods
        public void DownloadRSS(string rssURL)
        {
            WebClient myRSS = new WebClient();
            myRSS.DownloadStringCompleted += new DownloadStringCompletedEventHandler(myRSS_DownloadStringCompleted);
            myRSS.DownloadStringAsync(new Uri(rssURL));
        }
        private void DownloadImagefromServer(string imageUrl)
        {
            WebClient client = new WebClient();
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadCompleted);
            client.OpenReadAsync(new Uri(imageUrl, UriKind.Absolute));

        }
        void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(e.Result);

            // Create a filename for JPEG file in isolated storage.

            String tempJPEG = "download/Walleper_" + Convert.ToString(imageCount) + ".jpg";

            // Create virtual store and file stream. Check for duplicate tempJPEG files.
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!myIsolatedStorage.DirectoryExists("download"))
                {
                    myIsolatedStorage.CreateDirectory("download");
                }
                if (myIsolatedStorage.FileExists(tempJPEG))
                {
                    myIsolatedStorage.DeleteFile(tempJPEG);
                }

                IsolatedStorageFileStream fileStream = myIsolatedStorage.CreateFile(tempJPEG);

                StreamResourceInfo sri = null;
                Uri uri = new Uri(tempJPEG, UriKind.Relative);
                sri = Application.GetResourceStream(uri);

                WriteableBitmap wb = new WriteableBitmap(bitmap);

                // Encode WriteableBitmap object to a JPEG stream.
                System.Windows.Media.Imaging.Extensions.SaveJpeg(wb, fileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);

                //wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                fileStream.Close();

                System.Diagnostics.Debug.WriteLine("image downloaded: " + tempJPEG);
                DwldImg.Source = null;

                Thread.Sleep(400);

                if (imageCount == int.Parse(NoImage)-1)
                {
                    btnChangeImg.IsEnabled = true;

                    progress.IsIndeterminate = false;
                    progress.Visibility = Visibility.Collapsed;
                    downloadPercentage.Text = "Completed downloading.";                    
                }

                if (imageCount < imgarray.Length - 1)
                {
                    imageCount++;
                    DownloadImagefromServer(Convert.ToString(imgarray[imageCount]));
                    DwldImg.Source = bitmap;

                }

            }
        }
        void myRSS_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            //Check if the Network is available
            if (Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsNetworkAvailable)
            {
                XNamespace media = XNamespace.Get("http://search.yahoo.com/mrss/");
                imgarray = XElement.Parse(e.Result).Descendants(media + "content")
                    .Where(m => m.Attribute("type").Value == "image/jpeg")
                    .Select(m => m.Attribute("url").Value)
                    .ToArray();

                if (imgarray.Length > 0)
                {
                    imageCount = 0;
                    DownloadImagefromServer(Convert.ToString(imgarray[0]));
                }
                else
                {
                    MessageBox.Show("No image found for tag " + txtTag.Text + ". Please try other tag name for image.");
                }

            }
            else
            {
                MessageBox.Show("No network is available..");
            }
        }
        #endregion

        #region LockScreen Methods
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
                System.Diagnostics.Debug.WriteLine("The new lock screen background image is set to {0}", currentImage.ToString());
                MessageBox.Show("Lock screen changed. Click F12 or go to lock screen.");
            }
            else
            {
                MessageBox.Show("Background cant be updated as you clicked no!!");
            }
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
#if(DEBUG_AGENT)
                ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(30));
                System.Diagnostics.Debug.WriteLine("Periodic task is started: " + periodicTaskName);
#endif

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
        private void ChangeLockscreen()
        {
            try
            {
                var currentImage = LockScreen.GetImageUri();
                string Imagename = string.Empty;

                string imgCount = currentImage.ToString().Substring(currentImage.ToString().IndexOf('_') + 1, currentImage.ToString().Length - (currentImage.ToString().IndexOf('_') + 1)).Replace(".jpg", "");

                if (imgCount != "24")
                    Imagename = "DownloadedWalleper_" + Convert.ToString(Convert.ToUInt32(imgCount) + 1) + ".jpg";
                else
                    Imagename = "DownloadedWalleper_0.jpg";

                LockScreenChange(Imagename, false);
                BitmapImage Bit_Img = new BitmapImage();
                using (IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream FS = ISF.OpenFile(Imagename, FileMode.Open, FileAccess.Read))
                    {
                        Bit_Img.SetSource(FS);
                    }
                }
                this.DwldImg.Source = Bit_Img;
                //DwldImg.Source = new BitmapImage(new Uri("ms-appdata:///Local/" + Imagename, UriKind.RelativeOrAbsolute));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region Methods

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NoImage = rbt10.IsChecked == true ? "10" : (rbt20.IsChecked == true ? "20" : (rbt30.IsChecked == true ? "30" : "50"));
            if (!string.IsNullOrEmpty(txtTag.Text))
            {
                btnDownload.IsEnabled = false;
                listBox1.Visibility = System.Windows.Visibility.Collapsed;
                DwldImg.Visibility = System.Windows.Visibility.Visible;
                btnChangeImg.IsEnabled = false;
                progress.IsIndeterminate = true;
                progress.Visibility = Visibility.Visible;
                downloadPercentage.Text = "Downloading.... please wait...";
                string url = "http://www.degraeve.com/flickr-rss/rss.php?tags=" + txtTag.Text + "&tagmode=all&sort=interestingness-desc&num=" + NoImage;
                DownloadRSS(url);
                btnDownload.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Please write your tag in textbox of images you want to show on lock screen");
            }

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ChangeLockscreen();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            NoImage = rbt10.IsChecked == true ? "10" : (rbt20.IsChecked == true ? "20" : (rbt30.IsChecked == true ? "30" : "50"));
            //MessageBox.Show(NoImage);
            DwldImg.Visibility = System.Windows.Visibility.Collapsed;
            listBox1.Visibility = System.Windows.Visibility.Visible;
            WebClient webclient = new WebClient();
            webclient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webclient_DownloadStringCompleted);
            //webclient.DownloadStringAsync(new Uri("http://api.flickr.com/services/feeds/photos_public.gne?tag=" + txtTag.Text + "&format=rss2")); // Flickr search
            webclient.DownloadStringAsync(new Uri("http://www.degraeve.com/flickr-rss/rss.php?tags=" + txtTag.Text + "&tagmode=all&sort=interestingness-desc&num=" + NoImage)); // Flickr search
        }

        void webclient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("error");

            }

            // parsing Flickr 
            if (Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsNetworkAvailable)
            {
                //XNamespace media = XNamespace.Get("http://search.yahoo.com/mrss/");
                //imgarray = XElement.Parse(e.Result).Descendants(media + "content")
                //    .Where(m => m.Attribute("type").Value == "image/jpeg")
                //    .Select(m => m.Attribute("url").Value)
                //    .ToArray();

                XElement XmlTweet = XElement.Parse(e.Result);

                XNamespace ns = "http://search.yahoo.com/mrss/"; // flilckr
                var list = from tweet in XmlTweet.Descendants(ns + "content")
                                                      //where tweet.Element("type").Value == "image/jpeg"
                                                      select new FlickrData
                                                      {
                                                          ImageSource = tweet.Attribute("url").Value,
                                                          //Description = tweet.Element("description").Value,
                                                          //Title = tweet.Attribute("url").Value.Split('//')[],
                                                          //PubDate = DateTime.Parse(tweet.Element("pubDate").Value)
                                                      };
                IList<FlickrData> ilist = list.Cast<FlickrData>().ToList();
                listBox1.ItemsSource = ilist;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // set first image 
            LockScreenChange("Download/Walleper_0.jpg", false);
            // start service
            StartPeriodicAgent();
        }
        #endregion

        
    }
}