using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 商家商品规格实体.
/// </summary>
[SugarTable("log_enterprise_productmodel")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseProductmodelEntity :CUDEntityBase, ILogEnterpriseEntity
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
    /// 商品ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Pid")]
    public string Pid { get; set; }

    /// <summary>
    /// 规格.
    /// </summary>
    [SugarColumn(ColumnName = "F_Name")]
    public string Name { get; set; }

    /// <summary>
    /// 单位.
    /// </summary>
    [SugarColumn(ColumnName = "F_Unit")]
    public string Unit { get; set; }

    /// <summary>
    /// 成本价.
    /// </summary>
    [SugarColumn(ColumnName = "F_CostPrice")]
    public decimal CostPrice { get; set; }

    /// <summary>
    /// 销售价.
    /// </summary>
    [SugarColumn(ColumnName = "F_SalePrice")]
    public decimal SalePrice { get; set; }

    /// <summary>
    /// 条形码.
    /// </summary>
    [SugarColumn(ColumnName = "F_BarCode")]
    public string BarCode { get; set; }

    /// <summary>
    /// 起售数.
    /// </summary>
    [SugarColumn(ColumnName = "F_MinNum")]
    public decimal MinNum { get; set; }

    /// <summary>
    /// 限购数.
    /// </summary>
    [SugarColumn(ColumnName = "F_MaxNum")]
    public decimal MaxNum { get; set; }

    /// <summary>
    /// 库存数.
    /// </summary>
    [SugarColumn(ColumnName = "F_Num")]
    public decimal Num { get; set; }
}