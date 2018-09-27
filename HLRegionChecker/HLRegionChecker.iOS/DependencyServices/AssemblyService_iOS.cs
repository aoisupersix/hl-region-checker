using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using HLRegionChecker.Models;
using HLRegionChecker.Interfaces;

using Firebase.Database;
using HLRegionChecker.iOS.DependencyServices;
using Foundation;

[assembly: Dependency(typeof(AssemblyService_iOS))]
namespace HLRegionChecker.iOS.DependencyServices
{
    class AssemblyService_iOS : IAssemblyService
    {
        public string GetPackageName()
        {
            return NSBundle.MainBundle.InfoDictionary["CFBundleDisplayName"].ToString();
        }

        public string GetAppName()
        {
            return NSBundle.MainBundle.InfoDictionary["CFBundleDisplayName"].ToString();
        }

        public int GetVersionCode()
        {
            return int.Parse(NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString());
        }

        public string GetVersionName()
        {
            return NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
        }
    }
}