using QT.Common.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Asset.Dto.AssetCategory
{
    public class AssetCategorySelectorOutput: TreeModel
    {
        /// <summary>
        /// 分类名称.
        /// </summary>
        public string name { get; set; }
    }
}
