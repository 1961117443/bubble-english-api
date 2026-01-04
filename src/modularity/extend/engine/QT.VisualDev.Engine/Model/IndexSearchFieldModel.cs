using QT.DependencyInjection;
using QT.VisualDev.Engine.Model.CodeGen;

namespace QT.VisualDev.Engine;

/// <summary>
/// 列表查询字段模型.
/// </summary>
[SuppressSniffer]
public class IndexSearchFieldModel : IndexEachConfigBase
{
    /// <summary>
    /// 值.
    /// </summary>
    public string value { get; set; }

    /// <summary>
    /// 查询类型.
    /// </summary>
    public int? searchType { get; set; }


    /// <summary>
    /// SqlSugar查询类型.
    /// </summary>
    public string conditionalType { get; set; }
}
