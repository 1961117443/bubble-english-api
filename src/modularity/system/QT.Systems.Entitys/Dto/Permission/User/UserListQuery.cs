using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.User;

/// <summary>
/// 用户列表查询输入.
/// </summary>
[SuppressSniffer]
public class UserListQuery : PageInputBase
{
    /// <summary>
    /// 机构ID.
    /// </summary>
    public string organizeId { get; set; }

    /// <summary>
    /// 岗位ID.
    /// </summary>
    public string positionId { get; set; }

    /// <summary>
    /// 隐藏客户信息
    /// </summary>
    public bool hideCustomer { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    public int? origin { get; set; }
}