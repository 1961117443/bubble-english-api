using QT.Common;
using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 维保记录
/// </summary>
[SugarTable("iot_maintenance_order")]
public class MaintenanceOrderEntity : CUDEntityBase
{
    /// <summary>
    /// 维保单号.
    /// </summary>
    [SugarColumn(ColumnName = "No")]
    public string No { get; set; }


    /// <summary>
    /// 日期.
    /// </summary>
    [SugarColumn(ColumnName = "InTime")]
    public DateTime? InTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }


    /// <summary>
    /// 附件.
    /// </summary>
    [SugarColumn(ColumnName = "AttachJson")]
    public string AttachJson { get; set; }

    /// <summary>
    /// 所属项目.
    /// </summary>
    [SugarColumn(ColumnName = "ProjectId")]
    public string ProjectId { get; set; }
}
