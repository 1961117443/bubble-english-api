using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderdetail;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderoperaterecord;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderremarks;
using QT.Common.Models;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrder;

/// <summary>
/// 订单信息输出参数.
/// </summary>
public class ErpOrderInfoOutput
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
    /// 代下单人员.
    /// </summary>
    public string createUid { get; set; }

    /// <summary>
    /// 代下单时间.
    /// </summary>
    public DateTime? createTime { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    public string cid { get; set; }
    public string oid { get; set; }

    /// <summary>
    /// 约定送货时间.
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 订单商品表.
    /// </summary>
    public List<ErpOrderdetailInfoOutput> erpOrderdetailList { get; set; }

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

    public DateTime? printTime { get; set; }
    public DateTime? deliveryTime { get; set; }
    public DateTime? deliveryToTime { get; set; }
    public string deliveryManId { get; set; }
    public string deliveryCar { get; set; }

    /// <summary>
    /// 配送凭据.
    /// </summary>
    public List<FileControlsModel> deliveryProof { get; set; }

    /// <summary>
    /// 送达凭据.
    /// </summary>
    public List<FileControlsModel> deliveryToProof { get; set; }


    public decimal amount { get; set; }

    /// <summary>
    /// 订单打印记录.
    /// </summary>
    public List<ErpPrintrecordInfoOutput> erpPrintrecordList { get; set; }

    /// <summary>
    /// 餐别.
    /// </summary>
    public string diningType { get; set; }

    /// <summary>
    /// 子单记录
    /// </summary>
    public List<ErpSubOrderListOutput> erpChildOrderList { get; set; }

    public string remark { get; set; }
}