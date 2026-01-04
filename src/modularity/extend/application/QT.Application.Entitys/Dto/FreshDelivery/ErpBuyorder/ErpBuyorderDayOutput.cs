using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;

public class ErpBuyorderDayOutput
{
    public string id { get; set; }

    /// <summary>
    /// 订单编号
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 所属公司
    /// </summary>
    public string oidName { get; set; }

    /// <summary>
    /// 供应商
    /// </summary>
    public string supplierName { get; set; }

    public string taskToUserId { get; set; }
    /// <summary>
    /// 采购员
    /// </summary>
    public string taskToUserName { get; set; }

    /// <summary>
    /// 采购渠道
    /// </summary>
    public string channel { get; set; }

    public string productName { get; set; }

    public string gidName { get; set; }
    public decimal planNum { get; set; }
}
