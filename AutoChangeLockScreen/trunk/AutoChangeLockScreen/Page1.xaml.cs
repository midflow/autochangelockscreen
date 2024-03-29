﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using Microsoft.Phone.Shell;
using System.IO;
using System.IO.IsolatedStorage;
using AutoChangeLockScreen;
using AutoChangeLockScreen.Models;
using AutoChangeLockScreen.Resources;

namespace Image_Tiles
{
    public partial class Page1 : PhoneApplicationPage
    {
        //private static Version TargetedVersion = new Version(7, 10, 8858);
        //public static bool IsTargetedVersion { get { return Environment.OSVersion.Version >= TargetedVersion; } }
        bool isMove = true;
        Rectangle r;

        int trX = 0;
        int trY = 0;

        int height = 600;
        int width = 360;

        //String ImageName;

        static bool ShowImageTask;

        public Page1()
        {
            InitializeComponent();
            ShowImageTask = false;
            BuildLocalizedApplicationBar();
            IApplicationBarIconButton button;
            button = (IApplicationBarIconButton)ApplicationBar.Buttons[1];
            button.IsEnabled = !isMove;
            button = (IApplicationBarIconButton)ApplicationBar.Buttons[0];
            button.IsEnabled = isMove;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (!ShowImageTask)
            {
                base.OnNavigatedTo(e);

                //App.blLoadIamge = true;

                PhotoChooserTask task = new PhotoChooserTask();
                task.ShowCamera = true;
                ShowImageTask = true;
                task.Show();
                task.Completed += new EventHandler<PhotoResult>(task_Completed);

            }
        }

