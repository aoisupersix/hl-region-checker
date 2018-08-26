using HLRegionChecker.Interfaces;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace HLRegionChecker.ViewModels
{
    public class AppInfoPageViewModel: ViewModelBase
    {
        #region フィールド
        /// <summary>
        /// MainMasterPageのVM
        /// </summary>
        private MainMasterPageViewModel _mainViewModel;

        /// <summary>
        /// 遷移関係の処理を行うナビゲーションサービス
        /// </summary>
        private INavigationService navigationService;
        #endregion

        #region プロパティ
        /// <summary>
        /// アプリアイコン
        /// </summary>
        public ImageSource AppIcon { get; } = ImageSource.FromResource("HLRegionChecker.Resources.Icon_AppIcon.png");

        /// <summary>
        /// アプリ名
        /// </summary>
        public string AppName { get; } = DependencyService.Get<IAssemblyService>().GetPackageName();

        /// <summary>
        /// バージョンコード
        /// </summary>
        public string VersionCode { get; } = DependencyService.Get<IAssemblyService>().GetVersionCode().ToString();

        /// <summary>
        /// バージョン名
        /// </summary>
        public string VersionName { get; } = DependencyService.Get<IAssemblyService>().GetVersionName();
        #endregion

        public AppInfoPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;
        }

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            //Masterのジェスチャーを最有効化
            if (_mainViewModel != null)
                _mainViewModel.IsGestureEnabled.Value = true;
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            //MainMasterPageからの遷移であればインスタンスを保持
            var keyName = typeof(MainMasterPageViewModel).Name;
            if (parameters.Any(x => x.Key.Equals(keyName)))
            {
                _mainViewModel = (MainMasterPageViewModel)parameters[keyName];
                _mainViewModel.IsGestureEnabled.Value = false;
            }
        }
    }
}
