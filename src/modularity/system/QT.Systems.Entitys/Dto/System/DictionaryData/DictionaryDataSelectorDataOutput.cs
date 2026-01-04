using QT.Common.Security;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.DictionaryData;

/// <summary>
/// 数据字典下拉框数据输出.
/// </summary>
[SuppressSniffer]
public class DictionaryDataSelectorDataOutput : TreeModel
{
    /// <summary>
    /// 项目名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 图标.
    /// </summary>
    public string enCode { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public long? sortCode { get; set; }
}