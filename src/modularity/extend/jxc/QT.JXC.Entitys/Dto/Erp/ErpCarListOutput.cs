namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 车辆管理输入参数.
/// </summary>
public class ErpCarListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

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
    /// 车载监控
    /// </summary>
    public string deviceId { get; set; }

}