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
        public static List<myImages> GetImages()
        {            
            return App.imageList;
        }
    }
}
