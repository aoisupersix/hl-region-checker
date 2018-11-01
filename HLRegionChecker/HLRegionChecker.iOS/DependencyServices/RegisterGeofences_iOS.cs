using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using HLRegionChecker.Models;
using HLRegionChecker.Interfaces;

using Firebase.Database;
using HLRegionChecker.iOS.DependencyServices;
using Foundation;

[assembly: Dependency(typeof(RegisterGeofences_iOS))]
namespace HLRegionChecker.iOS.DependencyServices
{
    class RegisterGeofences_iOS : IRegisterGeofences
    {
        public void Register() { }
    }
}