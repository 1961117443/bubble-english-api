namespace QT.CMS.Entitys.Dto.Parameter;

/// <summary>
/// 报表查询参数
/// </summary>
public class ReportParameter : BaseParameter
{
    /// <summary>
    /// 显示数量
    /// </summary>
    public int? Top { get; set; }
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}
