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
using HLRegionChecker.Interfaces;
using HLRegionChecker.Droid.Geofences;

[assembly: Dependency(typeof(RegisterGeofences_Droid))]
namespace HLRegionChecker.Droid.DependencyServices
{
    public class RegisterGeofences_Droid: IRegisterGeofences
    {
        private RegisterGeofences _registerGeofences;

        public RegisterGeofences_Droid()
        {
            var context = Android.App.Application.Context;
            _registerGeofences = new RegisterGeofences(context);
        }
        public void Register()
        {
            _registerGeofences.AddGeofences();
        }
    }
}