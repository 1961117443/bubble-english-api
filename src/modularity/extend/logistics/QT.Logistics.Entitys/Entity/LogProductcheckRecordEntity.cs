using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 盘点记录实体.
/// </summary>
[SugarTable("log_productcheck_record")]
[Tenant(ClaimConst.TENANTID)]
public class LogProductcheckRecordEntity: CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 主表ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_CId")]
    public string CId { get; set; }

    /// <summary>
    /// 商家ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_EId")]
    public string EId { get; set; }

    /// <summary>
    /// 规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Gid")]
    public string Gid { get; set; }

    /// <summary>
    /// 系统数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_SystemNum")]
    public decimal SystemNum { get; set; }

    /// <summary>
    /// 实际数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_RealNum")]
    public decimal RealNum { get; set; }

    /// <summary>
    /// 拆损数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_LoseNum")]
    public decimal LoseNum { get; set; }

    /// <summary>
    /// 仓库ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_StoreRomeId")]
    public string StoreRomeId { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }

}