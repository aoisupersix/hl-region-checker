using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace HLRegionChecker.ViewModels
{
	public class MainMasterPageViewModel : BindableBase
	{
        /// <summary>
        /// Masterのメニューアイテム
        /// </summary>
        public List<MenuItem> MenuItems { get; private set; }
        /// <summary>
        /// Masterのアプリアイコン
        /// </summary>
        public ImageSource AppIcon { get; private set; } = ImageSource.FromResource("HLRegionChecker.Resources.Icon_AppIcon.png");

        /// <summary>
        /// デフォルトのコンストラクタです。
        /// </summary>
        public MainMasterPageViewModel()
        {
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
