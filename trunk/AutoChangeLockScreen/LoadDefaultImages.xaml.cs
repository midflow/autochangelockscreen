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
using AutoChangeLockScreen.Models;
using AutoChangeLockScreen.Services;
using AutoChangeLockScreen.ViewModels;
using System.IO;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using Windows.Storage;
using System.Windows.Media;
using Microsoft.Expression.Interactivity.Core;
using AutoChangeLockScreen.Resources;


namespace AutoChangeLockScreen
{
    public partial class LoadDefaultImages : PhoneApplicationPage
    {
        // Constructor
        public LoadDefaultImages()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            Loaded += LoadImages_Loaded;
        }
        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = App.GetColorFromHexString("FF08317B");
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 0.8;

            LocalizedButtonBar("/Assets/AppBar/transport.play.png", AppResources.Start);           
        }
        private void LocalizedButtonBar(string imgpath, string text)
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton =
                new ApplicationBarIconButton(new
                Uri(imgpath, UriKind.Relative));
            appBarButton.Text = text;
            appBarButton.Click += btnStart_Click;
            ApplicationBar.Buttons.Add(appBarButton);
        }

        private void LoadImages_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            string path = Path.Combine(Environment.CurrentDirectory, "wallpaper");
            string[] files = Directory.GetFiles(path);
            List<DefaultImage> list = new List<DefaultImage>();
            //App.imageList = new List<myImages>();
            foreach (string dirfile in files)
            {
                if (dirfile.ToString().Substring(dirfile.Length - 3, 3) == "jpg")
                {
                    list.Add(new DefaultImage(dirfile.ToString()));
                    //img. =new Uri(dirfile.ToString());
                    //this.myList.ItemsSource
                }
            }

            this.myList.ItemsSource = list;
            ApplicationBarIconButton aibStart = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
            aibStart.IsEnabled = list.Count > 0 ? true : false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            App.isDefault = 0;
            App.StartAgent();
        }

    }

    public class DefaultImage
    {
         public BitmapImage ImageBinary
        {
            get { return m_ImageBinary; }
            set { m_ImageBinary = value; }
        }
        public string ImageName
        {
            get { return m_ImageName; }
            set { m_ImageName = value; }
        }

        public string ImageSize
        {
            get { return m_ImageSize; }
            set { m_ImageSize = value; }
        }

       

        private string m_ImageName;        
        private string m_ImageSize;
        private BitmapImage m_ImageBinary;
        public DefaultImage(string strImageName)
        {
            this.ImageName = strImageName.Split('\\')[6];           
            //*** Image Binary ***'
            BitmapImage image = new BitmapImage(new Uri(strImageName));
            //IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            //string isoFilename = strImageName;
            Stream stream = File.Open(strImageName, System.IO.FileMode.Open);
            image.SetSource(stream);
            this.ImageBinary = image;
            //*** Image Size ***'
            this.ImageSize = stream.Length + " Bytes";
            stream.Close();
        }      
    }
}
