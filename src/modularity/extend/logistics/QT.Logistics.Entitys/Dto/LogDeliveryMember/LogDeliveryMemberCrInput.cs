
namespace QT.Logistics.Entitys.Dto.LogDeliveryMember;

/// <summary>
/// 配送点会员修改输入参数.
/// </summary>
public class LogDeliveryMemberCrInput
{
    /// <summary>
    /// 配送点ID.
    /// </summary>
    public string pointId { get; set; }

    /// <summary>
    /// 线路ID.
    /// </summary>
    public string memberId { get; set; }

}