using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 盘点记录主表实体.
/// </summary>
[SugarTable("iot_material_checkorder")]
public class MaterialCheckorderEntity : CUDEntityBase
{
    /// <summary>
    /// 盘点日期.
    /// </summary>
    [SugarColumn(ColumnName = "CheckTime")]
    public DateTime? CheckTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 盘点仓库ID.
    /// </summary>
    [SugarColumn(ColumnName = "StoreRomeId")]
    public string StoreRomeId { get; set; }

    /// <summary>
    /// 盘点单号.
    /// </summary>
    [SugarColumn(ColumnName = "No")]
    public string No { get; set; }

    /// <summary>
    /// 审核人id.
    /// </summary>
    [SugarColumn(ColumnName = "AuditUserId")]
    public string AuditUserId { get; set; }

    /// <summary>
    /// 审核时间.
    /// </summary>
    [SugarColumn(ColumnName = "AuditTime")]
    public DateTime? AuditTime { get; set; }   
}