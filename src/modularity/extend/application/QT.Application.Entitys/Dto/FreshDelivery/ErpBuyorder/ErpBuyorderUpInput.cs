namespace QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;

/// <summary>
/// 采购任务订单更新输入.
/// </summary>
public class ErpBuyorderUpInput : ErpBuyorderCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }


    /// <summary>
    /// 采购订单明细.
    /// </summary>
    public new List<ErpBuyorderdetailUpInput> erpBuyorderdetailList { get; set; }
}

/// <summary>
/// 更新采购单价和金额
/// </summary>
public class ErpBuyorderdetailUpAmountInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 采购单价
    /// </summary>
    public decimal price { get; set; }


    /// <summary>
    /// 采购金额
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 是否赠品
    /// </summary>
    public int isFree { get; set; }
}