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
            set { SetProperty(ref _members, value); }
        }

        /// <summary>
        /// ステータス情報
        /// </summary>
        public ObservableCollection<StateModel> States
        {
            get { return _states; }
            set { SetProperty(ref _states, value); }
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
                members.ForEach(x => Console.WriteLine("member-Id:{0},Name:{1},Status:{2}", x.Id, x.Name, x.Status));
                states.ForEach(x => Console.WriteLine("state-Id:{0},Name:{1},Color:{2},Hex-BgColor:{3},Hex-TextColor:{4}", x.Id, x.Name, x.Color, x.BgColor_Hex, x.TextColor_Hex));

                Members = new ObservableCollection<MemberModel>(members);
                States = new ObservableCollection<StateModel>(states);
            };
            _dbAdapter.UpdatedMembers += (_, members) =>
            {
                Members = new ObservableCollection<MemberModel>(members);
            };

            _dbAdapter.InitDb();
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
            if (Members.Count == 0 || States.Count == 0 || memId == null)
                return null;

            var state = DbModel.Instance.GetStatusNameForId(
                Members.Where(x => x.Id == memId)
                .Select(x => x.Status)
                .First()
            );
            return state.Value.Name;
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
