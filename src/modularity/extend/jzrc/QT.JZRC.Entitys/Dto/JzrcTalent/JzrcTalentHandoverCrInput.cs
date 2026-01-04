using QT.Common.Models;
using QT.JZRC.Entitys.Dto.JzrcTalentCommunication;

namespace QT.JZRC.Entitys.Dto.JzrcTalentHandover;

/// <summary>
/// 人才交接记录修改输入参数.
/// </summary>
public class JzrcTalentHandoverCrInput
{
    /// <summary>
    /// 人才id.
    /// </summary>
    public string talentId { get; set; }

    /// <summary>
    /// 交接时间.
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

/// <summary>
/// 人才沟通记录更新输入.
/// </summary>
public class JzrcTalentHandoverUpInput : JzrcTalentHandoverCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }
}