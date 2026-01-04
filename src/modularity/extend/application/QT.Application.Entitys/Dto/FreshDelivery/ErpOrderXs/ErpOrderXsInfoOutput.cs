using QT.Application.Entitys.Dto.FreshDelivery.ErpOrder;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderdetail;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderoperaterecord;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderremarks;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderXs;

/// <summary>
/// 订单信息输出参数.
/// </summary>
public class ErpOrderXsInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cid { get; set; }

    /// <summary>
    /// 约定送货时间.
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 订单商品表.
    /// </summary>
    public List<ErpOrderdetailXsInfoOutput> erpOrderdetailList { get; set; }

    /// <summary>
    /// 订单处理记录表.
    /// </summary>
    public List<ErpOrderoperaterecordInfoOutput> erpOrderoperaterecordList { get; set; }

    /// <summary>
    /// 订单备注记录.
    /// </summary>
    public List<ErpOrderremarksInfoOutput> erpOrderremarksList { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public int state { get; set; }



    /// <summary>
    /// 下单时间.
    /// </summary>
    public DateTime? createTime { get; set; }


    /// <summary>
    /// 餐别
    /// </summary>
    public string diningType { get; set; }


    /// <summary>
    /// 子单记录
    /// </summary>
    public List<ErpSubOrderListOutput> erpChildOrderList { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string remark { get; set; }
}