using HLRegionChecker.Const;
using HLRegionChecker.Interfaces;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
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
        /// Githubにジャンプするコマンド
        /// </summary>
        public ICommand JumpGithubCommand { get; }

        /// <summary>
        /// アプリアイコン
        /// </summary>
        public ImageSource AppIcon { get; } = ImageSource.FromResource("HLRegionChecker.Resources.Icon_AppIcon.png");

        /// <summary>
        /// アプリ名
        /// </summary>
        public string AppName { get; } = DependencyService.Get<IAssemblyService>().GetAppName();

        /// <summary>
        /// バージョンコード
        /// </summary>
        public string VersionCode { get; } = DependencyService.Get<IAssemblyService>().GetVersionCode().ToString();

        /// <summary>
        /// バージョン名
        /// </summary>
        public string VersionName { get; } = DependencyService.Get<IAssemblyService>().GetVersionName();

        /// <summary>
        /// ビーコンUUID
        /// </summary>
        public string BeaconUuid { get; } = RegionConst.BEACON_UUID;

        /// <summary>
        /// ビーコンMajor値
        /// </summary>
        public int BeaconMajorValue { get; } = RegionConst.BEACON_MAJOR;

        /// <summary>
        /// ビーコンMinor値
        /// </summary>
        public int BeaconMinorValue { get; } = RegionConst.BEACON_MINOR;

        /// <summary>
        /// ビーコン(研究室）識別子
        /// </summary>
        public string BeaconIdentifier { get; } = Region.研究室.GetIdentifier();
        #endregion

        public AppInfoPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            this.navigationService = navigationService;

            JumpGithubCommand = new DelegateCommand(() =>
            {
                Uri uri = new Uri("https://github.com/aoisupersix/hl-region-checker");
                Xamarin.Forms.DependencyService.Get<IWebBrowserService>().Open(uri);
            });
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
