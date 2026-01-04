using QT.DependencyInjection;
using QT.PRM.Entitys;
using System.ComponentModel.DataAnnotations;

namespace QT.PRM.Dto.Receivable;

/// <summary>
/// 应收记录创建输入
/// </summary>
[SuppressSniffer]
public class ReceivableCrInput
{
    /// <summary>
    /// 房间费用ID
    /// </summary>
    [Required(ErrorMessage = "房间费用ID不能为空")]
    public string roomFeeId { get; set; }

    /// <summary>
    /// 应收金额
    /// </summary>
    [Required(ErrorMessage = "应收金额不能为空")]
    [Range(0, double.MaxValue, ErrorMessage = "应收金额必须大于等于0")]
    public decimal amount { get; set; }

    /// <summary>
    /// 到期日期
    /// </summary>
    [Required(ErrorMessage = "到期日期不能为空")]
    public DateTime? dueDate { get; set; }

    /// <summary>
    /// 应收状态
    /// </summary>
    [Required(ErrorMessage = "应收状态不能为空")]
    public ReceivableStatus status { get; set; } = ReceivableStatus.未缴;
}
