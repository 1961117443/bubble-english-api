using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 盘点记录主表实体.
/// </summary>
[SugarTable("log_productcheck_master")]
[Tenant(ClaimConst.TENANTID)]
public class LogProductcheckMasterEntity :CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 商家ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_EId")]
    public string EId { get; set; }

    /// <summary>
    /// 盘点日期.
    /// </summary>
    [SugarColumn(ColumnName = "F_CheckTime")]
    public DateTime? CheckTime { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 盘点仓库ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_StoreRomeId")]
    public string StoreRomeId { get; set; }

    /// <summary>
    /// 盘点单号.
    /// </summary>
    [SugarColumn(ColumnName = "F_No")]
    public string No { get; set; }

    /// <summary>
    /// 审核人id.
    /// </summary>
    [SugarColumn(ColumnName = "F_AuditUserId")]
    public string AuditUserId { get; set; }

    /// <summary>
    /// 审核时间.
    /// </summary>
    [SugarColumn(ColumnName = "F_AuditTime")]
    public DateTime? AuditTime { get; set; }   
}