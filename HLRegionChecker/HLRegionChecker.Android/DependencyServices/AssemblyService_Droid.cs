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

[assembly: Dependency(typeof(AssemblyService_Droid))]
namespace HLRegionChecker.Droid.DependencyServices
{
    class AssemblyService_Droid: IAssemblyService
    {
        public string GetPackageName()
        {
            var context = Android.App.Application.Context;
            return context.PackageManager.GetPackageInfo(context.PackageName, 0).PackageName;
        }

        public string GetVersionName()
        {
            var context = Android.App.Application.Context;
            return context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName;
        }

        public int GetVersionCode()
        {
            var context = Android.App.Application.Context;
            return context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionCode;
        }
    }
}