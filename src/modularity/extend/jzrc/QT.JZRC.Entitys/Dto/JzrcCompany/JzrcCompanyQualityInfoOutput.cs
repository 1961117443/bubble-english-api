
namespace QT.JZRC.Entitys.Dto.JzrcCompanyQuality;

/// <summary>
/// 企业资质信息输出参数.
/// </summary>
public class JzrcCompanyQualityInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 企业id.
    /// </summary>
    public string companyId { get; set; }

    /// <summary>
    /// 资质名称.
    /// </summary>
    public string qualificationName { get; set; }

    /// <summary>
    /// 等级.
    /// </summary>
    public string level { get; set; }

    /// <summary>
    /// 专业.
    /// </summary>
    public string specialty { get; set; }

    /// <summary>
    /// 颁发时间.
    /// </summary>
    public DateTime? issuanceDate { get; set; }

    /// <summary>
    /// 颁发机构.
    /// </summary>
    public string issuingAuthority { get; set; }

    /// <summary>
    /// 有效期限.
    /// </summary>
    public string validityPeriod { get; set; }

}