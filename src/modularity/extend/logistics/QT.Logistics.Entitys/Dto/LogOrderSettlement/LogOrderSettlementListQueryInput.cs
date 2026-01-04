using QT.Logistics.Entitys.Dto.LogOrder;

namespace QT.Logistics.Entitys.Dto.LogOrderSettlement;

public class LogOrderSettlementListQueryInput : LogOrderListQueryInput
{
    /// <summary>
    /// 已结算=1，未结算=0
    /// </summary>
    public int? settlementStatus { get; set; }

    /// <summary>
    /// 寄件/送达配送点
    /// </summary>
    public string pointId { get; set; }


    /// <summary>
    /// 数据范围 scope=point只看当前用户绑定的配送点
    /// </summary>
    public string scope { get; set; }
}
