namespace QT.JZRC.Entitys.Dto.JzrcTalentCertificate;

/// <summary>
/// 建筑人才证书信息输入参数.
/// </summary>
public class JzrcTalentCertificateListOutput
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
    /// 人才姓名.
    /// </summary>
    public string talentIdName { get; set; }

    /// <summary>
    /// 证书名称.
    /// </summary>
    public string certificateName { get; set; }

    /// <summary>
    /// 等级.
    /// </summary>
    public string level { get; set; }

    /// <summary>
    /// 获取时间.
    /// </summary>
    public DateTime? acquisitionTime { get; set; }

    /// <summary>
    /// 发证机构.
    /// </summary>
    public string issuingOrganization { get; set; }

    /// <summary>
    /// 有效期限.
    /// </summary>
    public string validityPeriod { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 区域.
    /// </summary>
    public string region { get; set; }

    /// <summary>
    /// 证书分类.
    /// </summary>
    public string categoryId { get; set; }

    /// <summary>
    /// 证书编号.
    /// </summary>
    public string certificateNo { get; set; }

    /// <summary>
    /// 证书分类.
    /// </summary>
    public string categoryIdName { get; set; }

    /// <summary>
    /// 区域.
    /// </summary>
    public string regionName { get; set; }

}