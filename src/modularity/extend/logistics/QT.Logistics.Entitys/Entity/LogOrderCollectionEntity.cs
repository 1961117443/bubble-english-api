using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 订单收款明细实体.
/// </summary>
[SugarTable("log_order_collection")]
[Tenant(ClaimConst.TENANTID)]
public class LogOrderCollectionEntity : CUDEntityBase,IDeleteTime
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public override string Id { get; set; }

    /// <summary>
    /// 订单id.
    /// </summary>
    [SugarColumn(ColumnName = "OrderId")]
    public string OrderId { get; set; }

    /// <summary>
    /// 收款金额.
    /// </summary>
    [SugarColumn(ColumnName = "Amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 收款方.
    /// </summary>
    [SugarColumn(ColumnName = "Payee")]
    public string Payee { get; set; }
}