namespace QT.VisualDev.Entitys.Dto.Portal;

/// <summary>
/// 门户设计信息输出.
/// </summary>
public class PortalInfoAuthOutput
{
    /// <summary>
    /// 表单JSON.
    /// </summary>
    public string formData { get; set; }

    /// <summary>
    /// 类型(0-页面设计,1-自定义路径).
    /// </summary>
    public int? type { get; set; }

    /// <summary>
    /// 静态页面路径.
    /// </summary>
    public string customUrl { get; set; }

    /// <summary>
    /// 链接类型(0-页面,1-外链).
    /// </summary>
    public int? linkType { get; set; }
}
