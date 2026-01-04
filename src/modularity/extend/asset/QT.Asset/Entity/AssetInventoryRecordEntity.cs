using QT.Common.Contracts;

using SqlSugar;
using System;


namespace QT.Asset.Entity;
/// <summary>
/// 盘点记录实体
/// </summary>
[SugarTable("asset_inventory_records")]
public class AssetInventoryRecordEntity : CLDEntityBase
{
    /// <summary>
    /// 盘点任务ID
    /// </summary>
    [SugarColumn(ColumnName = "task_id", Length = 50, IsNullable = false)]
    public string TaskId { get; set; }

    /// <summary>
    /// 资产ID
    /// </summary>
    [SugarColumn(ColumnName = "asset_id", Length = 50, IsNullable = false)]
    public string AssetId { get; set; }

    /// <summary>
    /// 盘点状态：正常/缺失/异常
    /// </summary>
    [SugarColumn(ColumnName = "status", Length = 2, IsNullable = true)]
    public AssetStatus? Status { get; set; }

    /// <summary>
    /// 扫码时间
    /// </summary>
    [SugarColumn(ColumnName = "scan_time", IsNullable = true)]
    public DateTime? ScanTime { get; set; }
}
