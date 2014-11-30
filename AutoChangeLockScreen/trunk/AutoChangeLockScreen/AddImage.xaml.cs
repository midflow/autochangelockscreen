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
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Xna.Framework.Media;
using System.IO;
using Telerik.Windows.Controls;
using AutoChangeLockScreen;
using AutoChangeLockScreen.Models;
using AutoChangeLockScreen.Resources;
using System.IO.IsolatedStorage;

namespace AutoChangeLockScreen
{
    public partial class AddImage : PhoneApplicationPage
    {
        //private List<PictureModel> source;
        public static WriteableBitmap image;
        static bool ShowImageTask;
        public AddImage()
        {
            InitializeComponent();
            this.DataContext = AddImage.image;
            //this.source = new List<PictureModel>();
            ShowImageTask = false;
        }
       

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (!ShowImageTask)
            {
                base.OnNavigatedTo(e);

                PhotoChooserTask task = new PhotoChooserTask();
                task.Completed += task_Completed;
                task.ShowCamera = true;
                task.PixelWidth = 768;
                task.PixelHeight = 1280;
                ShowImageTask = true;
                task.Show();
            }
        }
        private void task_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                this.OpenImageEditor(e.ChosenPhoto);
            }
            else
            {
                NavigationService.GoBack();
            }
        }

        private void OpenImageEditor(Stream imageStream)
        {
            WriteableBitmap image = new WriteableBitmap(768, 1280);
            image.SetSource(imageStream);
            imageStream.Dispose();

            AddImage.image = image;
            imageEditor.Source = image;
            //MainViewModel.Instance.NavigateTo(new NavigationInfo() { PageUri = "/ImageEditor/Pages/EditorPage.xaml" });
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
            App.imgName = "image_" + App.imageList.Count.ToString() + ".jpg";
            App.FullImgName = App.imgName;
            CopyImage(App.imgName);
            myImages imageData = new myImages(App.imgName, false);
            App.imageList.Add(imageData);
            this.GoBack();
        }

        private void RadImageEditor_ImageEditCancelled(object sender, ImageEditCancelledEventArgs e)
        {
            this.GoBack();
        }

        private void GoBack()
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
                this.DataContext = null;
                image = null;
            }
        }

        private void imageEditor_ImageSaving(object sender, ImageSavingEventArgs e)
        {                        
            e.FileName = "image_" + App.imageList.Count.ToString();
            
        }
        private void CopyImage(string strImageName)
        {
            using (MediaLibrary defaultLib = new MediaLibrary())
            {
                PictureAlbum savedPicturesAlbum = defaultLib.RootPictureAlbum.Albums.FirstOrDefault<PictureAlbum>(album => album.Name.Contains("Saved"));
                if (savedPicturesAlbum == null)
                {
                    return;
                }

                IEnumerable<Picture> pictures = savedPicturesAlbum.Pictures.Where<Picture>(pic => pic.Name.Contains(strImageName.Split('.')[0].ToString()));

                foreach (Picture p in pictures)
                {
                    //this.source.Add(new PictureModel(p));
                    image.SetSource(p.GetImage());
                    using (MemoryStream stream = new MemoryStream())
                    {
                        image.SaveJpeg(stream, (int)p.Width, (int)p.Height, 0, 100);

                        using (var local = new IsolatedStorageFileStream(App.FullImgName, FileMode.Create, IsolatedStorageFile.GetUserStoreForApplication()))
                        {
                            local.Write(stream.GetBuffer(), 0, stream.GetBuffer().Length);
                        }
                    }
                }                
            }
        }
    }
   
}