using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
//using Telerik.QSF.WP;
using Telerik.Windows.Controls;

namespace Telerik.Examples.WP.ImageEditor.Pages
{
    public partial class PickImagePage : PhoneApplicationPage
    {
        private List<PictureModel> source;
        public PickImagePage()
        {
            InitializeComponent();
            this.source = new List<PictureModel>();
            this.Loaded += PickImagePage_Loaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            EditorPage.image = null;
        }

        private void PickImagePage_Loaded(object sender, RoutedEventArgs e)
        {
            this.picturesList.ItemsSource = null;
            this.source.Clear();

            this.RefreshSavedImages();

            this.txtPhotoCount.Text = this.source.Count.ToString();
            this.picturesList.ItemsSource = this.source;
        }

        private void RefreshSavedImages()
        {
            using (MediaLibrary defaultLib = new MediaLibrary())
            {
                PictureAlbum savedPicturesAlbum = defaultLib.RootPictureAlbum.Albums.FirstOrDefault<PictureAlbum>(album => album.Name.Contains("Saved"));
                if (savedPicturesAlbum == null)
                {
                    return;
                }

                IEnumerable<Picture> pictures = savedPicturesAlbum.Pictures.Where<Picture>(pic => pic.Name.Contains("telerik_edited_image"));

                foreach (Picture p in pictures)
                {
                    this.source.Add(new PictureModel(p));
                }
            }
        }

        private void btnNewPhoto_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PhotoChooserTask task = new PhotoChooserTask();
            task.Completed += task_Completed;
            task.ShowCamera = true;
            task.PixelWidth = 1024;
            task.PixelHeight = 768;
            task.Show();
        }

        private void task_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                this.OpenImageEditor(e.ChosenPhoto);
            }
        }

        private void picturesList_ItemTap(object sender, Windows.Controls.ListBoxItemTapEventArgs e)
        {
            this.OpenImageEditor((e.Item.Content as PictureModel).GetStream());
        }

        private void OpenImageEditor(Stream imageStream)
        {
            WriteableBitmap image = new WriteableBitmap(1024, 768);
            image.SetSource(imageStream);
            imageStream.Dispose();

            EditorPage.image = image;
            //MainViewModel.Instance.NavigateTo(new NavigationInfo() { PageUri = "/ImageEditor/Pages/EditorPage.xaml" });
        }
    }

    public class PictureModel
    {
        private WeakReference<ImageSource> picture;
        private Picture libraryModel;

        public PictureModel(Picture libPic)
        {
            this.libraryModel = libPic;
        }

        public Stream GetStream()
        {
            return this.libraryModel.GetImage();
        }

        public ImageSource PictureSource
        {
            get
            {
                ImageSource s = null;

                if (this.picture != null && this.picture.TryGetTarget(out s))
                {
                    return s;
                }
                else
                {
                    BitmapImage bim = new BitmapImage();
                    using (Stream str = libraryModel.GetThumbnail())
                    {
                        bim.SetSource(str);
                    }
                    s = bim;
                    this.picture = new WeakReference<ImageSource>(bim);
                }

                return s;
            }
        }
    }
}