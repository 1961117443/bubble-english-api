using QT.Common.Filter;

namespace QT.Emp.Entitys.Dto.EmpProbationEmployee;

/// <summary>
/// 转正管理列表查询输入
/// </summary>
public class EmpProbationEmployeeListQueryInput : PageInputBase
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
    /// 转正日期.
    /// </summary>
    public string regularTime { get; set; }

    ///// <summary>
    ///// 计划转正日期.
    ///// </summary>
    //public string planRegularTime { get; set; }

    ///// <summary>
    ///// 类型 1: 已离职的记录，0：离职待处理
    ///// </summary>
    //public int type { get; set; }

}