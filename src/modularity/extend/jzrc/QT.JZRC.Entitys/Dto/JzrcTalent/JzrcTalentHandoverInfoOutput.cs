using QT.Common.Models;

namespace QT.JZRC.Entitys.Dto.JzrcTalentCommunication;

/// <summary>
/// 人才交接记录输出参数.
/// </summary>
public class JzrcTalentHandoverInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 人才id.
    /// </summary>
    public string talentId { get; set; }

    /// <summary>
    /// 沟通时间.
    /// </summary>
    public DateTime? handoverTime { get; set; }

    /// <summary>
    /// 沟通内容.
    /// </summary>
    public string content { get; set; }

    /// <summary>
    /// 附件.
    /// </summary>
    public List<FileControlsModel> attachment { get; set; }

}