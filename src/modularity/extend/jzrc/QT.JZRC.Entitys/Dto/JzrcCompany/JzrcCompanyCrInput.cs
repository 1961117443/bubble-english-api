using QT.JZRC.Entitys.Dto.JzrcCompanyCommunication;
using QT.JZRC.Entitys.Dto.JzrcCompanyJob;
using QT.JZRC.Entitys.Dto.JzrcCompanyQuality;
using QT.JZRC.Entitys.Dto.JzrcCompanyTalent;

namespace QT.JZRC.Entitys.Dto.JzrcCompany;

/// <summary>
/// 企业信息修改输入参数.
/// </summary>
public class JzrcCompanyCrInput
{
    /// <summary>
    /// 企业名称.
    /// </summary>
    public string companyName { get; set; }

    /// <summary>
    /// 企业联系人.
    /// </summary>
    public string contactPerson { get; set; }

    /// <summary>
    /// 企业联系电话.
    /// </summary>
    public string contactPhoneNumber { get; set; }

    /// <summary>
    /// 省.
    /// </summary>
    public string[] province { get; set; }

    /// <summary>
    /// 市.
    /// </summary>
    public string city { get; set; }

    /// <summary>
    /// 区.
    /// </summary>
    public string district { get; set; }

    /// <summary>
    /// 传真.
    /// </summary>
    public string fax { get; set; }

    /// <summary>
    /// 邮编.
    /// </summary>
    public string postalCode { get; set; }

    /// <summary>
    /// 邮箱.
    /// </summary>
    public string email { get; set; }

    /// <summary>
    /// 企业地址.
    /// </summary>
    public string companyAddress { get; set; }

    /// <summary>
    /// 成立时间.
    /// </summary>
    public DateTime? establishmentDate { get; set; }

    /// <summary>
    /// 核准时间.
    /// </summary>
    public DateTime? approvalDate { get; set; }

    /// <summary>
    /// 企业官网.
    /// </summary>
    public string companyWebsite { get; set; }

    /// <summary>
    /// 企业法人.
    /// </summary>
    public string legalRepresentative { get; set; }

    /// <summary>
    /// 注册资金.
    /// </summary>
    public string registeredCapital { get; set; }

    /// <summary>
    /// 统一社会信用代码.
    /// </summary>
    public string unifiedSocialCreditCode { get; set; }

    /// <summary>
    /// 纳税人识别号.
    /// </summary>
    public string taxpayerIdNumber { get; set; }

    /// <summary>
    /// 工商注册地.
    /// </summary>
    public string registrationAddress { get; set; }

    /// <summary>
    /// 经营范围.
    /// </summary>
    public string businessScope { get; set; }

    /// <summary>
    /// 企业信息.
    /// </summary>
    public string information { get; set; }

    /// <summary>
    /// 公司性质.
    /// </summary>
    public string nature { get; set; }

    /// <summary>
    /// 公司人数.
    /// </summary>
    public int? number { get; set; }

    /// <summary>
    /// 登记状态.
    /// </summary>
    public string registrationStatus { get; set; }

    /// <summary>
    /// 注册号.
    /// </summary>
    public string registrationNumber { get; set; }

    /// <summary>
    /// 组织机构代码.
    /// </summary>
    public string organizationCode { get; set; }

    /// <summary>
    /// 参保人数.
    /// </summary>
    public int? insuredNumber { get; set; }

    /// <summary>
    /// 所属行业.
    /// </summary>
    public string industry { get; set; }

    /// <summary>
    /// 专业.
    /// </summary>
    public string major { get; set; }

    /// <summary>
    /// 曾用名.
    /// </summary>
    public string formerName { get; set; }

    /// <summary>
    /// 客户经理id.
    /// </summary>
    public string managerId { get; set; }

    /// <summary>
    /// 是否入驻.
    /// </summary>
    public bool? whetherSettled { get; set; }

    /// <summary>
    /// 入驻时间.
    /// </summary>
    public DateTime? settlementDate { get; set; }

    ///// <summary>
    ///// 企业沟通记录.
    ///// </summary>
    //public List<JzrcCompanyCommunicationCrInput> jzrcCompanyCommunicationList { get; set; }

    ///// <summary>
    ///// 企业招聘职位.
    ///// </summary>
    //public List<JzrcCompanyJobCrInput> jzrcCompanyJobList { get; set; }

    /// <summary>
    /// 企业资质信息.
    /// </summary>
    public List<JzrcCompanyQualityCrInput> jzrcCompanyQualityList { get; set; }

    ///// <summary>
    ///// 企业签约人才.
    ///// </summary>
    //public List<JzrcCompanyTalentCrInput> jzrcCompanyTalentList { get; set; }

}