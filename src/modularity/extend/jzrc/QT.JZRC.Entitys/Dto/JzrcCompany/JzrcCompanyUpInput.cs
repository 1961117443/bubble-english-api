using QT.JZRC.Entitys.Dto.JzrcCompanyCommunication;
using QT.JZRC.Entitys.Dto.JzrcCompanyJob;
using QT.JZRC.Entitys.Dto.JzrcCompanyQuality;
using QT.JZRC.Entitys.Dto.JzrcCompanyTalent;
namespace QT.JZRC.Entitys.Dto.JzrcCompany;

/// <summary>
/// 企业信息更新输入.
/// </summary>
public class JzrcCompanyUpInput : JzrcCompanyCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    ///// <summary>
    ///// 企业沟通记录.
    ///// </summary>
    //public new List<JzrcCompanyCommunicationUpInput> jzrcCompanyCommunicationList { get; set; }

    ///// <summary>
    ///// 企业招聘职位.
    ///// </summary>
    //public new List<JzrcCompanyJobUpInput> jzrcCompanyJobList { get; set; }

    /// <summary>
    /// 企业资质信息.
    /// </summary>
    public new List<JzrcCompanyQualityUpInput> jzrcCompanyQualityList { get; set; }

    ///// <summary>
    ///// 企业签约人才.
    ///// </summary>
    //public new List<JzrcCompanyTalentUpInput> jzrcCompanyTalentList { get; set; }

}