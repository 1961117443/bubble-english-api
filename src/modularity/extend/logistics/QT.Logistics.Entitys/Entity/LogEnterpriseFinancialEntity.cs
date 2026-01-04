using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 缴费记录实体.
/// </summary>
[SugarTable("log_enterprise_financial")]
[Tenant(ClaimConst.TENANTID)]
public class LogEnterpriseFinancialEntity : CUDEntityBase
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 商家id.
    /// </summary>
    [SugarColumn(ColumnName = "EId")]
    public string EId { get; set; }

    /// <summary>
    /// 商铺编号.
    /// </summary>
    [SugarColumn(ColumnName = "StoreNumber")]
    public string StoreNumber { get; set; }

    /// <summary>
    /// 缴费金额.
    /// </summary>
    [SugarColumn(ColumnName = "Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 缴费方式.
    /// </summary>
    [SugarColumn(ColumnName = "PaymentMethod")]
    public string PaymentMethod { get; set; }

    /// <summary>
    /// 缴费流水号.
    /// </summary>
    [SugarColumn(ColumnName = "No")]
    public string No { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }

}