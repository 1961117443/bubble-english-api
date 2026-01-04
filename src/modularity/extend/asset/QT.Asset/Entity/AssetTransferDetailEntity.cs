using QT.Common.Contracts;

namespace QT.Asset.Entity;

using SqlSugar;

/// <summary>
/// 资产调拨明细实体
/// </summary>
[SugarTable("asset_transfer_details")]
public class AssetTransferDetailEntity : CLDEntityBase
{
    /// <summary>
    /// 调拨任务ID
    /// </summary>
    [SugarColumn(ColumnName = "task_id", Length = 50, IsNullable = false)]
    public string TaskId { get; set; }

    /// <summary>
    /// 资产ID
    /// </summary>
    [SugarColumn(ColumnName = "asset_id", Length = 50, IsNullable = false)]
    public string AssetId { get; set; }

    /// <summary>
    /// 原负责人ID
    /// </summary>
    [SugarColumn(ColumnName = "old_duty_user_id", Length = 50, IsNullable = true)]
    public string OldDutyUserId { get; set; }

    /// <summary>
    /// 原使用人ID
    /// </summary>
    [SugarColumn(ColumnName = "old_user_id", Length = 50, IsNullable = true)]
    public string OldUserId { get; set; }

    /// <summary>
    /// 原部门ID
    /// </summary>
    [SugarColumn(ColumnName = "old_department_id", Length = 50, IsNullable = true)]
    public string OldDepartmentId { get; set; }

    /// <summary>
    /// 原仓库ID
    /// </summary>
    [SugarColumn(ColumnName = "old_warehouse_id", Length = 50, IsNullable = true)]
    public string OldWarehouseId { get; set; }

    /// <summary>
    /// 原位置
    /// </summary>
    [SugarColumn(ColumnName = "old_location", Length = 100, IsNullable = true)]
    public string OldLocation { get; set; }

    /// <summary>
    /// 新负责人ID
    /// </summary>
    [SugarColumn(ColumnName = "new_duty_user_id", Length = 50, IsNullable = true)]
    public string NewDutyUserId { get; set; }

    /// <summary>
    /// 新使用人ID
    /// </summary>
    [SugarColumn(ColumnName = "new_user_id", Length = 50, IsNullable = true)]
    public string NewUserId { get; set; }

    /// <summary>
    /// 新部门ID
    /// </summary>
    [SugarColumn(ColumnName = "new_department_id", Length = 50, IsNullable = true)]
    public string NewDepartmentId { get; set; }

    /// <summary>
    /// 新仓库ID
    /// </summary>
    [SugarColumn(ColumnName = "new_warehouse_id", Length = 50, IsNullable = true)]
    public string NewWarehouseId { get; set; }

    /// <summary>
    /// 新位置
    /// </summary>
    [SugarColumn(ColumnName = "new_location", Length = 100, IsNullable = true)]
    public string NewLocation { get; set; }
}
