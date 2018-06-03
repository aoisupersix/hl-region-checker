using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using HLRegionChecker.Models;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Reactive.Bindings;

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
        public ReactiveProperty<bool> IsPresented { get; set; }
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
        public MainMasterPageViewModel(INavigationService navigationService)
        {
            this.NavigationService = navigationService;

            //メニューの初期化
            MenuItems = new List<MenuItem>(new[]
            {
                new MenuItem
                {
                    Icon = ImageSource.FromResource("HLRegionChecker.Resources.Icon_IdentifierSelection.png"),
                    Title = "ユーザ識別子選択",
                },
                new MenuItem
                {
                    Icon = ImageSource.FromResource("HLRegionChecker.Resources.Icon_SelfUpdate.png"),
                    Title = "ステータス更新",
                },
            });

            //メニューが選択された際の処理
            ItemSelectedCommand = new Command<MenuItem>((item) =>
            {
                //TODO: ここの処理が気持ち悪いから後でなんとかする
                switch(item.Title)
                {
                    case "ユーザ識別子選択":
                        NavigationService.NavigateAsync("NavigationPage/MainMasterPage/IdentifierSelectPage");
                        IsPresented.Value = false;
                        break;
                    case "ステータス更新":
                        break;
                    default:
                        break;
                }
            });
        }
        #endregion コンストラクタ
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
