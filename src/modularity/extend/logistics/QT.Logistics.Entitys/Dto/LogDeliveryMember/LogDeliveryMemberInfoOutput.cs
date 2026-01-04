
namespace QT.Logistics.Entitys.Dto.LogDeliveryMember;

/// <summary>
/// 配送点会员输出参数.
/// </summary>
public class LogDeliveryMemberInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 配送点ID.
    /// </summary>
    public string pointId { get; set; }

    /// <summary>
    /// 线路ID.
    /// </summary>
    public string memberId { get; set; }

}