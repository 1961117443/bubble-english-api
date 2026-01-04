namespace QT.JZRC.Entitys.Dto.JzrcCompanyJob;

/// <summary>
/// 企业招聘输入参数.
/// </summary>
public class JzrcCompanyJobListOutput
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
    /// 状态.
    /// </summary>
    public int status { get; set; }

    /// <summary>
    /// 申请人数
    /// </summary>
    public int postNum { get; set; }

    /// <summary>
    /// 签约人数
    /// </summary>
    public int signNum { get; set; }
}