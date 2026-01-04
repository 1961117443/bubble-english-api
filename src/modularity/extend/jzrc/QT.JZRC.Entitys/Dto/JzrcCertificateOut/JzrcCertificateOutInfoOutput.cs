
namespace QT.JZRC.Entitys.Dto.JzrcCertificateOut;

/// <summary>
/// 建筑人才档案寄件输出参数.
/// </summary>
public class JzrcCertificateOutInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 证书id.
    /// </summary>
    public string certificateId { get; set; }

    /// <summary>
    /// 寄件时间.
    /// </summary>
    public DateTime? outTime { get; set; }

    /// <summary>
    /// 快递单号.
    /// </summary>
    public string expressNo { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

}