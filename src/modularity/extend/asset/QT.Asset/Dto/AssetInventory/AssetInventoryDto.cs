using QT.Asset.Dto.Asset;
using QT.Asset.Entity;
using QT.Common.Filter;
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace QT.Asset.Dto.AssetInventory
{
    /// <summary>
    /// 盘点任务创建输入模型
    /// </summary>
    [SuppressSniffer]
    public class AssetInventoryTaskCrInput
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        [Required(ErrorMessage = "任务名称不能为空")]
        [StringLength(100, ErrorMessage = "任务名称长度不能超过100")]
        public string taskName { get; set; }

        ///// <summary>
        ///// 仓库ID列表
        ///// </summary>
        //public List<string> warehouseIds { get; set; } = new List<string>();

        ///// <summary>
        ///// 部门ID列表
        ///// </summary>
        //public List<string> departmentIds { get; set; } = new List<string>();

        ///// <summary>
        ///// 资产分类ID列表
        ///// </summary>
        //public List<string> categoryIds { get; set; } = new List<string>();

        /// <summary>
        /// 盘点时间
        /// </summary>
        public DateTime? inventoryTime { get; set; }


        /// <summary>
        /// 明细
        /// </summary>
        public List<AssetInventoryRecordDto> assetInventoryRecordEntitys { get; set; }
    }

    /// <summary>
    /// 盘点任务更新输入模型
    /// </summary>
    [SuppressSniffer]
    public class AssetInventoryTaskUpInput : AssetInventoryTaskCrInput
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        [Required(ErrorMessage = "任务ID不能为空")]
        public string id { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public string status { get; set; }
    }

    [SuppressSniffer]
    public class AssetInventoryTaskInfoOutput : AssetInventoryTaskCrInput
    {
        /// <summary>
        /// id.
        /// </summary>
        public string? id { get; set; }
    }

    /*
    /// <summary>
    /// 盘点任务输出模型
    /// </summary>
    [SuppressSniffer]
    public class AssetInventoryTaskDto
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string taskName { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? creatorTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string creatorUserName { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? completeTime { get; set; }

        /// <summary>
        /// 盘点进度
        /// </summary>
        public string progress { get; set; }

        /// <summary>
        /// 盘点记录数
        /// </summary>
        public int totalCount { get; set; }

        /// <summary>
        /// 正常数量
        /// </summary>
        public int normalCount { get; set; }

        /// <summary>
        /// 异常数量
        /// </summary>
        public int abnormalCount { get; set; }

        /// <summary>
        /// 缺失数量
        /// </summary>
        public int missingCount { get; set; }

        /// <summary>
        /// 盘点记录
        /// </summary>
        public List<AssetInventoryRecordDto> records { get; set; } = new List<AssetInventoryRecordDto>();
    }*/

    /// <summary>
    /// 盘点记录输入模型
    /// </summary>
    [SuppressSniffer]
    public class AssetInventoryRecordInput
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        [Required(ErrorMessage = "任务ID不能为空")]
        public string taskId { get; set; }

        /// <summary>
        /// 资产ID
        /// </summary>
        [Required(ErrorMessage = "资产ID不能为空")]
        public string assetId { get; set; }

        /// <summary>
        /// 状态：正常/缺失/异常
        /// </summary>
        [Required(ErrorMessage = "状态不能为空")]
        public string status { get; set; }

        /// <summary>
        /// 异常备注
        /// </summary>
        public string remark { get; set; }
    }

    /// <summary>
    /// 盘点记录输出模型
    /// </summary>
    [SuppressSniffer]
    public class AssetInventoryRecordDto
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public string taskId { get; set; }

        /// <summary>
        /// 资产ID
        /// </summary>
        public string assetId { get; set; }

        /// <summary>
        /// 资产编号
        /// </summary>
        public string assetCode { get; set; }

        /// <summary>
        /// 资产名称
        /// </summary>
        public string assetName { get; set; }

        /// <summary>
        /// 资产分类
        /// </summary>
        public string categoryName { get; set; }

        /// <summary>
        /// 责任人
        /// </summary>
        public string dutyUserName { get; set; }

        /// <summary>
        /// 存放位置
        /// </summary>
        public string location { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        public string warehouseName { get; set; }

        /// <summary>
        /// 状态：正常/缺失/异常
        /// </summary>
        public AssetStatus? status { get; set; }

        ///// <summary>
        ///// 异常备注
        ///// </summary>
        //public string remark { get; set; }

        ///// <summary>
        ///// 扫码时间
        ///// </summary>
        //public DateTime? scanTime { get; set; }

        ///// <summary>
        ///// 操作人
        ///// </summary>
        //public string operatorName { get; set; }


        /// <summary>
        /// 条码
        /// </summary>
        public string barcode { get; set; }

        /// <summary>
        /// 盘点时间
        /// </summary>
        public DateTime? inventoryTime { get; set; }
    }

    /// <summary>
    /// 盘点任务分页查询输入
    /// </summary>
    [SuppressSniffer]
    public class AssetInventoryTaskListPageInput : PageInputBase
    {

        /// <summary>
        /// 资产id
        /// </summary>
        public string assetId { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string taskName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? startTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? endTime { get; set; }
    }

    /// <summary>
    /// 盘点任务分页输出
    /// </summary>
    [SuppressSniffer]
    public class AssetInventoryTaskListOutput
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string taskName { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public string status { get; set; }

        ///// <summary>
        ///// 创建时间
        ///// </summary>
        //public DateTime? creatorTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string creatorUserName { get; set; }

        ///// <summary>
        ///// 完成时间
        ///// </summary>
        //public DateTime? completeTime { get; set; }

        ///// <summary>
        ///// 盘点进度
        ///// </summary>
        //public string progress { get; set; }


        /// <summary>
        /// 盘点时间
        /// </summary>
        public DateTime? inventoryTime { get; set; }
    }
}