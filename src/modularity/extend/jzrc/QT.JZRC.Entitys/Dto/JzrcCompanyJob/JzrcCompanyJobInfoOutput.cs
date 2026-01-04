
namespace QT.JZRC.Entitys.Dto.JzrcCompanyJob;

/// <summary>
/// 企业招聘输出参数.
/// </summary>
public class JzrcCompanyJobInfoOutput
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
    /// 职位名称.
    /// </summary>
    public string jobTitle { get; set; }

    /// <summary>
    /// 人才类型.
    /// </summary>
    public string candidateType { get; set; }

    /// <summary>
    /// 招聘数量.
    /// </summary>
    public int? number { get; set; }

    /// <summary>
    /// 职位薪资.
    /// </summary>
    public decimal jobSalary { get; set; }

    /// <summary>
    /// 需求时间起.
    /// </summary>
    public DateTime? requiredStart { get; set; }

    /// <summary>
    /// 需求时间止.
    /// </summary>
    public DateTime? requiredEnd { get; set; }

    /// <summary>
    /// 招聘地区.
    /// </summary>
    public List<string> region { get; set; }

    /// <summary>
    /// 证书类型.
    /// </summary>
    public string certificateCategoryId { get; set; }

    /// <summary>
    /// 证书等级.
    /// </summary>
    public string certificateLevel { get; set; }

    /// <summary>
    /// 社保情况.
    /// </summary>
    public string socialSecurityStatus { get; set; }

    /// <summary>
    /// 业绩情况.
    /// </summary>
    public string performanceSituation { get; set; }

    /// <summary>
    /// 一口价.
    /// </summary>
    public decimal price { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    public int? status { get; set; }

    /// <summary>
    /// 报名人才
    /// </summary>
    public List<JzrcCompanyJobTalentInfo> jzrcCompanyJobTalents { get; set; }
}

public class JzrcCompanyJobTalentInfo
{
    /// <summary>
    /// 人才
    /// </summary>
    public string talentIdName { get; set; }

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