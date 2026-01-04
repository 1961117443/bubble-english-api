using QT.DependencyInjection;

namespace QT.Common.Models.Authorize;

/// <summary>
/// 数据权限条件字段.
/// </summary>
[SuppressSniffer]
public class AuthorizeModuleResourceConditionModel
{
    /// <summary>
    /// 逻辑.
    /// </summary>
    public string Logic { get; set; }

    /// <summary>
    /// 分组.
    /// </summary>
    public List<AuthorizeModuleResourceConditionItemModel> Groups { get; set; }
}