using QT.Common.Contracts;

namespace QT.Asset.Entity;

using SqlSugar;

/// <summary>
/// 盘点任务实体
/// </summary>
[SugarTable("asset_inventory_tasks")]
public class AssetInventoryTaskEntity : CLDEntityBase
{
    /// <summary>
    /// 盘点任务名称
    /// </summary>
    [SugarColumn(ColumnName = "task_name", Length = 100, IsNullable = false)]
    public string TaskName { get; set; }


    /// <summary>
    /// 盘点时间
    /// </summary>
    [SugarColumn(ColumnName = "inventory_time")]
    public DateTime? InventoryTime { get; set; }
}
