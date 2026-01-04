using QT.Common.Models;

namespace QT.JZRC.Entitys.Dto.JzrcCertificateIn;

/// <summary>
/// 建筑人才档案收件输入参数.
/// </summary>
public class JzrcCertificateInListOutput
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
    /// 证书名称
    /// </summary>
    public string certificateIdName { get; set; }

    /// <summary>
    /// 档案位置.
    /// </summary>
    public string storeroomIdName { get; set; }



    /// <summary>
    /// 经手人.
    /// </summary>
    public string handledBy { get; set; }

    /// <summary>
    /// 类型（0:借入，1借出）.
    /// </summary>
    public int inoutType { get; set; }

    /// <summary>
    /// 人才
    /// </summary>
    public string talentIdName { get; set; }

    /// <summary>
    /// 人才id
    /// </summary>
    public string talentId { get; set; }

    /// <summary>
    /// 公司名称
    /// </summary>
    public string companyIdName { get; set; }
}