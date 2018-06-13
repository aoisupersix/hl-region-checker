using HLRegionChecker.Models;
using HLRegionChecker.Const;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Xamarin.Forms;

namespace HLRegionChecker.ViewModels
{
    public class MyStatusDetailPageViewModel : ViewModelBase
	{
        #region フィールド
        /// <summary>
        /// MainMasterPageのVM
        /// </summary>
        private MainMasterPageViewModel _mainViewModel;
        #endregion

        #region プロパティ
        /// <summary>
        /// プロパティの監視管理
        /// </summary>
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();

        /// <summary>
        /// データベースモデル
        /// </summary>
        public DbModel Db { get; }

        /// <summary>
        /// ユーザの識別子名(名前)
        /// </summary>
        public ReactiveProperty<string> IdentifierName { get; private set; }

        /// <summary>
        /// ユーザのステータステキスト
        /// </summary>
        public ReactiveProperty<string> Status { get; private set; }

        /// <summary>
        /// ビーコンのUUID
        /// </summary>
        public string Beacon_UUID { get; } = "長いから省略";

        /// <summary>
        /// ビーコンのメジャー値
        /// </summary>
        public int Beacon_Major { get; } = RegionConst.BEACON_MAJOR;

        /// <summary>
        /// ビーコンのマイナー値
        /// </summary>
        public int Beacon_Minor { get; } = RegionConst.BEACON_MINOR;

        /// <summary>
        /// ビーコンのTxPower
        /// </summary>
        public string Beacon_TxPower { get; } = "-59";

        /// <summary>
        /// アプリアイコン
        /// </summary>
        public ImageSource AppIcon { get; } = ImageSource.FromResource("HLRegionChecker.Resources.Icon_AppIcon.png");

        /// <summary>
        /// アプリのバージョン情報
        /// </summary>
        public string Version { get; } = "Version";

        /// <summary>
        /// アプリのバージョンコード情報
        /// </summary>
        public string VersionCode { get; } = "VersionCode";
        #endregion

        #region コンストラクタ
        /// <summary>
        /// デフォルトのコンストラクタ
        /// </summary>
        public MyStatusDetailPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            IdentifierName = UserDataModel.Instance.ObserveProperty(x => x.MemberId).Select(id =>
            {
                var mems = DbModel.Instance.Members;
                if (mems.Count <= id)
                    return "Offline";
                return mems.Where(m => m.Id == id).Select(m => m.Name).First();
            }).ToReactiveProperty().AddTo(Disposable);

            Status = DbModel.Instance.ObserveProperty(x => x.Members).Select(m => DbModel.Instance.GetYourStatusText() ?? "Offline").ToReactiveProperty().AddTo(Disposable);
        }
        #endregion

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {
            //Masterのジェスチャーを最有効化
            System.Diagnostics.Debug.WriteLine(parameters.ToString());
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
}
