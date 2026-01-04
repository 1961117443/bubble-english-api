
namespace QT.JZRC.Entitys.Dto.JzrcCompanyTalent;

/// <summary>
/// 企业签约人才修改输入参数.
/// </summary>
public class JzrcCompanyTalentCrInput
{
    /// <summary>
    /// 企业id.
    /// </summary>
    public string companyId { get; set; }

    /// <summary>
    /// 人才id.
    /// </summary>
    public string talentId { get; set; }

    /// <summary>
    /// 签约时间.
    /// </summary>
    public DateTime? signingDate { get; set; }

    /// <summary>
    /// 解约时间.
    /// </summary>
    public DateTime? terminationDate { get; set; }

}