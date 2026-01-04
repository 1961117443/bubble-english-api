using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpOrder;
public class ErpOrderBatchInput
{
    /// <summary>
    /// 订单集合.
    /// </summary>
    [Required]
    public string[] items { get; set; }
}

public class ErpOrderStateBatchUpInput: ErpOrderBatchInput
{
    /// <summary>
    /// 操作日期
    /// </summary>
    [Required]
    public DateTime? date { get; set; }
}