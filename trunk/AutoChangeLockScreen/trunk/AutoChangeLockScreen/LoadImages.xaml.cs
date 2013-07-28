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


namespace AutoChangeLockScreen
{
    public partial class LoadImages : PhoneApplicationPage
    {
        // Constructor
        public LoadImages()
        {
            InitializeComponent();

            Loaded += LoadImages_Loaded;
        }
        private void LoadImages_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            string[] files = isoStore.GetFileNames("*");
            App.imageList = new List<myImages>();
            foreach (string dirfile in files)
            {
                if (dirfile.ToString() != "SetSource.ini")
                    App.imageList.Add(new myImages(dirfile.ToString(), false));
            }

            this.myList.ItemsSource = App.imageList;
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
            App.isDefault = 1;
            App.StartAgent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Page1.xaml", UriKind.Relative));
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
                    if (storage.FileExists(img.ImageName))
                    {
                        storage.DeleteFile(img.ImageName);
                        RenameImage(img.ImageName, list, i);
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
                    if (p == img.ImageName) return;
                    if (storage.FileExists(img.ImageName))
                    {
                        storage.MoveFile(img.ImageName, p);
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

    }
}
