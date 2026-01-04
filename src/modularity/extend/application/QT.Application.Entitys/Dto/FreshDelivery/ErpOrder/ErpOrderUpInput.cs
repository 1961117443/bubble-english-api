using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrder;

/// <summary>
/// 订单信息更新输入.
/// </summary>
public class ErpOrderUpInput : ErpOrderCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }
}


public class ErpOrderDetailUpPartInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    [Required]
    public string id { get; set; }

    [Required]
    public string field { get; set; }

    public string value { get; set; }
}