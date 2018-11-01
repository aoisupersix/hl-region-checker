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
        public List<MenuItem> MenuItems { get; private set; }
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
            MenuItems = new List<MenuItem>(new[]
            {
                new MenuItem
                {
                    Icon = ImageSource.FromResource("HLRegionChecker.Resources.Icon_SelfUpdate.png"),
                    Title = "ステータス更新",
                    OnSelectedAction = () =>
                    {
                        SelfUpdate(pageDialogService);
                    }
                },
                new MenuItem
                {
                    Icon = ImageSource.FromResource("HLRegionChecker.Resources.Icon_IdentifierSelection.png"),
                    Title = "ユーザ識別子選択",
                    OnSelectedAction = () =>
                    {
                        NavigationService.NavigateAsync("NavigationPage/StatusListPage/IdentifierSelectPage", new NavigationParameters { { typeof(MainMasterPageViewModel).Name, this } });
                    }
                },
                new MenuItem
                {
                    Icon = ImageSource.FromResource("HLRegionChecker.Resources.Icon_Open.png"),
                    Title = "Webで確認する",
                    OnSelectedAction = () =>
                    {
                        Uri uri = new Uri("https://hlmanager-32609.firebaseapp.com/");
                        Xamarin.Forms.DependencyService.Get<IWebBrowserService>().Open(uri);
                    }
                },
                new MenuItem
                {
                    Icon = ImageSource.FromResource("HLRegionChecker.Resources.Icon_Info.png"),
                    Title = "アプリ情報",
                    OnSelectedAction = () =>
                    {
                        NavigationService.NavigateAsync("NavigationPage/StatusListPage/AppInfoPage", new NavigationParameters { { typeof(MainMasterPageViewModel).Name, this } });
                    }
                }
            });

            if (Device.RuntimePlatform == Device.Android)
            {
                // Droid専用メニュー
                MenuItems.Add(new MenuItem
                {
                    Icon = null,
                    Title = "ジオフェンス再登録",
                    OnSelectedAction = () =>
                    {
                        var rgAdapter = Xamarin.Forms.DependencyService.Get<IRegisterGeofences>();
                        rgAdapter.Register();
                        pageDialogService.DisplayAlertAsync("完了", "ジオフェンスの再登録処理を行いました。", "OK");
                    }
                });
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
        /// <summary>
        /// メニューアイテムのアイコン
        /// </summary>
        public ImageSource Icon { get; set; }

        /// <summary>
        /// メニューアイテムのタイトル
        /// </summary>
        public string Title { get; set; }

        public Action OnSelectedAction { get; set; }
    }
}
