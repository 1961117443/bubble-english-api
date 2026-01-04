using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.JZRC.Entitys;

/// <summary>
/// 企业信息实体.
/// </summary>
[SugarTable("jzrc_company")]
[Tenant(ClaimConst.TENANTID)]
public class JzrcCompanyEntity :CUDEntityBase
{
    /// <summary>
    /// 企业名称.
    /// </summary>
    [SugarColumn(ColumnName = "CompanyName")]
    public string CompanyName { get; set; }

    /// <summary>
    /// 企业联系人.
    /// </summary>
    [SugarColumn(ColumnName = "ContactPerson")]
    public string ContactPerson { get; set; }

    /// <summary>
    /// 企业联系电话.
    /// </summary>
    [SugarColumn(ColumnName = "ContactPhoneNumber")]
    public string ContactPhoneNumber { get; set; }

    /// <summary>
    /// 省.
    /// </summary>
    [SugarColumn(ColumnName = "Province")]
    public string Province { get; set; }

    /// <summary>
    /// 市.
    /// </summary>
    [SugarColumn(ColumnName = "City")]
    public string City { get; set; }

    /// <summary>
    /// 区.
    /// </summary>
    [SugarColumn(ColumnName = "District")]
    public string District { get; set; }

    /// <summary>
    /// 传真.
    /// </summary>
    [SugarColumn(ColumnName = "Fax")]
    public string Fax { get; set; }

    /// <summary>
    /// 邮编.
    /// </summary>
    [SugarColumn(ColumnName = "PostalCode")]
    public string PostalCode { get; set; }

    /// <summary>
    /// 邮箱.
    /// </summary>
    [SugarColumn(ColumnName = "Email")]
    public string Email { get; set; }

    /// <summary>
    /// 企业地址.
    /// </summary>
    [SugarColumn(ColumnName = "CompanyAddress")]
    public string CompanyAddress { get; set; }

    /// <summary>
    /// 成立时间.
    /// </summary>
    [SugarColumn(ColumnName = "EstablishmentDate")]
    public DateTime? EstablishmentDate { get; set; }

    /// <summary>
    /// 核准时间.
    /// </summary>
    [SugarColumn(ColumnName = "ApprovalDate")]
    public DateTime? ApprovalDate { get; set; }

    /// <summary>
    /// 企业官网.
    /// </summary>
    [SugarColumn(ColumnName = "CompanyWebsite")]
    public string CompanyWebsite { get; set; }

    /// <summary>
    /// 企业法人.
    /// </summary>
    [SugarColumn(ColumnName = "LegalRepresentative")]
    public string LegalRepresentative { get; set; }

    /// <summary>
    /// 注册资金.
    /// </summary>
    [SugarColumn(ColumnName = "RegisteredCapital")]
    public string RegisteredCapital { get; set; }

    /// <summary>
    /// 统一社会信用代码.
    /// </summary>
    [SugarColumn(ColumnName = "UnifiedSocialCreditCode")]
    public string UnifiedSocialCreditCode { get; set; }

    /// <summary>
    /// 纳税人识别号.
    /// </summary>
    [SugarColumn(ColumnName = "TaxpayerIdNumber")]
    public string TaxpayerIdNumber { get; set; }

    /// <summary>
    /// 工商注册地.
    /// </summary>
    [SugarColumn(ColumnName = "RegistrationAddress")]
    public string RegistrationAddress { get; set; }

    /// <summary>
    /// 经营范围.
    /// </summary>
    [SugarColumn(ColumnName = "BusinessScope")]
    public string BusinessScope { get; set; }

    /// <summary>
    /// 企业信息.
    /// </summary>
    [SugarColumn(ColumnName = "Information")]
    public string Information { get; set; }

    /// <summary>
    /// 公司性质.
    /// </summary>
    [SugarColumn(ColumnName = "Nature")]
    public string Nature { get; set; }

    /// <summary>
    /// 公司人数.
    /// </summary>
    [SugarColumn(ColumnName = "Number")]
    public int? Number { get; set; }

    /// <summary>
    /// 登记状态.
    /// </summary>
    [SugarColumn(ColumnName = "RegistrationStatus")]
    public string RegistrationStatus { get; set; }

    /// <summary>
    /// 注册号.
    /// </summary>
    [SugarColumn(ColumnName = "RegistrationNumber")]
    public string RegistrationNumber { get; set; }

    /// <summary>
    /// 组织机构代码.
    /// </summary>
    [SugarColumn(ColumnName = "OrganizationCode")]
    public string OrganizationCode { get; set; }

    /// <summary>
    /// 参保人数.
    /// </summary>
    [SugarColumn(ColumnName = "InsuredNumber")]
    public int? InsuredNumber { get; set; }

    /// <summary>
    /// 所属行业.
    /// </summary>
    [SugarColumn(ColumnName = "Industry")]
    public string Industry { get; set; }

    /// <summary>
    /// 专业.
    /// </summary>
    [SugarColumn(ColumnName = "Major")]
    public string Major { get; set; }

    /// <summary>
    /// 曾用名.
    /// </summary>
    [SugarColumn(ColumnName = "FormerName")]
    public string FormerName { get; set; }

    /// <summary>
    /// 客户经理id.
    /// </summary>
    [SugarColumn(ColumnName = "ManagerId")]
    public string ManagerId { get; set; }

    /// <summary>
    /// 是否入驻.
    /// </summary>
    [SugarColumn(ColumnName = "WhetherSettled")]
    public int? WhetherSettled { get; set; }

    /// <summary>
    /// 入驻时间.
    /// </summary>
    [SugarColumn(ColumnName = "SettlementDate")]
    public DateTime? SettlementDate { get; set; }

}