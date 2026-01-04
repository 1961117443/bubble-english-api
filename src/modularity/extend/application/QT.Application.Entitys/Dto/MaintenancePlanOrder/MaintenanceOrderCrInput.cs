using QT.Common.Contracts;
using QT.Common.Models;

namespace QT.Iot.Application.Dto.MaintenancePlanOrder;

public class MaintenancePlanOrderCrInput:ISlaveCrInput<MaintenancePlanRecordListOutput>
{
    /// <summary>
    /// 计划单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 计划日期.
    /// </summary>
    public DateTime? inTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 维保人.
    /// </summary>
    public string taskUserId { get; set; }

    /// <summary>
    /// 入库明细
    /// </summary>
    public List<MaintenancePlanRecordListOutput> items { get; set; }

    /// <summary>
    /// 附件
    /// </summary>
    public List<FileControlsModel> attach { get; set; }

    /// <summary>
    /// 所属项目
    /// </summary>
    public string projectId { get; set; }
}
