namespace QT.JZRC.Entitys.Dto.JzrcCompany;

/// <summary>
/// 企业信息输入参数.
/// </summary>
public class JzrcCompanyListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

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
    /// 成立时间.
    /// </summary>
    public DateTime? establishmentDate { get; set; }

    /// <summary>
    /// 注册资金.
    /// </summary>
    public string registeredCapital { get; set; }

    /// <summary>
    /// 统一社会信用代码.
    /// </summary>
    public string unifiedSocialCreditCode { get; set; }

    /// <summary>
    /// 公司人数.
    /// </summary>
    public int? number { get; set; }

    /// <summary>
    /// 所属行业.
    /// </summary>
    public string industry { get; set; }

    /// <summary>
    /// 专业.
    /// </summary>
    public string major { get; set; }

    /// <summary>
    /// 入驻时间.
    /// </summary>
    public DateTime? settlementDate { get; set; }

    /// <summary>
    /// 客户经理
    /// </summary>
    public string managerIdName { get; set; }

    /// <summary>
    /// 是否签约入驻
    /// </summary>
    public bool signed { get; set; }

    /// <summary>
    /// 区域
    /// </summary>
    public string region { get; set; }
}