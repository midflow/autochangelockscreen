// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.ComponentModel;

namespace AutoChangeLockScreen.Models
{
    public class Photo
    {
        public string Title { get; set; }
        public Uri ImageSource { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool Selected { get; set; }
    }
}