using System.ComponentModel.DataAnnotations;

namespace QT.JXC.Entitys.Dto.Erp;

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