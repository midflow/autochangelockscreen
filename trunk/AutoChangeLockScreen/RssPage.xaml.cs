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
        //PeriodicTask periodicTask;
        //string periodicTaskName = "PeriodicAgent";
        //public bool agentsAreEnabled = true;

        // Global count
        public int imageCount = 0;
        public string[] imgarray;
        public string NoImage = "10";

        // Constructor
        public RssPage()
        {
            InitializeComponent();
            btnFinished.IsEnabled = false;            
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

            String tempJPEG = "download/Wallpaper_" + Convert.ToString(imageCount) + ".jpg";

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
                    btnFinished.IsEnabled = true;

                    progress.IsIndeterminate = false;
                    progress.Visibility = Visibility.Collapsed;
                    downloadPercentage.Text = AppResources.CompletedDownloading;                    
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

                NoImage = imgarray.Length.ToString();
                if (imgarray.Length > 0)
                {
                    imageCount = 0;
                    DownloadImagefromServer(Convert.ToString(imgarray[0]));
                }
                else
                {
                    MessageBox.Show(AppResources.NoImageFoundForTag + txtTag.Text + ". " + AppResources.PleaseTryOtherTag);
                }

            }
            else
            {
                MessageBox.Show(AppResources.NoNetwork);
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
                btnFinished.IsEnabled = false;
                progress.IsIndeterminate = true;
                progress.Visibility = Visibility.Visible;
                downloadPercentage.Text = AppResources.Downloading;
                string url = "http://www.degraeve.com/flickr-rss/rss.php?tags=" + txtTag.Text + "&tagmode=all&sort=interestingness-desc&num=" + NoImage;
                DownloadRSS(url);
                btnDownload.IsEnabled = true;                                
            }
            else
            {
                MessageBox.Show(AppResources.PleaseWriteTag);
            }

        }

        //private void Button_Click_2(object sender, RoutedEventArgs e)
        //{
        //    ChangeLockscreen();
        //}

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
                MessageBox.Show(AppResources.Error);

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
        
        #endregion

        private void btnFinished_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        
    }
}