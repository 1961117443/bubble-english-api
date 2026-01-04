
using QT.Common.Models;
using SqlSugar;

namespace QT.JZRC.Entitys.Dto.JzrcCertificateIn;

/// <summary>
/// 建筑人才档案收件修改输入参数.
/// </summary>
public class JzrcCertificateInCrInput
{
    /// <summary>
    /// 证书id.
    /// </summary>
    public string certificateId { get; set; }

    /// <summary>
    /// 档案位置id.
    /// </summary>
    public string storeroomId { get; set; }

    /// <summary>
    /// 收件时间.
    /// </summary>
    public DateTime? inTime { get; set; }

    /// <summary>
    /// 快递单号.
    /// </summary>
    public string expressNo { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }



    /// <summary>
    /// 经手人.
    /// </summary>
    public string handledBy { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    public List<FileControlsModel> attachment { get; set; }

    /// <summary>
    /// 类型（0:借入，1借出）.
    /// </summary>
    public int inoutType { get; set; }
}