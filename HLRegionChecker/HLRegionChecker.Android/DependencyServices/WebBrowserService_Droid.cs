using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HLRegionChecker.Models;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Firebase.Database;
using HLRegionChecker.Droid.DependencyServices;
using Reactive.Bindings;
using HLRegionChecker.Interfaces;

[assembly: Dependency(typeof(WebBrowserService_Droid))]
namespace HLRegionChecker.Droid.DependencyServices
{
    public class WebBrowserService_Droid : IWebBrowserService
    {
        public void Open(Uri uri)
        {
            Android.App.Application.Context.StartActivity(
                new Intent(Intent.ActionView,
                           global::Android.Net.Uri.Parse(uri.AbsoluteUri))
            );
        }
    }
}