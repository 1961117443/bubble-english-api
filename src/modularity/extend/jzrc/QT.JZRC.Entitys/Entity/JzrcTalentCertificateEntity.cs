using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 建筑人才证书信息实体.
/// </summary>
[SugarTable("jzrc_talent_certificate")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcTalentCertificateEntity :CUDEntityBase
{
    /// <summary>
    /// 人才id.
    /// </summary>
    [SugarColumn(ColumnName = "TalentId")]
    public string TalentId { get; set; }

    /// <summary>
    /// 证书名称.
    /// </summary>
    [SugarColumn(ColumnName = "CertificateName")]
    public string CertificateName { get; set; }

    /// <summary>
    /// 等级.
    /// </summary>
    [SugarColumn(ColumnName = "Level")]
    public string Level { get; set; }

    /// <summary>
    /// 获取时间.
    /// </summary>
    [SugarColumn(ColumnName = "AcquisitionTime")]
    public DateTime? AcquisitionTime { get; set; }

    /// <summary>
    /// 发证机构.
    /// </summary>
    [SugarColumn(ColumnName = "IssuingOrganization")]
    public string IssuingOrganization { get; set; }

    /// <summary>
    /// 有效期限.
    /// </summary>
    [SugarColumn(ColumnName = "ValidityPeriod")]
    public string ValidityPeriod { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    [SugarColumn(ColumnName = "Attachment")]
    public string Attachment { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }

    /// <summary>
    /// 区域.
    /// </summary>
    [SugarColumn(ColumnName = "Region")]
    public string Region { get; set; }

    /// <summary>
    /// 扩展属性.
    /// </summary>
    [SugarColumn(ColumnName = "PropertyJson")]
    public string PropertyJson { get; set; }

    /// <summary>
    /// 证书分类.
    /// </summary>
    [SugarColumn(ColumnName = "CategoryId")]
    public string CategoryId { get; set; }

    /// <summary>
    /// 证书编号.
    /// </summary>
    [SugarColumn(ColumnName = "CertificateNo")]
    public string CertificateNo { get; set; }

}