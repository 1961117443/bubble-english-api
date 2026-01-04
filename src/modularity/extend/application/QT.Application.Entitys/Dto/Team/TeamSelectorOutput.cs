using QT.Common.Security;
using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.Team;

/// <summary>
/// 分组下拉框输出.
/// </summary>
[SuppressSniffer]
public class TeamSelectorOutput : TreeModel
{
    /// <summary>
    /// 分组名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 有效标志.
    /// </summary>
    public bool enabledMark { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public long? sortCode { get; set; }
}