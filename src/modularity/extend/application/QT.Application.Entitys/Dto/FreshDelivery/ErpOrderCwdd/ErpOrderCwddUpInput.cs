using QT.Application.Entitys.Dto.FreshDelivery.ErpOrder;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderdetail;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderCwdd;

public class ErpOrderCwddUpInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单商品表.
    /// </summary>
    public List<ErpOrderdetailCwddUpInput> erpOrderdetailList { get; set; }
}

public class ErpOrderdetailCwddUpInput
{
    public string id { get; set; }

    public decimal printNum { get; set; }

    public decimal printPrice { get; set; }

    public decimal printAmount { get; set; }
}

public class ErpOrderdetailCwddInfoOutput : ErpOrderdetailInfoOutput
{
    public decimal? printNum { get; set; }

    public decimal? printPrice { get; set; }

    public decimal? printAmount { get; set; }
}

public class ErpOrderCwddListOutput: ErpOrderListOutput
{

    public decimal? printAmount { get; set; }
}