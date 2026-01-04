using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderdetail;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderremarks;
using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderXs;

/// <summary>
/// 订单信息修改输入参数.
/// </summary>
public class ErpOrderXsCrInput
{
    /// <summary>
    /// 订单编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 客户ID.
    /// </summary>
    [Required(ErrorMessage ="客户不能为空！")]
    public string cid { get; set; }

    /// <summary>
    /// 约定送货时间.
    /// </summary>
    public DateTime? posttime { get; set; }


    /// <summary>
    /// 下单时间.
    /// </summary>
    public DateTime? createTime { get; set; }

    /// <summary>
    /// 订单商品表.
    /// </summary>
    public List<ErpOrderdetailCrInput> erpOrderdetailList { get; set; }

    ///// <summary>
    ///// 订单处理记录表.
    ///// </summary>
    //public List<ErpOrderoperaterecordCrInput> erpOrderoperaterecordList { get; set; }

    /// <summary>
    /// 订单备注记录.
    /// </summary>
    public List<ErpOrderremarksCrInput> erpOrderremarksList { get; set; }

    /// <summary>
    /// 餐别.
    /// </summary>
    public string diningType { get; set; }


    /// <summary>
    /// 备注
    /// </summary>
    public string remark { get; set; }
}