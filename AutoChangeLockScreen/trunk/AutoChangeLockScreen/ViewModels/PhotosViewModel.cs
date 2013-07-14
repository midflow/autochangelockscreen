// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Linq;
using AutoChangeLockScreen.Helpers;
using AutoChangeLockScreen.Models;
using AutoChangeLockScreen.Services;

namespace AutoChangeLockScreen.ViewModels
{
    public class PhotosViewModel
    {
        public List<KeyedList<string, Photo>> GroupedPhotos
        {
            get
            {
                var photos = DataService.GetPhotos();

                var groupedPhotos =
                    from photo in photos
                    orderby photo.Title
                    group photo by photo.Title.ToString() into photosByTitle
                    select new KeyedList<string, Photo>(photosByTitle);

                return new List<KeyedList<string, Photo>>(groupedPhotos);
            }
        }
    }
}
