using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.Position;

/// <summary>
/// 岗位列表查询输入.
/// </summary>
[SuppressSniffer]
public class PositionListQuery : PageInputBase
{
    /// <summary>
    /// 机构ID.
    /// </summary>
    public string organizeId { get; set; }

    /// <summary>
    /// 组织Id集合.
    /// </summary>
    public List<string> organizeIds { get; set; }
}