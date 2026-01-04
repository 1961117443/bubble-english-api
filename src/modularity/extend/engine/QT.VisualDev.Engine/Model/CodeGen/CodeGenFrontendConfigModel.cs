using QT.DependencyInjection;

namespace QT.VisualDev.Engine.Model.CodeGen;

/// <summary>
/// 代码生成前端配置模型.
/// </summary>
[SuppressSniffer]
public class CodeGenFrontendConfigModel
{
    /// <summary>
    /// 表单ref.
    /// </summary>
    public string FormRef { get; set; }

    /// <summary>
    /// 表单Model.
    /// </summary>
    public string FromModel { get; set; }

    /// <summary>
    /// 表单宽度.
    /// </summary>
    public string LabelWidth { get; set; }

    /// <summary>
    /// 表单位置.
    /// </summary>
    public string LabelPosition { get; set; }
}