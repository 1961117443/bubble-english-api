using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 商家商品入库明细实体.
/// </summary>
[SugarTable("log_enterprise_inrecord")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseInrecordEntity : CUDEntityBase, ILogEnterpriseEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 商家ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_EId")]
    public string EId { get; set; }

    /// <summary>
    /// 入库订单ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_InId")]
    public string InId { get; set; }

    /// <summary>
    /// 商品规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Gid")]
    public string Gid { get; set; }

    /// <summary>
    /// 入库数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_InNum")]
    public decimal InNum { get; set; }

    /// <summary>
    /// 入库单价.
    /// </summary>
    [SugarColumn(ColumnName = "F_Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// 入库金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 仓库.
    /// </summary>
    [SugarColumn(ColumnName = "F_StoreRomeId")]
    public string StoreRomeId { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "F_Remark")]
    public string Remark { get; set; }
}