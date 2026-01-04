using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderdetail;
using QT.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrderPs;

/// <summary>
/// 订单信息更新输入.
/// </summary>
public class ErpOrderPsUpInput : ErpOrderPsCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }
}

public class ErpOrderPsAuditInput 
{
    /// <summary>
    /// 主键.
    /// </summary>
    [Required(ErrorMessage ="订单id不能为空")]
    public string id { get; set; }

    /// <summary>
    /// 凭据.
    /// </summary>
    [Required(ErrorMessage ="凭据不能为空")]
    public List<FileControlsModel> files { get; set; }

    /// <summary>
    /// 订单明细
    /// </summary>
    public List<ErpOrderdetailInfoOutput> erpOrderdetailList { get;set; }
}

public class ErpOrderPsAuditDetailInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [Required(ErrorMessage = "订单明细id不能为空")]
    public string id { get; set; }

    /// <summary>
    /// 送达状态.
    /// 1:送达，0:未送达
    /// </summary>
    public int receiveState { get; set; }

    /// <summary>
    /// 复合数量
    /// </summary>
    public decimal num2 { get; set; }
}