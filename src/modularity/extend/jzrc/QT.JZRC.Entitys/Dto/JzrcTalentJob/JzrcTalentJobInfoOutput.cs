
namespace QT.JZRC.Entitys.Dto.JzrcTalentJob;

/// <summary>
/// 建筑人才求职信息输出参数.
/// </summary>
public class JzrcTalentJobInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 人才id.
    /// </summary>
    public string talentId { get; set; }

    /// <summary>
    /// 招聘地区.
    /// </summary>
    public string region { get; set; }

    /// <summary>
    /// 求职类型.
    /// </summary>
    public string candidateType { get; set; }

    /// <summary>
    /// 薪资要求（年薪）.
    /// </summary>
    public decimal jobSalary { get; set; }

    /// <summary>
    /// 社保情况.
    /// </summary>
    public string socialSecurityStatus { get; set; }

    /// <summary>
    /// 业绩情况.
    /// </summary>
    public string performanceSituation { get; set; }

    /// <summary>
    /// 证书id.
    /// </summary>
    public string certificateId { get; set; }

    /// <summary>
    /// 求职时间起.
    /// </summary>
    public DateTime? requiredStart { get; set; }

    /// <summary>
    /// 求职时间止.
    /// </summary>
    public DateTime? requiredEnd { get; set; }

    /// <summary>
    /// 一口价.
    /// </summary>
    public decimal price { get; set; }

    public List<JzrcTalentJobCompanyInfo> jzrcTalentJobCompanys { get; set; }
}

public class JzrcTalentJobCompanyInfo
{
    /// <summary>
    /// 企业
    /// </summary>
    public string companyIdName { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public int status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? creatorTime { get; set; }

    /// <summary>
    /// 金额
    /// </summary>
    public decimal amount { get; set; }
}