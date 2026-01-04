using QT.Common.Contracts;
using QT.Common.Models;

namespace QT.Iot.Application.Dto.MaintenanceOrder;

public class MaintenanceOrderCrInput:ISlaveCrInput<MaintenanceRecordListOutput>
{
    /// <summary>
    /// 维保单号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 日期.
    /// </summary>
    public DateTime? inTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 入库明细
    /// </summary>
    public List<MaintenanceRecordListOutput> items { get; set; }

    /// <summary>
    /// 附件
    /// </summary>
    public List<FileControlsModel> attach { get; set; }

    /// <summary>
    /// 所属项目
    /// </summary>
    public string projectId { get; set; }
}
