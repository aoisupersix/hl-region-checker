using HLRegionChecker.Models;
using Prism.Navigation;
using Prism.Services;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HLRegionChecker.ViewModels
{
    public class StatusDetailPageViewModel : ViewModelBase
    {
        #region フィールド

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
        /// 表示するメンバー情報
        /// </summary>
        public ReactiveProperty<MemberModel> SelectedMemberModel { get; private set; }

        #endregion

        public StatusDetailPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService) : base(navigationService)
        {
            this.navigationService = navigationService;
            this.dialogService = pageDialogService;
            SelectedMemberModel = new ReactiveProperty<MemberModel>();
        }

        public override void OnNavigatingTo(NavigationParameters parameters)
        {
            //遷移元MemberIdを取得
            var keyName = StatusListPageViewModel.NAVIGATION_PARAM_MEMBER_ID;
            if (parameters.Any(x => x.Key.Equals(keyName)))
            {
                var memberId = (int)parameters[keyName];
                SelectedMemberModel.Value = DbModel.Instance.Members.Where(x => x.Id == memberId).First();
            }
        }
    }
}
