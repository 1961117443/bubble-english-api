using QT.DependencyInjection;

namespace QT.Common.Models.Authorize;

/// <summary>
/// 数据权限条件字段.
/// </summary>
[SuppressSniffer]
public class AuthorizeModuleResourceConditionItemModel
{
    /// <summary>
    /// ID.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 字段.
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// 字段类型.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// 条件.
    /// </summary>
    public string Op { get; set; }

    /// <summary>
    /// 值.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 绑定表.
    /// </summary>
    public string BindTable { get; set; }

    /// <summary>
    /// 规则(0:主表，1：副表).
    /// </summary>
    public int FieldRule { get; set; }
}