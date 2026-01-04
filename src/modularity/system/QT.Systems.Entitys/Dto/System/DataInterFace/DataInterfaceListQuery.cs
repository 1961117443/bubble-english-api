using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.DataInterFace;

/// <summary>
/// 数据接口列表查询输入.
/// </summary>
[SuppressSniffer]
public class DataInterfaceListQuery : PageInputBase
{
    /// <summary>
    /// 分类id.
    /// </summary>
    public string categoryId { get; set; }

    /// <summary>
    /// 数据类型.
    /// </summary>
    public string dataType { get; set; }
}