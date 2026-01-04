using System.Text.Json.Serialization;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.Systems.Entitys.Enum;

namespace QT.Systems.Entitys.Dto.Module;

/// <summary>
/// 功能节点输出.
/// </summary>
[SuppressSniffer]
public class ModuleNodeOutput : TreeModel
{
    /// <summary>
    /// 菜单名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 菜单编码.
    /// </summary>
    public string enCode { get; set; }

    /// <summary>
    /// 菜单图标.
    /// </summary>
    public string icon { get; set; }

    /// <summary>
    /// 菜单分类【1-类别、2-页面】.
    /// </summary>
    public MenuType type { get; set; }

    /// <summary>
    /// 菜单地址.
    /// </summary>
    public string urlAddress { get; set; }

    /// <summary>
    /// 链接目标.
    /// </summary>
    public string linkTarget { get; set; }

    /// <summary>
    /// 菜单分类.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MenuCategory category { get; set; }

    /// <summary>
    /// 扩展属性.
    /// </summary>
    public string propertyJson { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public long? sortCode { get; set; }

    /// <summary>
    /// 路由地址地址.
    /// </summary>
    public string urlRoute { get; set; }
}
