using QT.Common.Filter;

namespace QT.Emp.Entitys.Dto.EmpInterviewEmployee;

/// <summary>
/// 面试管理列表查询输入
/// </summary>
public class EmpInterviewEmployeeListQueryInput : PageInputBase
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
    /// 手机号码.
    /// </summary>
    public string mobile { get; set; }

    /// <summary>
    /// 预计面试时间.
    /// </summary>
    public string interviewTime { get; set; }

    /// <summary>
    /// 类型 1: 已面试的记录，0：面试待处理
    /// </summary>
    public int type { get; set; }
}