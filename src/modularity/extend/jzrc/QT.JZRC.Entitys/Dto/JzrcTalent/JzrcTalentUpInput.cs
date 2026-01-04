using QT.JZRC.Entitys.Dto.JzrcTalentCertificate;
using QT.JZRC.Entitys.Dto.JzrcTalentCommunication;
using QT.JZRC.Entitys.Dto.JzrcTalentHandover;

namespace QT.JZRC.Entitys.Dto.JzrcTalent;

/// <summary>
/// 人才信息更新输入.
/// </summary>
public class JzrcTalentUpInput : JzrcTalentCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }
    /// <summary>
    /// 人才证书.
    /// </summary>
    public new List<JzrcTalentCertificateUpInput> jzrcTalentCertificateList { get; set; }

    /// <summary>
    /// 人才沟通记录.
    /// </summary>
    public new List<JzrcTalentCommunicationUpInput> jzrcTalentCommunicationList { get; set; }


    /// <summary>
    /// 人才交接记录.
    /// </summary>
    public new List<JzrcTalentHandoverUpInput> jzrcTalentHandoverList { get; set; }

}