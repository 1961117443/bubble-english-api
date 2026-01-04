using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpBuyorder;

public class ErpBuyorderTransferListOutput
{
    /// <summary>
    /// 特殊入库id
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 特殊入库时间
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 特殊入库数量
    /// </summary>
    public decimal num { get; set; }

    /// <summary>
    /// 特殊入库单号
    /// </summary>
    public string no { get; set; }


    /// <summary>
    /// 订单编号
    /// </summary>
    public string orderNo { get; set; }

    /// <summary>
    /// 约定配送时间
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 是否选中
    /// </summary>
    public bool ifselected { get; set; }

    /// <summary>
    /// 已关联数量
    /// </summary>
    public decimal num1 { get; set; }

    /// <summary>
    /// 本次关联数量
    /// </summary>
    public decimal num2 { get; set; }

    /// <summary>
    /// 客户名称
    /// </summary>
    public string customerName { get; set; }

    /// <summary>
    /// 客户类型
    /// </summary>
    public string customerType { get; set; }
}
