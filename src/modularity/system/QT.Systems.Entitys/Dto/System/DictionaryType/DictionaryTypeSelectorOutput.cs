using QT.Common.Security;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.DictionaryType;

/// <summary>
/// 字典类型下拉框输出.
/// </summary>
[SuppressSniffer]
public class DictionaryTypeSelectorOutput : TreeModel
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public long? sortCode { get; set; }
}