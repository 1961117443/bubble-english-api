using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.Team;

/// <summary>
/// 创建分组输入.
/// </summary>
[SuppressSniffer]
public class TeamCrInput
{
    /// <summary>
    /// 分组编码.
    /// </summary>
    public string enCode { get; set; }

    /// <summary>
    /// 分组名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 分组类型.
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// 有效标志.
    /// </summary>
    public int? enabledMark { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public long? sortCode { get; set; }

    /// <summary>
    /// 管理人员.
    /// </summary>
    public string? managerIds { get; set; }

    /// <summary>
    /// 参与人员.
    /// </summary>
    public string? memberIds { get; set; }
}