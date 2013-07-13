// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using AutoChangeLockScreen.Models;

namespace AutoChangeLockScreen.Services
{
    public static class DataService
    {
        public static List<Photo> GetPhotos()
        {
            List<Photo> imageList = new List<Photo>();
            Random _rnd = new Random(42 * 42);
            DateTime start = new DateTime(2010, 1, 1);

            for (int i = 0; i < 15; i++)
            {
                Photo imageData = new Photo()
                {
                    ImageSource = new Uri(String.Format("/Content/{0}.jpg", i), UriKind.Relative),
                    Title = i.ToString(),
                    TimeStamp = start.AddDays(_rnd.Next(0, 450)),
                    Selected = false
                };

                imageList.Add(imageData);
            }

            return imageList;
        }
    }
}
