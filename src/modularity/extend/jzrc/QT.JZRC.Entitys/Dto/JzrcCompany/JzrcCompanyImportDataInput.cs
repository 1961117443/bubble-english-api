using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace QT.JZRC.Entitys.Dto.JzrcCompany;

/// <summary>
/// 企业信息导入模型.
/// </summary>
public class JzrcCompanyImportDataTemplate
{
    /// <summary>
    /// 企业名称.
    /// </summary>
    [Display(Name = "企业名称")]
    public string companyName { get; set; }

    /// <summary>
    /// 企业联系人.
    /// </summary>
    [Display(Name = "企业联系人")]
    public string contactPerson { get; set; }

    /// <summary>
    /// 企业联系电话.
    /// </summary>
    [Display(Name = "企业联系电话")]
    public string contactPhoneNumber { get; set; }

    /// <summary>
    /// 省.
    /// </summary>
    [Display(Name = "省")]
    public string province { get; set; }

    /// <summary>
    /// 市.
    /// </summary>
    [Display(Name = "市")]
    public string city { get; set; }

    /// <summary>
    /// 区.
    /// </summary>
    [Display(Name = "区")]
    public string district { get; set; }

    /// <summary>
    /// 传真.
    /// </summary>
    [Display(Name = "传真")]
    public string fax { get; set; }

    /// <summary>
    /// 邮编.
    /// </summary>
    [Display(Name = "邮编")]
    public string postalCode { get; set; }

    /// <summary>
    /// 邮箱.
    /// </summary>
    [Display(Name = "邮箱")]
    public string email { get; set; }

    /// <summary>
    /// 企业地址.
    /// </summary>
    [Display(Name = "企业地址")]
    public string companyAddress { get; set; }

    /// <summary>
    /// 成立时间.
    /// </summary>
    [Display(Name = "成立时间")]
    public string establishmentDate { get; set; }

    /// <summary>
    /// 核准时间.
    /// </summary>
    [Display(Name = "核准时间")]
    public string? approvalDate { get; set; }

    /// <summary>
    /// 企业官网.
    /// </summary>
    [Display(Name = "企业官网")]
    public string? companyWebsite { get; set; }

    /// <summary>
    /// 企业法人.
    /// </summary>
    [Display(Name = "企业法人")]
    public string legalRepresentative { get; set; }

    /// <summary>
    /// 注册资金.
    /// </summary>
    [Display(Name = "注册资金")]
    public string registeredCapital { get; set; }

    /// <summary>
    /// 统一社会信用代码.
    /// </summary>
    [Display(Name = "统一社会信用代码")]
    public string unifiedSocialCreditCode { get; set; }

    /// <summary>
    /// 纳税人识别号.
    /// </summary>
    [Display(Name = "纳税人识别号")]
    public string taxpayerIdNumber { get; set; }

    /// <summary>
    /// 工商注册地.
    /// </summary>
    [Display(Name = "工商注册地")]
    public string registrationAddress { get; set; }

    /// <summary>
    /// 经营范围.
    /// </summary>
    [Display(Name = "经营范围")]
    public string businessScope { get; set; }

    /// <summary>
    /// 企业信息.
    /// </summary>
    [Display(Name = "企业信息")]
    public string information { get; set; }

    /// <summary>
    /// 公司性质.
    /// </summary>
    [Display(Name = "公司性质")]
    public string nature { get; set; }

    /// <summary>
    /// 公司人数.
    /// </summary>
    [Display(Name = "公司人数")]
    public int? number { get; set; }

    /// <summary>
    /// 登记状态.
    /// </summary>
    [Display(Name = "登记状态")]
    public string registrationStatus { get; set; }

    /// <summary>
    /// 注册号.
    /// </summary>
    [Display(Name = "注册号")]
    public string registrationNumber { get; set; }

    /// <summary>
    /// 组织机构代码.
    /// </summary>
    [Display(Name = "组织机构代码")]
    public string organizationCode { get; set; }

    /// <summary>
    /// 参保人数.
    /// </summary>
    [Display(Name = "参保人数")]
    public int? insuredNumber { get; set; }

    /// <summary>
    /// 所属行业.
    /// </summary>
    [Display(Name = "所属行业")]
    public string industry { get; set; }

    /// <summary>
    /// 专业.
    /// </summary>
    [Display(Name = "专业")]
    public string major { get; set; }

    /// <summary>
    /// 曾用名.
    /// </summary>
    [Display(Name = "曾用名")]
    public string formerName { get; set; }

}


public class JzrcCompanyImportDataInput : JzrcCompanyImportDataTemplate
{
    /// <summary>
    /// 主键.
    /// </summary>
    [Display(Name = "主键")]
    public string id { get; set; }
}