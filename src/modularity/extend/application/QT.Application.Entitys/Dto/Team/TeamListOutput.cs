using QT.DependencyInjection;
using QT.Extend.Entitys.Dto.ProjectGantt;

namespace QT.Extend.Entitys.Dto.Team;

/// <summary>
/// 分组列表输出.
/// </summary>
[SuppressSniffer]
public class TeamListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 分组名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 分组编号.
    /// </summary>
    public string enCode { get; set; }

    /// <summary>
    /// 分组类型.
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// 有效标志.
    /// </summary>
    public int? enabledMark { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 描述.
    /// </summary>
    public string description { get; set; }

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

    /// <summary>
    /// 负责人员信息.
    /// </summary>
    public List<ManagersInfo> managersInfo { get; set; } = new List<ManagersInfo>();


    /// <summary>
    /// 参加人员信息.
    /// </summary>
    public List<ManagersInfo> membersInfo { get; set; } = new List<ManagersInfo>();
}