using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.Logistics.Entitys;

/// <summary>
/// 订单物品明细实体.
/// </summary>
[SugarTable("log_order_detail")]
[Tenant(ClaimConst.TENANTID)]
public class LogOrderDetailEntity : CUDEntityBase,IDeleteTime
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
    /// 物品名称.
    /// </summary>
    [SugarColumn(ColumnName = "Name")]
    public string Name { get; set; }

    /// <summary>
    /// 计量单位.
    /// </summary>
    [SugarColumn(ColumnName = "Unit")]
    public string Unit { get; set; }

    /// <summary>
    /// 数量.
    /// </summary>
    [SugarColumn(ColumnName = "Num")]
    public decimal Num { get; set; }

    /// <summary>
    /// 运费.
    /// </summary>
    [SugarColumn(ColumnName = "Freight")]
    public decimal Freight { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }
}