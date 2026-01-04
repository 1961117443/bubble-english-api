namespace QT.CMS.Entitys.Dto.Parameter;

/// <summary>
/// 会员查询参数
/// </summary>
public class MemberParameter : BaseParameter
{
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}
