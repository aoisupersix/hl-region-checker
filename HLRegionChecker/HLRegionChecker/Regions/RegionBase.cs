﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HLRegionChecker.Regions
{
    /// <summary>
    /// 仮想領域のベースクラス
    /// 今はわざわざ定義する必要ないけど今後拡張することを考えてベースクラスを定義しておく。
    /// </summary>
    public class RegionBase
    {
        /// <summary>
        /// 識別子
        /// </summary>
        public string Identifier { get; set; }

        public RegionBase(string identifier)
        {
            Identifier = identifier;
        }
    }
}