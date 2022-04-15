using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpimRevitAddIn.UI.Base
{
    /// <summary>
    /// 导出配置
    /// </summary>
    interface IExportUserConfig
    {
        /// <summary>
        /// 导出的几何细节等级
        /// </summary>
        int LevelOfDetail { get; }
    }
}
