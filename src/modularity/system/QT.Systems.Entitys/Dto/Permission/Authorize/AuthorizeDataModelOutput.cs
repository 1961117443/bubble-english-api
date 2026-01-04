using QT.Common.Security;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.Authorize;

/// <summary>
/// 权限数据输出模型.
/// </summary>
[SuppressSniffer]
public class AuthorizeDataModelOutput : TreeModel
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 图标.
    /// </summary>
    public string icon { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public long? sortCode { get; set; }

    /// <summary>
    /// 菜单分类
    /// </summary>
    public string category { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string description { get; set; }
}