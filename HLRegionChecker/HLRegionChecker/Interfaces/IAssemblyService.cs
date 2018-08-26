using System;
using System.Collections.Generic;
using System.Text;

namespace HLRegionChecker.Interfaces
{
    public interface IAssemblyService
    {
        /// <summary>
        /// パッケージ名を取得します。
        /// </summary>
        /// <returns></returns>
        string GetPackageName();

        /// <summary>
        /// バージョン名を取得します。
        /// </summary>
        /// <returns></returns>
        string GetVersionName();

        /// <summary>
        /// バージョンコードを取得します。
        /// </summary>
        /// <returns></returns>
        int GetVersionCode();
    }
}
