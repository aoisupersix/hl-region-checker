using HLRegionChecker.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Xamarin.Forms;

namespace HLRegionChecker.ViewModels
{
	public class MyStatusDetailPageViewModel : ViewModelBase
	{
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
        public string Beacon_UUID { get; } = "UUID";

        /// <summary>
        /// ビーコンのメジャー値
        /// </summary>
        public string Beacon_Major { get; } = "Major";

        /// <summary>
        /// ビーコンのマイナー値
        /// </summary>
        public string Beacon_Minor { get; } = "Minor";

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
