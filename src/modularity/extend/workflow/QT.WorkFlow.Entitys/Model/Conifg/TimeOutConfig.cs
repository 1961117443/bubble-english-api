using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Model.Conifg;

[SuppressSniffer]
public class TimeOutConfig
{
    /// <summary>
    /// 开关.
    /// </summary>
    public bool on { get; set; }

    /// <summary>
    /// 数量.
    /// </summary>
    public int quantity { get; set; }

    /// <summary>
    /// 类型 day、 hour、 minute.
    /// </summary>
    public string? type { get; set; }

    /// <summary>
    /// 同意1 拒绝2.
    /// </summary>
    public int handler { get; set; }
}
