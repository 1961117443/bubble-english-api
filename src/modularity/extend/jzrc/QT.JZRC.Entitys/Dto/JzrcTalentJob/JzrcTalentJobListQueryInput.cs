using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcTalentJob;

/// <summary>
/// 建筑人才求职信息列表查询输入
/// </summary>
public class JzrcTalentJobListQueryInput : PageInputBase
{
    /// <summary>
    /// 选择导出数据key.
    /// </summary>
    public string selectKey { get; set; }

    /// <summary>
    /// 导出类型.
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// 招聘地区.
    /// </summary>
    public string region { get; set; }

    /// <summary>
    /// 求职类型.
    /// </summary>
    public string candidateType { get; set; }

    /// <summary>
    /// 求职时间起.
    /// </summary>
    public string requiredStart { get; set; }

    /// <summary>
    /// 求职时间止.
    /// </summary>
    public string requiredEnd { get; set; }

}