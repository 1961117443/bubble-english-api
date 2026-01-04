using QT.DependencyInjection;

namespace QT.Systems.Entitys.Model.Menu;

/// <summary>
/// 功能模型.
/// </summary>
[SuppressSniffer]
public class FunctionalModel : FunctionalBase
{
    /// <summary>
    /// 功能类别【1-类别、2-页面】..
    /// </summary>
    public int? Type { get; set; }

    /// <summary>
    /// 功能地址.
    /// </summary>
    public string UrlAddress { get; set; }

    /// <summary>
    /// 图标.
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 菜单分类（Web | App）
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
}