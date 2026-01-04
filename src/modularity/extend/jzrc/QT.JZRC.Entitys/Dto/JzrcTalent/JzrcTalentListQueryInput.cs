using QT.Common.Filter;

namespace QT.JZRC.Entitys.Dto.JzrcTalent;

/// <summary>
/// 人才信息列表查询输入
/// </summary>
public class JzrcTalentListQueryInput : PageInputBase
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
    /// 姓名.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 区域.
    /// </summary>
    public string region { get; set; }

    /// <summary>
    /// 手机号码.
    /// </summary>
    public string mobilePhone { get; set; }


    /// <summary>
    /// 客户经理.
    /// </summary>
    public string managerId { get; set; }

    /// <summary>
    /// 是否入驻
    /// </summary>
    public int? signed { get; set; }

}