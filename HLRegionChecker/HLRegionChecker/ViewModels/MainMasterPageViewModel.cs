using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using HLRegionChecker.Models;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Reactive.Bindings;
using Prism.Services;
using System;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using HLRegionChecker.Interfaces;

namespace HLRegionChecker.ViewModels
{
	public class MainMasterPageViewModel : ViewModelBase
	{
        #region プロパティ
        /// <summary>
        /// プロパティの監視管理
        /// </summary>
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();
        /// <summary>
        /// メニューを表示しているか？
        /// </summary>
        public ReactiveProperty<bool> IsPresented { get; set; } = new ReactiveProperty<bool>(false);
        /// <summary>
        /// メニューのジェスチャーが有効か？
        /// </summary>
        public ReactiveProperty<bool> IsGestureEnabled { get; set; } = new ReactiveProperty<bool>(true);
        /// <summary>
        /// Masterのメニューアイテム
        /// </summary>
        public ReactiveCollection<MenuItem> MenuItems { get; private set; }
        /// <summary>
        /// Masterのアプリアイコン
        /// </summary>
        public ImageSource AppIcon { get; private set; } = ImageSource.FromResource("HLRegionChecker.Resources.Icon_AppIcon.png");
        /// <summary>
        /// Masterのメンバー名
        /// </summary>
        public ReactiveProperty<string> MemberName { get; set; }
        /// <summary>
        /// ListViewのItemを選択した際のコマンド
        /// </summary>
        public Command<MenuItem> ItemSelectedCommand { get; }
        #endregion プロパティ

        #region コンストラクタ
        /// <summary>
        /// デフォルトのコンストラクタです。
        /// </summary>
        public MainMasterPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService) : base(navigationService)
        {
            MemberName = DbModel.Instance.ObserveProperty(x => x.MemberDisplayName).ToReactiveProperty().AddTo(Disposable);

            //メニューの初期化
            MenuItems = new ReactiveCollection<MenuItem>();
            MenuItems.Add(new MenuItem
            (
                ImageSource.FromResource("HLRegionChecker.Resources.Icon_SelfUpdate.png"),
                "ステータス更新",
                () => { SelfUpdate(pageDialogService); }
            ));
            MenuItems.Add(new MenuItem
            (
                ImageSource.FromResource("HLRegionChecker.Resources.Icon_IdentifierSelection.png"),
                "ユーザ識別子選択",
                () =>
                {
                    NavigationService.NavigateAsync("NavigationPage/StatusListPage/IdentifierSelectPage", new NavigationParameters { { typeof(MainMasterPageViewModel).Name, this } });
                }
            ));
            MenuItems.Add(new MenuItem
            (
                ImageSource.FromResource("HLRegionChecker.Resources.Icon_Open.png"),
                "Webで確認する",
                () =>
                {
                    Uri uri = new Uri("https://hlmanager-32609.firebaseapp.com/");
                    Xamarin.Forms.DependencyService.Get<IWebBrowserService>().Open(uri);
                }
            ));
            MenuItems.Add(new MenuItem
            (
                ImageSource.FromResource("HLRegionChecker.Resources.Icon_Info.png"),
                "アプリ情報",
                () =>
                {
                    NavigationService.NavigateAsync("NavigationPage/StatusListPage/AppInfoPage", new NavigationParameters { { typeof(MainMasterPageViewModel).Name, this } });
                }
            ));

            if (Device.RuntimePlatform == Device.Android)
            {
                // Droid専用メニュー
                MenuItems.Add(new MenuItem
                (
                    ImageSource.FromResource("HLRegionChecker.Resources.Icon_RegisterGeofences.png"),
                    "ジオフェンス再登録",
                    () =>
                    {
                        var rgAdapter = Xamarin.Forms.DependencyService.Get<IRegisterGeofences>();
                        rgAdapter.Register();
                        pageDialogService.DisplayAlertAsync("完了", "ジオフェンスの再登録処理を行いました。", "OK");
                    }
                ));

                var beaconServiceMenu = new MenuItem();
                MenuItems.Add(beaconServiceMenu);
                RegisterBeaconServiceMenu(beaconServiceMenu, pageDialogService);
            }

            //メニューが選択された際の処理
            ItemSelectedCommand = new Command<MenuItem>((item) =>
            {
                IsPresented.Value = false;
                item.OnSelectedAction();
            });
        }
        #endregion コンストラクタ

