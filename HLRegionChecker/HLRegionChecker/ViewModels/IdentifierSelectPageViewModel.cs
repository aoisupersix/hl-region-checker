using HLRegionChecker.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HLRegionChecker.ViewModels
{
	public class IdentifierSelectPageViewModel : BindableBase
	{
        /// <summary>
        /// 識別子リスト
        /// </summary>
        public ObservableCollection<MemberModel> IdentifierListViewItems { get; set; }

        public IdentifierSelectPageViewModel()
        {
            IdentifierListViewItems = DbModel.Instance.Members;
        }
	}
}
