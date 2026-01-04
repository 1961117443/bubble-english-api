namespace QT.CMS.Entitys.Dto.Parameter;

/// <summary>
/// 运费查询参数
/// </summary>
public class DeliveryParameter : BaseParameter
{
    /// <summary>
    /// 所属省份
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// 总重量(克)
    /// </summary>
    public int TotalWeight { get; set; }

    /// <summary>
    /// 是否保价
    /// </summary>
    public byte IsInsure { get; set; }

    /// <summary>
    /// 保价金额
    /// </summary>
    public decimal InsurePrice { get; set; }

}
