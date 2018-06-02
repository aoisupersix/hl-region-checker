using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HLRegionChecker.ViewModels
{
    public class StatusDetailPageViewModel : ViewModelBase
    {
        public StatusDetailPageViewModel(INavigationService navigationService) 
            : base (navigationService)
        {
            Title = "Status Detail";
        }
    }
}
