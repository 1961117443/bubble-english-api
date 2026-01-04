using QT.Common.Models;

namespace QT.JZRC.Entitys.Dto.JzrcCompanyCommunication;

/// <summary>
/// 企业沟通记录修改输入参数.
/// </summary>
public class JzrcCompanyCommunicationCrInput
{
    /// <summary>
    /// 企业id.
    /// </summary>
    public string companyId { get; set; }

    /// <summary>
    /// 客户经理id.
    /// </summary>
    public string managerId { get; set; }

    /// <summary>
    /// 沟通时间.
    /// </summary>
    public DateTime? communicationTime { get; set; }

    /// <summary>
    /// 是否接通.
    /// </summary>
    public bool whetherContent { get; set; }

    /// <summary>
    /// 沟通内容.
    /// </summary>
    public string content { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    public List<FileControlsModel> attachment { get; set; }

}