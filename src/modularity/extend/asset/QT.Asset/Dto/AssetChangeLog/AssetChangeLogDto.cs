using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Asset.Dto.AssetChangeLog
{
    public class AssetChangeLogDto
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// 资产ID
        /// </summary>
        public string assetId { get; set; }

        /// <summary>
        /// 变更类型：Asset/Inventory/Transfer
        /// </summary>
        public string changeFrom { get; set; }

        /// <summary>
        /// 字段描述
        /// </summary>
        public string fieldTitle { get; set; }

        /// <summary>
        /// 字段名
        /// </summary>
        public string fieldName { get; set; }

        /// <summary>
        /// 旧值
        /// </summary>
        public string oldValue { get; set; }

        /// <summary>
        /// 新值
        /// </summary>
        public string newValue { get; set; }

        /// <summary>
        /// 来源任务ID
        /// </summary>
        public string? taskId { get; set; }

        /// <summary>
        /// 变更原因
        /// </summary>
        public string changeReason { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        public string operatorId { get; set; }

        /// <summary>
        /// 操作人姓名
        /// </summary>
        public string operatorName { get; set; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime changeTime { get; set; }
    }
}
