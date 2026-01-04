using QT.DependencyInjection;

namespace QT.VisualDev.Engine.Model.CodeGen;

/// <summary>
/// 代码生成功能模型.
/// </summary>
[SuppressSniffer]
public class CodeGenFunctionModel
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// 是否接口.
    /// </summary>
    public bool IsInterface { get; set; }
}