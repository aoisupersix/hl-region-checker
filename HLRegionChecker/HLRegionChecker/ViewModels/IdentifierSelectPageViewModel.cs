using HLRegionChecker.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace HLRegionChecker.ViewModels
{
	public class IdentifierSelectPageViewModel : ViewModelBase
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

        /// <summary>
        /// ダイアログの処理を行うダイアログサービス
        /// </summary>
        private IPageDialogService dialogService;
        #endregion

        #region プロパティ
        /// <summary>
        /// ListViewのItemを選択した際のコマンド
        /// </summary>
        public Command<MemberModel> ItemSelectedCommand { get; }

        /// <summary>
        /// 識別子リスト
        /// </summary>
        public ObservableCollection<MemberModel> IdentifierListViewItems { get; set; }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// デフォルトのコンストラクタ
        /// </summary>
        /// <param name="navigationService"></param>
        /// <param name="pageDialogService"></param>
        public IdentifierSelectPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService) : base(navigationService)
        {
            this.navigationService = navigationService;
            this.dialogService = pageDialogService;
            IdentifierListViewItems = DbModel.Instance.Members;

            //識別子が選択された際の処理
            ItemSelectedCommand = new Command<MemberModel>(IdentifierSelected);
        }
        #endregion

        /// <summary>
        /// 識別子が選択された際に遷移元に戻ります
        /// </summary>
        /// <param name="item">選択されたListViewItem</param>
        public async void IdentifierSelected(MemberModel item)
        {
            UserDataModel.Instance.MemberId = item.Id;
            await dialogService.DisplayAlertAsync("設定完了", $"ユーザ識別子を「{item.Name}」に設定しました。", "OK");
            await navigationService.GoBackAsync();
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
