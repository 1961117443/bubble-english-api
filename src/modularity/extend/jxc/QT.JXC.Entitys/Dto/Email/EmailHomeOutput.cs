using QT.DependencyInjection;

namespace QT.JXC.Entitys.Dto.Email;

[SuppressSniffer]
public class EmailHomeOutput
{
    /// <summary>
    /// ID.
    /// </summary>
    public string? id { get; set; }

    /// <summary>
    /// 标题.
    /// </summary>
    public string? fullName { get; set; }

    /// <summary>
    /// 创建时间.
    /// </summary>
    public DateTime? creatorTime { get; set; }
}
