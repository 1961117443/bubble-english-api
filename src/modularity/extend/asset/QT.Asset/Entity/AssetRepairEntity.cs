using QT.Common.Contracts;

namespace QT.Asset.Entity;

using SqlSugar;
using System;

/// <summary>
/// 资产维修记录实体
/// </summary>
[SugarTable("asset_repairs")]
public class AssetRepairEntity : CLDEntityBase
{
    /// <summary>
    /// 资产ID
    /// </summary>
    [SugarColumn(ColumnName = "asset_id", Length = 50, IsNullable = false)]
    public string AssetId { get; set; }

    /// <summary>
    /// 报修人ID
    /// </summary>
    [SugarColumn(ColumnName = "report_user_id", Length = 50, IsNullable = true)]
    public string ReportUserId { get; set; }

    /// <summary>
    /// 故障描述
    /// </summary>
    [SugarColumn(ColumnName = "fault_desc", IsNullable = true)]
    public string FaultDesc { get; set; }

    /// <summary>
    /// 报修时间
    /// </summary>
    [SugarColumn(ColumnName = "report_time", IsNullable = true)]
    public DateTime? ReportTime { get; set; }

    /// <summary>
    /// 维修人ID
    /// </summary>
    [SugarColumn(ColumnName = "repair_user_id", Length = 50, IsNullable = true)]
    public string RepairUserId { get; set; }

    /// <summary>
    /// 维修时间
    /// </summary>
    [SugarColumn(ColumnName = "repair_time", IsNullable = true)]
    public DateTime? RepairTime { get; set; }

    /// <summary>
    /// 维修费用
    /// </summary>
    [SugarColumn(ColumnName = "repair_cost", IsNullable = true)]
    public decimal? RepairCost { get; set; }

    /// <summary>
    /// 状态：维修中/已完成
    /// </summary>
    [SugarColumn(ColumnName = "status", Length = 20, IsNullable = true)]
    public string Status { get; set; } = "维修中";
}
