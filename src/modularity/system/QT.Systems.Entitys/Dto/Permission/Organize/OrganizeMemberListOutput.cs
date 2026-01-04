using System.Text.Json.Serialization;
using QT.Common.Security;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.Organize;

/// <summary>
/// 机构成员列表输出.
/// </summary>
[SuppressSniffer]
public class OrganizeMemberListOutput : TreeModel
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 有效标记.
    /// </summary>
    public int? enabledMark { get; set; }

    /// <summary>
    /// 类型.
    /// </summary>
    public string type { get; set; }

    /// <summary>
    /// 图标.
    /// </summary>
    public string icon { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    [JsonIgnore]
    public long? SortCode { get; set; }

    /// <summary>
    /// 账号.
    /// </summary>
    [JsonIgnore]
    public string Account { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    [JsonIgnore]
    public string RealName { get; set; }

    /// <summary>
    /// 删除标志.
    /// </summary>
    [JsonIgnore]
    public int? DeleteMark { get; set; }
}