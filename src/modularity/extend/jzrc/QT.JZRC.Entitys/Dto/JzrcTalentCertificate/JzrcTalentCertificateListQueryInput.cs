using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcTalentCertificate;

/// <summary>
/// 建筑人才证书信息列表查询输入
/// </summary>
public class JzrcTalentCertificateListQueryInput : PageInputBase
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
    /// 证书名称.
    /// </summary>
    public string certificateName { get; set; }

    /// <summary>
    /// 区域.
    /// </summary>
    public string region { get; set; }

    /// <summary>
    /// 证书分类.
    /// </summary>
    public string categoryId { get; set; }


    /// <summary>
    /// 人才
    /// </summary>
    public string talentId { get; set; }

    /// <summary>
    /// 企业
    /// </summary>
    public string companyId { get; set; }
}