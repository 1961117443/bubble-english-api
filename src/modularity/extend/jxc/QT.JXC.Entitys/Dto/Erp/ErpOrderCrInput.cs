using QT.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 订单信息修改输入参数.
/// </summary>
public class ErpOrderCrInput
{
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

    /// <summary>
    /// 约定送货时间.
    /// </summary>
    public DateTime? posttime { get; set; }

    /// <summary>
    /// 订单商品表.
    /// </summary>
    public List<ErpOrderdetailCrInput> erpOrderdetailList { get; set; }

    /// <summary>
    /// 订单处理记录表.
    /// </summary>
    public List<ErpOrderoperaterecordCrInput> erpOrderoperaterecordList { get; set; }

    /// <summary>
    /// 订单备注记录.
    /// </summary>
    public List<ErpOrderremarksCrInput> erpOrderremarksList { get; set; }


    /// <summary>
    /// 配送凭据.
    /// </summary>
    public List<FileControlsModel> deliveryProof { get; set; }

    /// <summary>
    /// 送达凭据.
    /// </summary>
    public List<FileControlsModel> deliveryToProof { get; set; }


    public string oid { get; set; }

    public DateTime? printTime { get; set; }
    public DateTime? deliveryTime { get; set; }
    public DateTime? deliveryToTime { get; set; }
    public string deliveryManId { get; set; }
    public string deliveryCar { get; set; }

    /// <summary>
    /// 餐别.
    /// </summary>
    public string diningType { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string remark { get; set; }
}