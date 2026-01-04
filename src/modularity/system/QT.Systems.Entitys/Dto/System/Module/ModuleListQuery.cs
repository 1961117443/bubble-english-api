using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.Module;

/// <summary>
/// 功能列表查询
/// </summary>
[SuppressSniffer]
public class ModuleListQuery : KeywordInput
{
    /// <summary>
    /// 分类
    /// </summary>
    public string category { get; set; }
}
