using QT.Common.Contracts;

namespace QT.Asset.Entity;

using SqlSugar;
using System;

/// <summary>
/// 资产变更日志实体
/// </summary>
[SugarTable("asset_change_logs")]
public class AssetChangeLogEntity:IDeleteTime
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 资产ID
    /// </summary>
    [SugarColumn(ColumnName = "asset_id", Length = 50, IsNullable = false)]
    public string AssetId { get; set; }

    /// <summary>
    /// 变更类型：Asset/Inventory/Transfer
    /// </summary>
    [SugarColumn(ColumnName = "change_from", Length = 50, IsNullable = false)]
    public string ChangeFrom { get; set; }

    /// <summary>
    /// 字段描述
    /// </summary>
    [SugarColumn(ColumnName = "field_title", Length = 100, IsNullable = false)]
    public string FieldTitle { get; set; }

    /// <summary>
    /// 字段名
    /// </summary>
    [SugarColumn(ColumnName = "field_name", Length = 100, IsNullable = false)]
    public string FieldName { get; set; }

    /// <summary>
    /// 旧值
    /// </summary>
    [SugarColumn(ColumnName = "old_value", IsNullable = true)]
    public string OldValue { get; set; }

    /// <summary>
    /// 新值
    /// </summary>
    [SugarColumn(ColumnName = "new_value", IsNullable = true)]
    public string NewValue { get; set; }

    /// <summary>
    /// 来源任务ID
    /// </summary>
    [SugarColumn(ColumnName = "task_id", IsNullable = true)]
    public string? TaskId { get; set; }

    /// <summary>
    /// 变更原因
    /// </summary>
    [SugarColumn(ColumnName = "change_reason", Length = 255, IsNullable = true)]
    public string ChangeReason { get; set; }

    /// <summary>
    /// 操作人ID
    /// </summary>
    [SugarColumn(ColumnName = "operator_id", Length = 50, IsNullable = true)]
    public string OperatorId { get; set; }

    /// <summary>
    /// 操作人姓名
    /// </summary>
    [SugarColumn(ColumnName = "operator_name", Length = 50, IsNullable = true)]
    public string OperatorName { get; set; }

    /// <summary>
    /// 变更时间
    /// </summary>
    [SugarColumn(ColumnName = "change_time", IsNullable = true)]
    public DateTime ChangeTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 记录删除时间
    /// </summary>
    [SugarColumn(ColumnName = "F_DeleteTime", IsNullable = true)]
    public DateTime? DeleteTime { get; set; }
}


public enum AssetChangeType
{
    /// <summary>
    /// 资产信息
    /// </summary>
    Asset,

    /// <summary>
    /// 资产盘点
    /// </summary>
    Inventory,

    /// <summary>
    /// 资产变更
    /// </summary>
    Transfer
}