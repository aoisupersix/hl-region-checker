using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using HLRegionChecker.Models;
using HLRegionChecker.Interfaces;

using Firebase.Database;
using HLRegionChecker.iOS.DependencyServices;
using Foundation;
using UIKit;

[assembly: Dependency(typeof(WebBrowserService_iOS))]
namespace HLRegionChecker.iOS.DependencyServices
{
    public class WebBrowserService_iOS : IWebBrowserService
    {
        public void Open(Uri uri)
        {
            UIApplication.SharedApplication.OpenUrl(uri);
        }
    }
}