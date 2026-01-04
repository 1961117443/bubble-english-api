namespace QT.CMS.Entitys.Dto.Parameter;

/// <summary>
/// 商品评价参数
/// </summary>
public class EvaluateParameter : BaseParameter
{
    /// <summary>
    /// 评分
    /// </summary>
    public byte Score { get; set; } = 0;

    /// <summary>
    /// 是否有图
    /// </summary>
    public byte Image { get; set; } = 0;
}
