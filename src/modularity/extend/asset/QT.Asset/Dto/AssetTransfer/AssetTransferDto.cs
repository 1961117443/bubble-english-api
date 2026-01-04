using QT.Common.Filter;
using QT.DependencyInjection;
using System.ComponentModel.DataAnnotations;
namespace QT.Asset.Dto.AssetTransfer
{
    /// <summary>
    /// 调拨任务创建输入模型
    /// </summary>
    [SuppressSniffer]
    public class AssetTransferTaskCrInput
    {
        /// <summary>
        /// 变更时间
        /// </summary>
        [Required(ErrorMessage = "变更时间不能为空")]
        public DateTime transferTime { get; set; }

        /// <summary>
        /// 变更原因
        /// </summary>
        [StringLength(255, ErrorMessage = "变更原因长度不能超过255")]
        public string reason { get; set; }

        /// <summary>
        /// 调拨明细列表
        /// </summary>
        //[Required(ErrorMessage = "调拨明细不能为空")]
        //[MinLength(1, ErrorMessage = "至少需要一条调拨明细")]
        public List<AssetTransferDetailDto> assetTransferDetailEntitys { get; set; }
    }
    /// <summary>
    /// 调拨任务更新输入模型
    /// </summary>
    [SuppressSniffer]
    public class AssetTransferTaskUpInput : AssetTransferTaskCrInput
    {
        /// <summary>
        /// 调拨任务ID
        /// </summary>
        [Required(ErrorMessage = "任务ID不能为空")]
        public string id { get; set; }
    }

    /// <summary>
    /// 调拨任务输出模型
    /// </summary>
    [SuppressSniffer]
    public class AssetTransferTaskDto : AssetTransferTaskCrInput
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public string id { get; set; }
    }

   

    /// <summary>
    /// 调拨任务分页查询输入
    /// </summary>
    [SuppressSniffer]
    public class AssetTransferTaskListPageInput : PageInputBase
    {

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? startTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? endTime { get; set; }

        /// <summary>
        /// 资产id
        /// </summary>
        public string assetId { get; set; }
    }

    /// <summary>
    /// 调拨任务分页输出
    /// </summary>
    [SuppressSniffer]
    public class AssetTransferTaskListOutput
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime transferTime { get; set; }

        /// <summary>
        /// 变更原因
        /// </summary>
        public string reason { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? creatorTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string creatorUserName { get; set; }

        /// <summary>
        /// 资产数量
        /// </summary>
        public int assetCount { get; set; }
    }

    /// <summary>
    /// 调拨明细输出模型
    /// </summary>
    [SuppressSniffer]
    public class AssetTransferDetailDto: AssetTransferDetailCrInput
    {
        /// <summary>
        /// 明细ID
        /// </summary>
        public string id { get; set; }

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
        /// 资产条码
        /// </summary>
        public string barcode { get; set; }

        /// <summary>
        /// 原责任人ID
        /// </summary>
        public string oldDutyUserId { get; set; }

        /// <summary>
        /// 原责任人
        /// </summary>
        public string oldDutyUserName { get; set; }

        /// <summary>
        /// 原使用人ID
        /// </summary>
        public string oldUserId { get; set; }

        /// <summary>
        /// 原使用人
        /// </summary>
        public string oldUserName { get; set; }

        /// <summary>
        /// 原部门ID
        /// </summary>
        public string oldDepartmentId { get; set; }

        /// <summary>
        /// 原部门
        /// </summary>
        public string oldDepartmentName { get; set; }

        /// <summary>
        /// 原仓库ID
        /// </summary>
        public string oldWarehouseId { get; set; }

        /// <summary>
        /// 原仓库
        /// </summary>
        public string oldWarehouseName { get; set; }

        /// <summary>
        /// 原位置
        /// </summary>
        public string oldLocation { get; set; }

        /// <summary>
        /// 新责任人ID
        /// </summary>
        public string newDutyUserId { get; set; }

        /// <summary>
        /// 新责任人
        /// </summary>
        public string newDutyUserName { get; set; }

        /// <summary>
        /// 新使用人ID
        /// </summary>
        public string newUserId { get; set; }

        /// <summary>
        /// 新使用人
        /// </summary>
        public string newUserName { get; set; }

        /// <summary>
        /// 新部门ID
        /// </summary>
        public string newDepartmentId { get; set; }

        /// <summary>
        /// 新部门
        /// </summary>
        public string newDepartmentName { get; set; }

        /// <summary>
        /// 新仓库ID
        /// </summary>
        public string newWarehouseId { get; set; }

        /// <summary>
        /// 新仓库
        /// </summary>
        public string newWarehouseName { get; set; }

        /// <summary>
        /// 新位置
        /// </summary>
        public string newLocation { get; set; }

        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime? transferTime { get; set; }
    }


    /// <summary>
    /// 调拨明细创建输入模型
    /// </summary>
    [SuppressSniffer]
    public class AssetTransferDetailCrInput
    {
        /// <summary>
        /// 资产ID
        /// </summary>
        [Required(ErrorMessage = "资产ID不能为空")]
        public virtual string assetId { get; set; }

        /// <summary>
        /// 新责任人ID
        /// </summary>
        public virtual string newDutyUserId { get; set; }

        /// <summary>
        /// 新使用人ID
        /// </summary>
        public virtual string newUserId { get; set; }

        /// <summary>
        /// 新部门ID
        /// </summary>
        public virtual string newDepartmentId { get; set; }

        /// <summary>
        /// 新仓库ID
        /// </summary>
        public virtual string newWarehouseId { get; set; }

        /// <summary>
        /// 新位置
        /// </summary>
        [StringLength(100, ErrorMessage = "位置长度不能超过100")]
        public virtual string newLocation { get; set; }
    }
}
