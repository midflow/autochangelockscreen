// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;

namespace AutoChangeLockScreen.Models
{
    public class myImages
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

        public bool ImageSeclected
        {
            get { return m_ImageSeclected; }
            set { m_ImageSeclected = value; }
        }

        public bool IsContextMenu
        {
            get { return m_IsContextMenu; }
            set { m_IsContextMenu = value; }
        }
        private bool m_IsContextMenu;
        private string m_ImageName;
        private bool m_ImageSeclected;
        private string m_ImageSize;
        private BitmapImage m_ImageBinary;
        public myImages(string strImageName, bool isSelected)
        {
            string[] strFileName = strImageName.Split('/');
            if (strImageName.Length > 1)
            {
               this.ImageName = strFileName[strFileName.Length - 1]; ;
            }
            else
            {
                this.ImageName = strImageName;
            }
            
            this.ImageSeclected = isSelected;
            //*** Image Binary ***'
            BitmapImage image = new BitmapImage();
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();            
            string isoFilename = strImageName;
            
            Stream stream = isoStore.OpenFile(isoFilename, System.IO.FileMode.Open);
            image.SetSource(stream);
            this.ImageBinary = image;
            //*** Image Size ***'
            this.ImageSize = stream.Length + " Bytes";
            stream.Close();
        }
    }
}