using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 商家商品出库明细实体.
/// </summary>
[SugarTable("log_enterprise_outrecord")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseOutrecordEntity: CUDEntityBase, ILogEnterpriseEntity
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
    /// 出库订单ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_OutId")]
    public string OutId { get; set; }

    /// <summary>
    /// 商品规格ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Gid")]
    public string Gid { get; set; }

    /// <summary>
    /// 数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_Num")]
    public decimal Num { get; set; }

    /// <summary>
    /// 单价.
    /// </summary>
    [SugarColumn(ColumnName = "F_Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// 总价.
    /// </summary>
    [SugarColumn(ColumnName = "F_Amount")]
    public decimal Amount { get; set; }

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