        void task_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                BitmapImage image = new BitmapImage();
                image.SetSource(e.ChosenPhoto);
                image1.Source = image;
                if (image.PixelHeight / image.PixelWidth > 800 / 480)
                {
                    image1.Height = image.PixelHeight > 800 ? 800 : image.PixelHeight;
                    image1.Width = image1.Height * image.PixelWidth / image.PixelHeight;
                }
                else
                {
                    image1.Width = image.PixelWidth > 480 ? 480 : image.PixelWidth;
                    image1.Height = image1.Width * image.PixelHeight / image.PixelWidth;
                }
                SetPicture();
            }
            else
            {
                Dispatcher.BeginInvoke(() =>
                {
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                });
                //NavigationService.GoBack();
            }
        }

        void SetPicture()
        {
            if (image1.Height / image1.Width > 800 / 480)
            {
                width = (int) image1.Width;
                height = 800 * width / 480;
            }
            else
            {
                height = (int) image1.Height;
                width = 480 * height / 800;
            }

            Rectangle rect = new Rectangle();
            rect.Opacity = .5;
            rect.Fill = new SolidColorBrush(Colors.White);
            rect.Height = height;
            rect.MaxHeight = height;
            rect.MaxWidth = width;
            rect.Width = width;
            rect.Stroke = new SolidColorBrush(Colors.Red);
            rect.StrokeThickness = 2;
            rect.Margin = image1.Margin;
            rect.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(rect_ManipulationDelta);

            LayoutRoot.Children.Add(rect);
            LayoutRoot.Height = image1.Height;
            LayoutRoot.Width = image1.Width;
        }

        void rect_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            ////try
            ////{
            GeneralTransform gt = ((Rectangle)sender).TransformToVisual(LayoutRoot);
            Point p = gt.Transform(new Point(0, 0));

            int intermediateValueY = (int)((LayoutRoot.Height - ((Rectangle)sender).Height));
            int intermediateValueX = (int)((LayoutRoot.Width - ((Rectangle)sender).Width));
            Rectangle croppingRectangle = (Rectangle)sender;

            if (isMove)
            {
                TranslateTransform tr = new TranslateTransform();
                trX += (int)e.DeltaManipulation.Translation.X;
                trY += (int)e.DeltaManipulation.Translation.Y;

                if (trY < (-intermediateValueY / 2))
                {
                    trY = (-intermediateValueY / 2);
                }
                else if (trY > (intermediateValueY / 2))
                {
                    trY = (intermediateValueY / 2);
                }

                if (trX < (-intermediateValueX / 2))
                {
                    trX = (-intermediateValueX / 2);
                }
                else if (trX > (intermediateValueX / 2))
                {
                    trX = (intermediateValueX / 2);
                }

                tr.X = trX;
                tr.Y = trY;

                croppingRectangle.RenderTransform = tr;
            }
            else
            {
                if (e.DeltaManipulation.Translation.X < 0 || croppingRectangle.Width > 50)
                {
                    if (p.X >= 0)
                    {
                        if (p.X <= intermediateValueX)
                        {
                            croppingRectangle.Width -= (int)e.DeltaManipulation.Translation.X;
                            croppingRectangle.Height = croppingRectangle.Width * height / width;
                        }
                        else
                        {
                            croppingRectangle.Width -= (p.X - intermediateValueX);
                            croppingRectangle.Height = croppingRectangle.Width * height / width;
                        }
                    }
                    else
                    {
                        croppingRectangle.Width -= Math.Abs(p.X);
                        croppingRectangle.Height = croppingRectangle.Width * height / width;
                    }
                }

                if (e.DeltaManipulation.Translation.Y < 0 || croppingRectangle.Height > 50)
                {
                    if (p.Y >= 0)
                    {
                        if (p.Y <= intermediateValueY)
                        {
                            croppingRectangle.Height -= (int)e.DeltaManipulation.Translation.Y;
                            croppingRectangle.Width = croppingRectangle.Height * width / height;
                        }
                        else
                        {
                            croppingRectangle.Height -= (p.Y - intermediateValueY);
                            croppingRectangle.Width = croppingRectangle.Height * width / height;
                        }
                    }
                    else
                    {
                        croppingRectangle.Height -= Math.Abs(p.Y);
                        croppingRectangle.Width = croppingRectangle.Height * width / height;
                    }
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString()) ;
            //}
        }

        private void btn_Click(object sender, EventArgs e)
        {
            IApplicationBarIconButton button;
            if (isMove)
            {
                button = (IApplicationBarIconButton)ApplicationBar.Buttons[1];
                button.IsEnabled = true;
                button = (IApplicationBarIconButton)ApplicationBar.Buttons[0];
                button.IsEnabled = false;
                isMove = false;
            }
            else
            {
                button = (IApplicationBarIconButton)ApplicationBar.Buttons[1];
                button.IsEnabled = false;
                button = (IApplicationBarIconButton)ApplicationBar.Buttons[0];
                button.IsEnabled = true;
                isMove = true;
            }
        }

        private void Accept_Click(object sender, EventArgs e)
        {
            App.imgName = "image_" + App.imageList.Count.ToString() + ".jpg";
            App.FullImgName = App.imgName;

            ClipImage();

            WriteBitmap(LayoutRoot);

            var image = new BitmapImage();

            using (var local = new IsolatedStorageFileStream(App.FullImgName, FileMode.Open, FileAccess.Read, IsolatedStorageFile.GetUserStoreForApplication()))
            {
                image.SetSource(local);
            }

            //image.SetSource(App.LoadFile(App.imgName));            

            WriteDummyImage(image);

            myImages imageData = new myImages(App.imgName, false);

            App.imageList.Add(imageData);

            //NavigationService.Navigate(new Uri("/MainPage.xaml?imagename=" + ImageName, UriKind.Relative));

            NavigationService.GoBack();
        }

        private void WriteDummyImage(BitmapImage image)
        {
            try
            {
                Image imageC = new Image();
                imageC.Stretch = Stretch.None;
                imageC.Source = image;
                imageC.Height = r.Height;
                imageC.Width = r.Width;

                WriteBitmap(imageC);
            }
            catch (IsolatedStorageException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        void ClipImage()
        {
            RectangleGeometry geo = new RectangleGeometry();

            r = (Rectangle)(from c in LayoutRoot.Children where c.Opacity == .5 select c).First();
            GeneralTransform gt = r.TransformToVisual(LayoutRoot);
            Point p = gt.Transform(new Point(0, 0));
            geo.Rect = new Rect(p.X, p.Y, r.Width, r.Height);
            image1.Clip = geo;
            r.Visibility = System.Windows.Visibility.Collapsed;

            TranslateTransform t = new TranslateTransform();
            t.X = -p.X;
            t.Y = -p.Y;
            image1.RenderTransform = t;
        }

        void WriteBitmap(FrameworkElement element)
        {
            try
            {
                WriteableBitmap wBitmap = new WriteableBitmap(element, null);

                using (MemoryStream stream = new MemoryStream())
                {
                    wBitmap.SaveJpeg(stream, (int)element.Width, (int)element.Height, 0, 100);

                    using (var local = new IsolatedStorageFileStream(App.FullImgName, FileMode.Create, IsolatedStorageFile.GetUserStoreForApplication()))
                    {
                        local.Write(stream.GetBuffer(), 0, stream.GetBuffer().Length);
                    }
                }
                //using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                //{
                //    if (isf.FileExists(imgName))
                //        isf.DeleteFile(imgName);
                //    using (IsolatedStorageFileStream isfs = isf.CreateFile(imgName))
                //    {
                //        var bmp = new WriteableBitmap(element, null);
                //        bmp.SaveJpeg(isfs, (int)element.Width, (int)element.Height, 0, 100);                        
                //    }                    
                //}
            }
            catch (IsolatedStorageException exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/LoadImages.xaml", UriKind.Relative));
            });
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.BackgroundColor = App.GetColorFromHexString("FF08317B");
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 0.8;
            LocalizedButtonBar("/Assets/AppBar/edit.png", AppResources.Resize, btn_Click);
            LocalizedButtonBar("/Assets/AppBar/appbar.map.position.rest.png", AppResources.Move, btn_Click);
            LocalizedButtonBar("/Assets/AppBar/photo.crop.png", AppResources.Accept, Accept_Click);
            LocalizedButtonBar("/Assets/AppBar/close.png", AppResources.Cancel, btnCancel_Click); 
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
        
    }
}