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
        public LoadImages()
        {
            InitializeComponent();
            var viewModel = new PhotosViewModel();
            DataContext = viewModel;

            
            //if (PhotoHubLLS.ItemsSource != null)
            //    PhotoHubLLS.ScrollTo(PhotoHubLLS.ItemsSource[PhotoHubLLS.ItemsSource.Count - 1]);

        }

        private void stackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //if (!IsSelected)
            //{
            //VisualStateManager.GoToState(this, "Selected", true);
            //FrameworkElement fre = (FrameworkElement) sender;
            StackPanel stp = (StackPanel) sender;            

            foreach (var child in stp.Children)
            {
                if (child.GetType().ToString() == "System.Windows.Controls.CheckBox")
                {
                    CheckBox chk = (CheckBox)child;
                    chk.IsChecked = !chk.IsChecked;
                    if (chk.IsChecked == true) ExtendedVisualStateManager.GoToElementState(stp, "Selected", true);
                    else ExtendedVisualStateManager.GoToElementState(stp, "Normal", true);
                }
            }           
        }

        private void btnMinus_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var lli in PhotoHubLLS.ItemsSource)
                {
                    System.Collections.IList illi = (System.Collections.IList)lli;
                    Photo pt = (Photo)illi[0];
                    if (pt.Selected == true)
                    {
                        IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication();
                        if (storage.FileExists(pt.ImageSource.ToString().Substring(1)))
                        {
                            storage.DeleteFile(pt.ImageSource.ToString().Substring(1));
                            PhotoHubLLS.ItemsSource.Remove(pt);
                        }
                    }
                }   
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {

        }

        //private void SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    //if (PhotoHubLLS != null)
        //    //{
        //    //    // Get item of LongListSelector.
        //    //    List<CustomUserControl> userControlList = new List<CustomUserControl>();
        //    //    GetItemsRecursive<CustomUserControl>(PhotoHubLLS, ref userControlList);

        //    //    // Selected.
        //    //    if (e.AddedItems.Count > 0 && e.AddedItems[0] != null)
        //    //    {
        //    //        foreach (CustomUserControl userControl in userControlList)
        //    //        {
        //    //            if (e.AddedItems[0].Equals(userControl.DataContext))
        //    //            {
        //    //                VisualStateManager.GoToState(userControl, "Selected", true);
        //    //            }
        //    //        }
        //    //    }

        //    //    // Removed.
        //    //    if (e.RemovedItems.Count > 0 && e.RemovedItems[0] != null)
        //    //    {
        //    //        foreach (CustomUserControl userControl in userControlList)
        //    //        {
        //    //            if (e.RemovedItems[0].Equals(userControl.DataContext))
        //    //            {
        //    //                VisualStateManager.GoToState(userControl, "Normal", true);
        //    //            }
        //    //        }
        //    //    }
        //    //}        
        //}

        ///// <summary>
        ///// Recursively get the item.
        ///// </summary>
        ///// <typeparam name="T">The item to get.</typeparam>
        ///// <param name="parents">Parent container.</param>
        ///// <param name="objectList">Item list</param>
        //public static void GetItemsRecursive<T>(DependencyObject parents, ref List<T> objectList) where T : DependencyObject
        //{
        //    var childrenCount = VisualTreeHelper.GetChildrenCount(parents);

        //    for (int i = 0; i < childrenCount; i++)
        //    {
        //        var child = VisualTreeHelper.GetChild(parents, i);

        //        if (child is T)
        //        {
        //            objectList.Add(child as T);
        //        }

        //        GetItemsRecursive<T>(child, ref objectList);
        //    }

        //    return;
        //}

    }
}