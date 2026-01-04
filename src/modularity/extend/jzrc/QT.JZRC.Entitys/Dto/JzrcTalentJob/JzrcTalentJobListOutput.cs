namespace QT.JZRC.Entitys.Dto.JzrcTalentJob;

/// <summary>
/// 建筑人才求职信息输入参数.
/// </summary>
public class JzrcTalentJobListOutput
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

    /// <summary>
    /// 人才id.
    /// </summary>
    public string talentIdName { get; set; }

    /// <summary>
    /// 招聘地区.
    /// </summary>
    public string regionName { get; set; }

    /// <summary>
    /// 证书名称.
    /// </summary>
    public string certificateIdName
    {
        get; set;
    }
    /// <summary>
    /// 证书类型id.
    /// </summary>
    public string certificateCategoryId { get; set; }

    /// <summary>
    /// 证书等级.
    /// </summary>
    public string level { get; set; }


    /// <summary>
    /// 状态.
    /// </summary>
    public int status { get; set; }
}