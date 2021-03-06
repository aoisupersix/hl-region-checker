﻿using HLRegionChecker.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace HLRegionChecker.Models
{
    /// <summary>
    /// メンバー情報とステータス情報のモデルクラスです。
    /// 最初期に書いたコードで自分でも全然覚えてないので、いち早く修正したいです。
    /// </summary>
    public class DbModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// クラスのインスタンス
        /// </summary>
        private static DbModel _instance;

        /// <summary>
        /// データベースの個別処理用アダプタ
        /// </summary>
        private IDbAdapter _dbAdapter = DependencyService.Get<IDbAdapter>();

        private ObservableCollection<MemberModel> _members;

        private ObservableCollection<StateModel> _states;

        private string _memberDisplayName; 

        #endregion

        #region プロパティ
        /// <summary>
        /// クラスのインスタンスを取得します。
        /// </summary>
        public static DbModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DbModel();

                return _instance;
            }
        }

        /// <summary>
        /// メンバー情報
        /// </summary>
        public ObservableCollection<MemberModel> Members
        {
            get { return _members; }
            set
            {
                SetProperty(ref _members, value);
                UpdateMemberDisplayName();
            }
        }

        /// <summary>
        /// ステータス情報
        /// </summary>
        public ObservableCollection<StateModel> States
        {
            get { return _states; }
            set { SetProperty(ref _states, value); }
        }

        /// <summary>
        /// 自分の表示名
        /// </summary>
        public string MemberDisplayName
        {
            get
            {
                if (_memberDisplayName == null)
                    UpdateMemberDisplayName();
                return _memberDisplayName;
            }
            set { SetProperty(ref _memberDisplayName, value); }
        }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// デフォルトのコンストラクタ
        /// </summary>
        private DbModel()
        {
            Members = new ObservableCollection<MemberModel>();
            States = new ObservableCollection<StateModel>();

            InitDb();
        }
        #endregion

        private void InitDb()
        {
            //DBAdapterのイベント登録
            _dbAdapter.InitializedDb += (_, members, states) =>
            {
                //members.ForEach(x => Console.WriteLine("member-Id:{0},Name:{1},Status:{2}", x.Id, x.Name, x.Status));
                //states.ForEach(x => Console.WriteLine("state-Id:{0},Name:{1},Color:{2},Hex-BgColor:{3},Hex-TextColor:{4}", x.Id, x.Name, x.Color, x.BgColor_Hex, x.TextColor_Hex));

                //更新処理はメインスレッドで行う
                Device.BeginInvokeOnMainThread(() =>
                {
                    //Members = new ObservableCollection<MemberModel>(members);
                    States = new ObservableCollection<StateModel>(states);
                });
            };
            _dbAdapter.UpdatedMembers += (_, members) =>
            {
                //更新処理はメインスレッドで行う
                Device.BeginInvokeOnMainThread(() =>
                {
                    Members = new ObservableCollection<MemberModel>(members);
                });
            };

            _dbAdapter.InitDb();
        }

        /// <summary>
        /// メンバーの表示名を更新します。
        /// </summary>
        public void UpdateMemberDisplayName()
        {
            var id = UserDataModel.Instance.MemberId;
            if (id == UserDataModel.DefaultMemberId || Members == null || !Members.Any())
                MemberDisplayName = "ー";
            else
                MemberDisplayName = Members.Where(x => x.Id == id).Select(x => x.Name).First() ?? "ー";
        }

        /// <summary>
        /// 現在設定されているメンバーのステータスを更新します。
        /// </summary>
        /// <param name="stateId">更新するステータスID</param>
        public void UpdateState(int stateId)
        {
            var memId = UserDataModel.Instance.MemberId;
            if(memId != UserDataModel.DefaultMemberId)
                UpdateState(memId, stateId);
        }

        /// <summary>
        /// 引数に与えられたメンバーIDのステータスを更新します。
        /// </summary>
        /// <param name="memberId">更新するメンバーID</param>
        /// <param name="stateId">更新するステータスID</param>
        public void UpdateState(int memberId, int stateId)
        {
            _dbAdapter.UpdateStatus(memberId, stateId, false);
        }

        /// <summary>
        /// 引数に与えられたIDのステータスを取得します。
        /// </summary>
        /// <param name="stateId">取得するステータスID</param>
        /// <returns>ステータスID、見つからなければNull</returns>
        public StateModel? GetStatusNameForId(int stateId)
        {
            if (States == null)
                return null;
            return States.Where(x => x.Id == stateId).First();
        }

        /// <summary>
        /// 現在設定されているメンバーのステータステキストを取得します。
        /// </summary>
        /// <returns>ステータステキスト、存在しない場合はnull</returns>
        public String GetYourStatusText()
        {
            var memId = UserDataModel.Instance.MemberId;
            if (Members.Count == 0 || States.Count == 0 || memId == UserDataModel.DefaultMemberId)
                return null;

            var state = DbModel.Instance.GetStatusNameForId(
                Members.Where(x => x.Id == memId)
                .Select(x => x.Status)
                .First()
            );
            return state.Value.Name;
        }

        /// <summary>
        /// デバイス情報を更新します。
        /// </summary>
        public void UpdateDeviceInfo()
        {
            _dbAdapter.UpdateDeviceInfo();
        }

        /// <summary>
        /// デバイス情報を更新します。
        /// </summary>
        /// <param name="memberId">デバイスで指定されているメンバーID</param>
        public void UpdateDeviceInfo(int memberId)
        {
            _dbAdapter.UpdateDeviceInfo(memberId: memberId);
        }

        /// <summary>
        /// Dbのイベントハンドラを削除します。
        /// </summary>
        public void Detach()
        {
            _dbAdapter.Disappear();
        }
    }
}
