using SqlSugar;

namespace QT.JZRC.Entitys.Dto.JzrcCompanyJob;

/// <summary>
/// 企业招聘输入参数.
/// </summary>
public class ClientJzrcCompanyJobListOutput
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
    /// 企业名称.
    /// </summary>
    public string companyIdName { get; set; }

    /// <summary>
    /// 职位名称.
    /// </summary>
    public string jobTitle { get; set; }

    /// <summary>
    /// 职位类型.
    /// </summary>
    public string candidateType { get; set; }

    /// <summary>
    /// 招聘数量.
    /// </summary>
    public int? number { get; set; }

    /// <summary>
    /// 薪资待遇.
    /// </summary>
    public int jobSalary { get; set; }


    /// <summary>
    /// 一口价.
    /// </summary>
    public int price { get; set; }
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
    public string region { get; set; }

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
    /// 当前投递人数
    /// </summary>
    public int num { get; set; }
}