        /// <summary>
        /// ステータス手動更新ダイアログを表示し、選択されたステータスで更新します。
        /// </summary>
        /// <param name="pageDialogService"></param>
        private async void SelfUpdate(IPageDialogService pageDialogService)
        {
            var ret = await pageDialogService.DisplayActionSheetAsync(
                "ステータス手動更新",
                "Cancel",
                null,
                DbModel.Instance.States.Select(s => s.Name).ToArray());
            if(DbModel.Instance.States.Select(s => s.Name).Contains(ret))
            {
                var state = DbModel.Instance.States.Where(s => s.Name.Equals(ret)).Select(s => s.Id).First();
                DbModel.Instance.UpdateState(state);
            }
        }

        private async void DisableBeaconService(MenuItem item, IPageDialogService pageDialogService)
        {
            var ret = await pageDialogService.DisplayAlertAsync(
                "ビーコンフォアグラウンドサービス無効化",
                "ビーコンのフォアグラウンドサービスを無効にし、スキャンジョブに切り替えます。「Scanning for beacons」の通知は表示されなくなりますが、ビーコンの検知に最大15分掛かります。よろしいですか？",
                "OK", "キャンセル");

            if (false == ret)
                return;

            UserDataModel.Instance.IsUseForegroundService = false;
            await pageDialogService.DisplayAlertAsync("完了", "フォアグラウンドサービスを無効化しました。適用にはアプリを再起動する必要があります。設定＞アプリから強制停止を行い、もう一度起動して下さい。", "OK");

            RegisterBeaconServiceMenu(item, pageDialogService);
        }

        private async void EnableBeaconService(MenuItem item, IPageDialogService pageDialogService)
        {
            var ret = await pageDialogService.DisplayAlertAsync(
                "ビーコンフォアグラウンドサービス有効化",
                "ビーコンのスキャンジョブを無効にし、フォアグラウンドサービスに切り替えます。ビーコンの検知は即時に行われるようになりますが、「Scanning for beacons」の通知は表示されアプリがバックグラウンドで動作します。よろしいですか？",
                "OK", "キャンセル");

            if (false == ret)
                return;

            UserDataModel.Instance.IsUseForegroundService = true;
            await pageDialogService.DisplayAlertAsync("完了", "フォアグラウンドサービスを有効化しました。適用にはアプリを再起動する必要があります。", "OK");

            RegisterBeaconServiceMenu(item, pageDialogService);
        }

        private void RegisterBeaconServiceMenu(MenuItem item, IPageDialogService pageDialogService)
        {
            if (UserDataModel.Instance.IsUseForegroundService)
            {
                // フォアグラウンドサービス無効化
                item.Icon.Value = ImageSource.FromResource("HLRegionChecker.Resources.Icon_DisableForegroundService.png");
                item.Title.Value = "ビーコンサービス無効化";
                item.OnSelectedAction = () => DisableBeaconService(item, pageDialogService);
            }
            else
            {
                // フォアグラウンドサービス有効化
                item.Icon.Value = ImageSource.FromResource("HLRegionChecker.Resources.Icon_EnableForegroundService.png");
                item.Title.Value = "ビーコンサービス有効化";
                item.OnSelectedAction = () => EnableBeaconService(item, pageDialogService);
            }
        }

        public override void Destroy()
        {
            Dispose();
            base.Destroy();
        }

        /// <summary>
        /// プロパティの変更監視を終了します。
        /// </summary>
        public void Dispose()
        {
            this.Disposable.Dispose();
        }
    }

    /// <summary>
    /// メニューで表示するアイテムです。
    /// </summary>
    public class MenuItem
    {
        public MenuItem(ImageSource icon = null, string title = null, Action selectedAction = null)
        {
            Icon.Value = icon;
            Title.Value = title;
            OnSelectedAction = selectedAction;
        }

        /// <summary>
        /// メニューアイテムのアイコン
        /// </summary>
        public ReactiveProperty<ImageSource> Icon { get; set; } = new ReactiveProperty<ImageSource>();

        /// <summary>
        /// メニューアイテムのタイトル
        /// </summary>
        public ReactiveProperty<string> Title { get; set; } = new ReactiveProperty<string>();

        public Action OnSelectedAction { get; set; }
    }
}
