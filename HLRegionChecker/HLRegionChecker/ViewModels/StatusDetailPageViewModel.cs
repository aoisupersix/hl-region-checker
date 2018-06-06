﻿using Prism.Commands;
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

        public StatusDetailPageViewModel(INavigationService navigationService) : base (navigationService)
        {
            Db = DbModel.Instance;
            Members = Db.ObserveProperty(x => x.Members).ToReactiveProperty().AddTo(Disposable);
            Status = Db.ObserveProperty(x => x.Members).Select(m => Db.GetYourStatusText() ?? "Offline").ToReactiveProperty().AddTo(Disposable);
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
