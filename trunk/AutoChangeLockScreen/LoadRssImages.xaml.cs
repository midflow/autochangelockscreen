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
    public partial class LoadRssImages : PhoneApplicationPage
    {
        // Constructor
        public LoadRssImages()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            Loaded += LoadImages_Loaded;
        }
        private void LoadImages_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (isoStore.DirectoryExists("download"))
            {
                string[] files = isoStore.GetFileNames("download/*");
                App.imageList = new List<myImages>();
                foreach (string dirfile in files)
                {
                    App.imageList.Add(new myImages("download/" + dirfile.ToString(), false));
                }

                this.myList.ItemsSource = App.imageList;
                ApplicationBarIconButton aibStart = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                aibStart.IsEnabled = App.imageList.Count > 0 ? true : false;
            }
        }
        // Build a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {            
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = App.GetColorFromHexString("FF2648A2");
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 0.8;
            LocalizedButtonBar("/Assets/AppBar/transport.play.png", AppResources.Start, btnStart_Click);
            LocalizedButtonBar("/Assets/AppBar/add.png", AppResources.Start, btnAdd_Click);
            LocalizedButtonBar("/Assets/AppBar/minus.png", AppResources.Start, btnMinus_Click);

            LocalizedMenuBar(AppResources.DeleteAll, btnDeleteAll_Click);
            LocalizedMenuBar(AppResources.Delete, btnMinus_Click);
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
        private void LocalizedMenuBar(string text, EventHandler function)
        {
            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItem =
                new ApplicationBarMenuItem(text);
            appBarMenuItem.Click += function;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadImages_Loaded(null, null);
        }
        private void myList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<StackPanel> listItems = new List<StackPanel>();
            GetItemsRecursive<StackPanel>(myList, ref listItems);

            if (e.AddedItems.Count > 0 && e.AddedItems[0] != null)
            {
                foreach (StackPanel stp in listItems)
                {
                    if (e.AddedItems[0].Equals(stp.DataContext))
                    {
                        myImages myimg = e.AddedItems[0] as myImages;

                        myimg.ImageSeclected = !myimg.ImageSeclected;

                        if (myimg.ImageSeclected)
                            ExtendedVisualStateManager.GoToElementState(stp, "Selected", true);
                        else
                            ExtendedVisualStateManager.GoToElementState(stp, "Normal", true);
                    }
                }
            }
        }

        private void ClearSelectedPanel()
        {
            List<StackPanel> listItems = new List<StackPanel>();
            GetItemsRecursive<StackPanel>(myList, ref listItems);

            foreach (StackPanel stp in listItems)
            {
                ExtendedVisualStateManager.GoToElementState(stp, "Normal", true);
            }

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            App.isDefault = 2;
            App.StartAgent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/RssPage.xaml", UriKind.Relative));
        }

        private void btnMinus_Click(object sender, EventArgs e)
        {
            var list = myList.ItemsSource;

            for (int i = 0; i < list.Count; i++)
            {
                myImages img = (myImages)list[i];
                if (img.ImageSeclected)
                {
                    IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                    if (storage.FileExists("download/" + img.ImageName))
                    {
                        storage.DeleteFile("download/" + img.ImageName);
                        RenameImage("download/" + img.ImageName, list, i);
                    }
                    //App.imageList.Remove(img);
                }
            }
            ClearSelectedPanel();
            LoadImages_Loaded(null, null);
        }

        private void RenameImage(string p, System.Collections.IList list, int j)
        {
            for (int i = list.Count; i > j; i--)
            {
                myImages img = (myImages)list[i - 1];
                if (img.ImageSeclected == false)
                {
                    IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                    if (p == "download/" + img.ImageName) return;
                    if (storage.FileExists("download/" + img.ImageName))
                    {
                        storage.MoveFile("download/" + img.ImageName, p);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Recursively get the item.
        /// </summary>
        /// <typeparam name="T">The item to get.</typeparam>
        /// <param name="parents">Parent container.</param>
        /// <param name="objectList">Item list</param>
        public static void GetItemsRecursive<T>(DependencyObject parents, ref List<T> objectList) where T : DependencyObject
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(parents);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parents, i);

                if (child is T)
                {
                    objectList.Add(child as T);
                }

                GetItemsRecursive<T>(child, ref objectList);
            }

            return;
        }
        private T FindFirstElementInVisualTree<T>(DependencyObject parentElement) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(parentElement);
            if (count == 0)
                return null;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parentElement, i);

                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    var result = FindFirstElementInVisualTree<T>(child);
                    if (result != null)
                        return result;

                }
            }
            return null;
        }

        private void btnDeleteAll_Click(object sender, EventArgs e)
        {
            var list = myList.ItemsSource;

            for (int i = 0; i < list.Count; i++)
            {
                myImages img = (myImages)list[i];

                IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (storage.FileExists("download/" + img.ImageName))
                {
                    storage.DeleteFile("download/" + img.ImageName);
                    //RenameImage("download/" + img.ImageName, list, i);
                }
                //App.imageList.Remove(img);

            }
            ClearSelectedPanel();
            LoadImages_Loaded(null, null);
        }

    }
}
