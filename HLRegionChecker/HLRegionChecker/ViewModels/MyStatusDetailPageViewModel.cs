using HLRegionChecker.Models;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace HLRegionChecker.ViewModels
{
	public class MyStatusDetailPageViewModel : BindableBase
	{
        /// <summary>
        /// プロパティの監視管理
        /// </summary>
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();

        /// <summary>
        /// データベースモデル
        /// </summary>
        public DbModel Db { get; }

        /// <summary>
        /// ユーザのステータステキスト
        /// </summary>
        public ReactiveProperty<string> Status { get; private set; }

        /// <summary>
        /// デフォルトのコンストラクタ
        /// </summary>
        public MyStatusDetailPageViewModel()
        {
            Db = DbModel.Instance;
            Status = Db.ObserveProperty(x => x.Members).Select(m => Db.GetYourStatusText() ?? "Offline").ToReactiveProperty().AddTo(Disposable);
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
