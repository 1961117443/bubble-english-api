namespace QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;

/// <summary>
/// 采购订单明细更新输入.
/// </summary>
public class ErpBuyorderdetailUpInput : ErpBuyorderdetailCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }


    public override decimal? price { get; set; }

    public override decimal? amount { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    public override DateTime? productionDate { get; set; }

    /// <summary>
    /// 保质期
    /// </summary>
    public override string retention { get; set; }
}