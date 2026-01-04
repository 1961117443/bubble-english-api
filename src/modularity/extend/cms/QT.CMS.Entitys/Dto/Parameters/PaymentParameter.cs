namespace QT.CMS.Entitys.Dto.Parameter;

/// <summary>
/// 支付方式查询参数
/// </summary>
public class PaymentParameter : BaseParameter
{
    /// <summary>
    /// 接口类型
    /// 示例：jsapi,h5...
    /// </summary>
    public string? Types { get; set; }
}
