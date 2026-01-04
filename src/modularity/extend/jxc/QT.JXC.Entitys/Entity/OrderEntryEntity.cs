using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JXC.Entitys.Entity;

/// <summary>
/// 订单明细
/// </summary>
[SugarTable("EXT_ORDERENTRY")]
[Tenant(ClaimConst.TENANTID)]
public class OrderEntryEntity : EntityBase<string>
{
    /// <summary>
    /// 订单主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_ORDERID")]
    public string? OrderId { get; set; }

    /// <summary>
    /// 商品Id.
    /// </summary>
    [SugarColumn(ColumnName = "F_GOODSID")]
    public string? GoodsId { get; set; }

    /// <summary>
    /// 商品编码.
    /// </summary>
    [SugarColumn(ColumnName = "F_GOODSCODE")]
    public string? GoodsCode { get; set; }

    /// <summary>
    /// 商品名称.
    /// </summary>
    [SugarColumn(ColumnName = "F_GOODSNAME")]
    public string? GoodsName { get; set; }

    /// <summary>
    /// 规格型号.
    /// </summary>
    [SugarColumn(ColumnName = "F_SPECIFICATIONS")]
    public string? Specifications { get; set; }

    /// <summary>
    /// 单位.
    /// </summary>
    [SugarColumn(ColumnName = "F_UNIT")]
    public string? Unit { get; set; }

    /// <summary>
    /// 数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_QTY")]
    public decimal? Qty { get; set; }

    /// <summary>
    /// 单价.
    /// </summary>
    [SugarColumn(ColumnName = "F_PRICE")]
    public decimal? Price { get; set; }

    /// <summary>
    /// 金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_AMOUNT")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// 折扣%.
    /// </summary>
    [SugarColumn(ColumnName = "F_DISCOUNT")]
    public decimal? Discount { get; set; }

    /// <summary>
    /// 税率%.
    /// </summary>
    [SugarColumn(ColumnName = "F_CESS")]
    public decimal? Cess { get; set; }

    /// <summary>
    /// 实际单价.
    /// </summary>
    [SugarColumn(ColumnName = "F_ACTUALPRICE")]
    public decimal? ActualPrice { get; set; }

    /// <summary>
    /// 实际金额.
    /// </summary>
    [SugarColumn(ColumnName = "F_ACTUALAMOUNT")]
    public decimal? ActualAmount { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    [SugarColumn(ColumnName = "F_DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// 排序码.
    /// </summary>
    [SugarColumn(ColumnName = "F_SORTCODE")]
    public long? SortCode { get; set; }
}
