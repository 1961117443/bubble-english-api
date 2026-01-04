using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcCertificateIn;

/// <summary>
/// 建筑人才档案收件列表查询输入
/// </summary>
public class JzrcCertificateInListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 档案位置id.
    /// </summary>
    public string storeroomId { get; set; }

    /// <summary>
    /// 收件时间.
    /// </summary>
    public string inTime { get; set; }

    /// <summary>
    /// 快递单号.
    /// </summary>
    public string expressNo { get; set; }

    /// <summary>
    /// 人才
    /// </summary>
    public string talentId { get; set; }

    /// <summary>
    /// 企业
    /// </summary>
    public string companyId { get; set; }

}