namespace QT.JZRC.Entitys.Dto.JzrcTalentDemand;

/// <summary>
/// 建筑人才需求输入参数.
/// </summary>
public class JzrcTalentDemandListOutput
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
    /// 需求内容.
    /// </summary>
    public string content { get; set; }


    /// <summary>
    /// 人才姓名.
    /// </summary>
    public string talentIdName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? creatorTime { get; set; }

}