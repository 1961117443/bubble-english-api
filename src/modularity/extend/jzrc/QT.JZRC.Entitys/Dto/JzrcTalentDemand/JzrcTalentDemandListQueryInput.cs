using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcTalentDemand;

/// <summary>
/// 建筑人才需求列表查询输入
/// </summary>
public class JzrcTalentDemandListQueryInput : PageInputBase
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
    /// 需求内容.
    /// </summary>
    public string content { get; set; }

}