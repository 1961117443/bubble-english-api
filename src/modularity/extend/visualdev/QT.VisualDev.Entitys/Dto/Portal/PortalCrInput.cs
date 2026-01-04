namespace QT.VisualDev.Entitys.Dto.Portal;

/// <summary>
/// 门户设计创建输入.
/// </summary>
public class PortalCrInput
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 分类.
    /// </summary>
    public string category { get; set; }

    /// <summary>
    /// 编码.
    /// </summary>
    public string enCode { get; set; }

    /// <summary>
    /// 是否可用
    /// 0-不可用，1-可用.
    /// </summary>
    public int enabledMark { get; set; } = 0;

    /// <summary>
    /// 说明.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 表单JSON.
    /// </summary>
    public string formData { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public string sortCode { get; set; }

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
