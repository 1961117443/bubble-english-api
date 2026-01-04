using QT.Common.Models;

namespace QT.SDMS.Entitys.Dto.CustomerCommunication;

/// <summary>
/// 体验用户沟通记录输出参数.
/// </summary>
public class CustomerCommunicationInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 客户id.
    /// </summary>
    public string customerId { get; set; }

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
    public int? whetherContent { get; set; }

    /// <summary>
    /// 沟通内容.
    /// </summary>
    public string content { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    public List<FileControlsModel> attachment { get; set; }

    /// <summary>
    /// 客户经理.
    /// </summary>
    public string managerIdName { get; set; }
}