using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
//using Telerik.QSF.WP;
using Telerik.Windows.Controls;

namespace Telerik.Examples.WP.ImageEditor.Pages
{
    public partial class EditorPage : PhoneApplicationPage
    {
        public static WriteableBitmap image;

        public EditorPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.DataContext = EditorPage.image;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            e.Cancel = this.imageEditor.CurrentTool != null;
            if (e.Cancel)
            {
                this.imageEditor.CurrentTool = null;
            }
        }

        private void RadImageEditor_ImageSaved(object sender, ImageSavedEventArgs e)
        {
            this.GoBack();
        }

        private void RadImageEditor_ImageEditCancelled(object sender, ImageEditCancelledEventArgs e)
        {
            this.GoBack();
        }

        private void GoBack()
        {
            //if (MainViewModel.Instance.CurrentPage.NavigationService.CanGoBack)
            //{
            //    MainViewModel.Instance.CurrentPage.NavigationService.GoBack();
            //    this.DataContext = null;
            //    image = null;
            //}
        }
    }
}