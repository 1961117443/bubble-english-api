using QT.DependencyInjection;

namespace QT.VisualDev.Engine;

/// <summary>
/// 显示列模型.
/// </summary>
[SuppressSniffer]
public class IndexGridFieldModel : IndexEachConfigBase
{
    /// <summary>
    /// 对齐.
    /// </summary>
    public string align { get; set; }

    /// <summary>
    /// 宽度.
    /// </summary>
    public int? width { get; set; } = 1;

    /// <summary>
    /// 是否排序.
    /// </summary>
    public bool sortable { get; set; }

    /// <summary>
    /// 是否隐藏
    /// </summary>
    public bool hidden { get; set; }

    /// <summary>
    /// 是否合计
    /// </summary>
    public bool summary { get; set; }
}