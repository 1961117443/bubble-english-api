
namespace QT.JZRC.Entitys.Dto.JzrcCertificateCategory;

/// <summary>
/// 证书分类修改输入参数.
/// </summary>
public class JzrcCertificateCategoryCrInput
{
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