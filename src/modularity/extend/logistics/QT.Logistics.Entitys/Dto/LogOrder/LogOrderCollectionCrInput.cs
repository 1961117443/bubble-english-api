
namespace QT.Logistics.Entitys.Dto.LogOrderFinancial;

/// <summary>
/// 订单财务明细修改输入参数.
/// </summary>
public class LogOrderCollectionCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
    /// <summary>
    /// 收款金额.
    /// </summary>
    public decimal amount { get; set; }

    /// <summary>
    /// 收款方.
    /// </summary>
    public string payee { get; set; }
}

public class LogOrderCollectionInfoOutput : LogOrderCollectionCrInput
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? creatorTime { get; set; }
}