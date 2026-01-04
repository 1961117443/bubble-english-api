using QT.Common.Contracts;
using SqlSugar;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 维保记录明细明细实体.
/// </summary>
[SugarTable("iot_maintenance_record")]
public class MaintenanceRecordEntity : CUDSlaveEntityBase
{
    /// <summary>
    /// 物资ID.
    /// </summary>
    [SugarColumn(ColumnName = "Mid")]
    public string Mid { get; set; }

    /// <summary>
    /// 工时.
    /// </summary>
    [SugarColumn(ColumnName = "Num")]
    public decimal Num { get; set; }

    /// <summary>
    /// 人工合价.
    /// </summary>
    [SugarColumn(ColumnName = "ArtificialAmount")]
    public decimal ArtificialAmount { get; set; }

    /// <summary>
    /// 材料合价.
    /// </summary>
    [SugarColumn(ColumnName = "MaterialAmount")]
    public decimal MaterialAmount { get; set; }

    /// <summary>
    /// 小计金额.
    /// </summary>
    [SugarColumn(ColumnName = "Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 设备状态.
    /// </summary>
    [SugarColumn(ColumnName = "Status")]
    public string Status { get; set; }

    /// <summary>
    /// 维保内容.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }
}