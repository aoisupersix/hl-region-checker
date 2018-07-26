using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using HLRegionChecker.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Linq;
using System;
using Xamarin.Forms;

namespace HLRegionChecker.ViewModels
{
    public class StatusDetailPageViewModel : ViewModelBase
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
        /// メンバー情報
        /// </summary>
        public ReactiveProperty<ObservableCollection<MemberModel>> Members { get; }

        /// <summary>
        /// ユーザのステータステキスト
        /// </summary>
        public ReactiveProperty<string> Status { get; private set; }

        public ReactiveProperty<int> SelectedItemId { get; }

        public Command<MemberModel> ItemSelectedCommand { get; }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// デフォルトのコンストラクタ
        /// </summary>
        /// <param name="navigationService"></param>
        public StatusDetailPageViewModel(INavigationService navigationService) : base (navigationService)
        {
            Db = DbModel.Instance;
            Members = Db.ObserveProperty(x => x.Members).ToReactiveProperty().AddTo(Disposable);
            Status = Db.ObserveProperty(x => x.Members).Select(m => Db.GetYourStatusText() ?? "Offline").ToReactiveProperty().AddTo(Disposable);

            SelectedItemId = new ReactiveProperty<int>();
            ItemSelectedCommand = new Command<MemberModel>(
                x => SelectedItemId.Value = x.Id,
                x => x != null
                );

            //初期でIdが0になってしまうので-1にしておく
            SelectedItemId.Value = -1;

            SelectedItemId.Subscribe(value =>
            {
                Console.WriteLine(value);
            });
        }
        #endregion

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
