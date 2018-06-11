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

namespace HLRegionChecker.ViewModels
{
	public class MainMasterPageViewModel : BindableBase
	{
        #region フィールド
        /// <summary>
        /// 画面遷移関係用
        /// </summary>
        private INavigationService NavigationService { get; }
        #endregion フィールド

        #region プロパティ
        /// <summary>
        /// ユーザ情報モデル
        /// </summary>
        public UserDataModel UserData { get; private set; }
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
        /// ListViewのItemを選択した際のコマンド
        /// </summary>
        public Command<MenuItem> ItemSelectedCommand { get; }
        #endregion プロパティ

        #region コンストラクタ
        /// <summary>
        /// デフォルトのコンストラクタです。
        /// </summary>
        public MainMasterPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService)
        {
            this.NavigationService = navigationService;

            //メニューの初期化
            MenuItems = new List<MenuItem>(new[]
            {
                new MenuItem
                {
                    Icon = ImageSource.FromResource("HLRegionChecker.Resources.Icon_SelfUpdate.png"),
                    Title = "ステータス更新",
                },
                new MenuItem
                {
                    Icon = ImageSource.FromResource("HLRegionChecker.Resources.Icon_IdentifierSelection.png"),
                    Title = "ユーザ識別子選択",
                },
                new MenuItem
                {
                    Icon = ImageSource.FromResource("HLRegionChecker.Resources.Icon_UserStatus.png"),
                    Title = "各種情報"
                }
            });

            //メニューが選択された際の処理
            ItemSelectedCommand = new Command<MenuItem>((item) =>
            {
                //TODO: ここの処理が気持ち悪いから後でなんとかする
                switch(item.Title)
                {
                    case "ステータス更新":
                        IsPresented.Value = false;
                        SelfUpdate(pageDialogService);
                        break;
                    case "ユーザ識別子選択":
                        PresentIdentifierSelectPage();
                        break;
                    case "各種情報":
                        //とりあえず仮対応
                        //ユーザ識別子が選択されていない場合は遷移しない
                        if(UserDataModel.Instance.MemberId.HasValue)
                            PresentMyStatusDetailPage();
                        break;
                    default:
                        break;
                }
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

        /// <summary>
        /// 識別子選択ページに遷移します
        /// </summary>
        private async void PresentIdentifierSelectPage()
        {
            IsGestureEnabled.Value = false;
            IsPresented.Value = false;
            await NavigationService.NavigateAsync("NavigationPage/StatusDetailPage/IdentifierSelectPage");
            IsGestureEnabled.Value = true;
        }

        private async void PresentMyStatusDetailPage()
        {
            IsGestureEnabled.Value = false;
            IsPresented.Value = false;
            await NavigationService.NavigateAsync("NavigationPage/StatusDetailPage/MyStatusDetailPage");
            IsGestureEnabled.Value = true;
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
    }
}
