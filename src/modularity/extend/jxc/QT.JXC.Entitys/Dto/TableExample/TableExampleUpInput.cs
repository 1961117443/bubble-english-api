using QT.DependencyInjection;

namespace QT.JXC.Entitys.Dto.TableExample;

/// <summary>
/// 更新项目.
/// </summary>
[SuppressSniffer]
public class TableExampleUpInput : TableExampleCrInput
{
    /// <summary>
    /// 主键id.
    /// </summary>
    public string? id { get; set; }
}
