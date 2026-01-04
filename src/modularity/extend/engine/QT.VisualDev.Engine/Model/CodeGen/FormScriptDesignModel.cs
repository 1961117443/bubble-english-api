using QT.DependencyInjection;

namespace QT.VisualDev.Engine.Model.CodeGen;

/// <summary>
/// 代码生成表单列模型.
/// </summary>
[SuppressSniffer]
public class FormScriptDesignModel
{
    /// <summary>
    /// 列名.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 原始列名.
    /// </summary>
    public string OriginalName { get; set; }

    /// <summary>
    /// 首字母小写列名.
    /// </summary>
    public string LowerName => string.IsNullOrWhiteSpace(Name) ? null : Name.Substring(0, 1).ToLower() + Name[1..];

    /// <summary>
    /// 标签类型.
    /// </summary>
    public string qtKey { get; set; }

    /// <summary>
    /// 数据类型.
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// 字典类型.
    /// </summary>
    public string DictionaryType { get; set; }

    /// <summary>
    /// 时间格式化.
    /// </summary>
    public string Format { get; set; }

    /// <summary>
    /// 多选标记.
    /// </summary>
    public bool Multiple { get; set; }

    /// <summary>
    /// 自动生成规则.
    /// </summary>
    public string BillRule { get; set; }

    /// <summary>
    /// 必填.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// 验证规则.
    /// </summary>
    public List<RegListModel> RegList { get; set; }

    /// <summary>
    /// 提示时机.
    /// </summary>
    public string Trigger { get; set; }

    /// <summary>
    /// 提示语.
    /// </summary>
    public string Placeholder { get; set; }

    /// <summary>
    /// 范围.
    /// </summary>
    public bool Range { get; set; }

    /// <summary>
    /// 默认值.
    /// </summary>
    public object DefaultValue { get; set; }

    /// <summary>
    /// 子控件列表.
    /// </summary>
    public List<FormScriptDesignModel> ChildrenList { get; set; }

    /// <summary>
    /// 是否开启合计.
    /// </summary>
    public bool ShowSummary { get; set; }

    /// <summary>
    /// 合计数组.
    /// </summary>
    public string SummaryField { get; set; }

    /// <summary>
    /// 是否合计.
    /// </summary>
    public bool IsSummary { get; set; }
}