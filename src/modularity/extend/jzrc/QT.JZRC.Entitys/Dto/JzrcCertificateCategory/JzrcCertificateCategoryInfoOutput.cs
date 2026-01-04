
namespace QT.JZRC.Entitys.Dto.JzrcCertificateCategory;

/// <summary>
/// 证书分类输出参数.
/// </summary>
public class JzrcCertificateCategoryInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public int? order { get; set; }

}