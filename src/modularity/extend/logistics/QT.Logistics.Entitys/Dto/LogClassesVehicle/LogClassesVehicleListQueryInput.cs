using QT.Common.Filter;

namespace QT.Logistics.Entitys.Dto.LogClassesVehicle;

/// <summary>
/// 发车记录列表查询输入
/// </summary>
public class LogClassesVehicleListQueryInput : PageInputBase
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
    /// 班次ID.
    /// </summary>
    public string cId { get; set; }

    /// <summary>
    /// 车辆ID.
    /// </summary>
    public string vId { get; set; }

    /// <summary>
    /// 调度人.
    /// </summary>
    public string dispatcher { get; set; }

    /// <summary>
    /// 班次编号.
    /// </summary>
    public string code { get; set; }

}