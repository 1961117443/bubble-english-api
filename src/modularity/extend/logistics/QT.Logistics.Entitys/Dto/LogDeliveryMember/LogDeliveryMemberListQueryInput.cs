using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogDeliveryMember;

/// <summary>
/// 配送点会员列表查询输入
/// </summary>
public class LogDeliveryMemberListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 配送点ID.
    /// </summary>
    public string pointId { get; set; }

    /// <summary>
    /// 线路ID.
    /// </summary>
    public string memberId { get; set; }

}