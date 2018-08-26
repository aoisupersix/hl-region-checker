using System;
using System.Collections.Generic;
using System.Text;

namespace HLRegionChecker.Interfaces
{
    /// <summary>
    /// WebBrowser関係のDependencyServiceInterface
    /// </summary>
    public interface IWebBrowserService
    {
        /// <summary>
        /// 引数に与えられたUriをブラウザで開きます。
        /// </summary>
        /// <param name="uri"></param>
        void Open(Uri uri);
    }
}
