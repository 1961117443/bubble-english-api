using QT.JZRC.Entitys.Dto.JzrcTalentCertificate;
using QT.JZRC.Entitys.Dto.JzrcTalentCommunication;
using QT.JZRC.Entitys.Dto.JzrcTalentHandover;
using SqlSugar;

namespace QT.JZRC.Entitys.Dto.JzrcTalent;

/// <summary>
/// 人才信息修改输入参数.
/// </summary>
public class JzrcTalentCrInput
{
    /// <summary>
    /// 姓名.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 身份证.
    /// </summary>
    public string idCard { get; set; }

    /// <summary>
    /// 性别.
    /// </summary>
    public string gender { get; set; }

    /// <summary>
    /// 手机.
    /// </summary>
    public string mobilePhone { get; set; }

    /// <summary>
    /// 区域.
    /// </summary>
    public string[] region { get; set; }

    /// <summary>
    /// 注册类别.
    /// </summary>
    public string registrationCategory { get; set; }

    /// <summary>
    /// 专业1.
    /// </summary>
    public string major1 { get; set; }

    /// <summary>
    /// 专业2.
    /// </summary>
    public string major2 { get; set; }

    /// <summary>
    /// 专业3.
    /// </summary>
    public string major3 { get; set; }

    /// <summary>
    /// 专业4.
    /// </summary>
    public string major4 { get; set; }

    /// <summary>
    /// 专业5.
    /// </summary>
    public string major5 { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 客户经理id.
    /// </summary>
    public string managerId { get; set; }

    ///// <summary>
    ///// 是否入驻.
    ///// </summary>
    //public bool whetherSettled { get; set; }

    ///// <summary>
    ///// 入驻时间.
    ///// </summary>
    //public DateTime? settlementDate { get; set; }

    /// <summary>
    /// 社保情况.
    /// </summary>
    public string socialSecurityStatus { get; set; }

    /// <summary>
    /// 业绩情况.
    /// </summary>
    public string performanceSituation { get; set; }

    /// <summary>
    /// 薪资要求.
    /// </summary>
    public string salaryRequirement { get; set; }

    /// <summary>
    /// 人才证书.
    /// </summary>
    public List<JzrcTalentCertificateCrInput> jzrcTalentCertificateList { get; set; }

    /// <summary>
    /// 人才沟通记录.
    /// </summary>
    public List<JzrcTalentCommunicationCrInput> jzrcTalentCommunicationList { get; set; }


    /// <summary>
    /// 人才交接记录.
    /// </summary>
    public List<JzrcTalentHandoverCrInput> jzrcTalentHandoverList { get; set; }

    /// <summary>
    /// 学历
    /// </summary>
    public string education { get; set; }

    /// <summary>
    /// 业绩数量
    /// </summary>
    public int? performanceNum { get; set; }
}