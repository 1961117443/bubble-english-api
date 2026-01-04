using QT.Common.Filter;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpCarService;

/// <summary>
/// 车辆管理列表查询输入
/// </summary>
public class ErpCarListQueryInput : PageInputBase
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
    /// 车牌号码.
    /// </summary>
    public string carNo { get; set; }

    /// <summary>
    /// 驾驶员.
    /// </summary>
    public string driver { get; set; }

    /// <summary>
    /// 驾驶员手机.
    /// </summary>
    public string phone { get; set; }

    /// <summary>
    /// 运输状态.
    /// </summary>
    public string status { get; set; }


    /// <summary>
    /// 车载监控id
    /// 100=已绑定
    /// 101=未绑定
    /// 其他=设备id
    /// </summary>
    public string deviceId { get; set; }
}