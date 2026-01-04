using QT.Common.Filter;
using QT.SDMS.Entitys.Entity;

namespace QT.SDMS.Entitys.Dto.CustomerTicket;

public class CustomerTicketListQueryInput:PageInputBase
{
    ///// <summary>
    ///// 订单日期范围
    ///// </summary>
    //public string orderDate { get; set; }

    /// <summary>
    /// 认证状态
    /// </summary>
    public TicketStatus? status { get; set; }

    /// <summary>
    /// 客户id
    /// </summary>
    public string customerId { get; set; }
}
