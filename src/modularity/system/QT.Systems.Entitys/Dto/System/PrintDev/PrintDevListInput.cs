using QT.Common.Filter;
using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.PrintDev;

/// <summary>
/// 打印模板列表查询输入.
/// </summary>
[SuppressSniffer]
public class PrintDevListInput : PageInputBase
{
    /// <summary>
    /// 分类.
    /// </summary>
    public string category { get; set; }

    /// <summary>
    /// 绑定模块
    /// </summary>
    public string moduleName { get; set; }
}