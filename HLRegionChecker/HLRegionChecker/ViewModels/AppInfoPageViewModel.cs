using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace HLRegionChecker.ViewModels
{
    public class AppInfoPageViewModel: ViewModelBase
    {
        #region プロパティ
        /// <summary>
        /// アプリアイコン
        /// </summary>
        public ImageSource AppIcon { get; } = ImageSource.FromResource("HLRegionChecker.Resources.Icon_AppIcon.png");

        /// <summary>
        /// アプリ名
        /// </summary>
        public string AppName { get; } = "HLRegionChecker";
        #endregion

        public AppInfoPageViewModel(INavigationService navigationService) : base(navigationService)
        {

        }
    }
}
