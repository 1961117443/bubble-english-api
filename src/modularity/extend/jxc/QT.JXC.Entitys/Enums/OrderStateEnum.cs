using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Enums;

public enum OrderStateEnum
{
    /// <summary>
    /// 草稿，待生效
    /// </summary>
    [Description("草稿，待生效")]
    Draft = 1,

    /// <summary>
    /// 生效，待分拣
    /// </summary>
    [Description("生效，待分拣")]
    PendingApproval = 2,

    /// <summary>
    /// 已分拣，待出库
    /// </summary>
    [Description("已分拣，待出库")]
    Picked = 3,

    /// <summary>
    /// 已出库，待配送
    /// </summary>
    [Description("已出库，待配送")]
    Outbound = 4,

    /// <summary>
    /// 已配送，待收货
    /// </summary>
    [Description("已配送，待收货")]
    Delivery = 5,

    /// <summary>
    /// 已收货，待结算
    /// </summary>
    [Description("已收货，待结算")]
    Receiving = 6,

    /// <summary>
    /// 已结算，待收款
    /// </summary>
    [Description("已结算，待收款")]
    Checkout = 7,

    /// <summary>
    /// 已收款
    /// </summary>
    [Description("已收款")]
    Paid = 8,

    /// <summary>
    /// 已取消
    /// </summary>
    [Description("已取消")]
    Cancelled = 9,

    ///// <summary>
    ///// 完结
    ///// </summary>
    //[Description("完结")]
    //Finish = 9

    /// <summary>
    /// 拆单（分批配送）
    /// </summary>
    [Description("拆单")]
    Split = 99,
}
