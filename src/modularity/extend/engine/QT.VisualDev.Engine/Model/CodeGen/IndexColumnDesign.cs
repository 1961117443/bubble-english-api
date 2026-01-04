using QT.DependencyInjection;

namespace QT.VisualDev.Engine.Model.CodeGen;

/// <summary>
/// 代码生成常规Index列表列设计.
/// </summary>
[SuppressSniffer]
public class IndexColumnDesign
{
    /// <summary>
    /// 控件名称.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Options名称.
    /// </summary>
    public string OptionsName { get; set; }

    /// <summary>
    /// 首字母小写列名.
    /// </summary>
    public string LowerName { get; set; }

    /// <summary>
    /// 控件Key.
    /// </summary>
    public string qtKey { get; set; }

    /// <summary>
    /// 文本.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// 宽度.
    /// </summary>
    public string Width { get; set; }

    /// <summary>
    /// Align.
    /// </summary>
    public string Align { get; set; }

    /// <summary>
    /// 是否自动转换.
    /// </summary>
    public bool IsAutomatic { get; set; }

    /// <summary>
    /// 是否排序.
    /// </summary>
    public string IsSort { get; set; }
}