namespace QT.Iot.Application.Dto.MaintenancePlanOrder;

public class MaintenancePlanOrderListOutput: MaintenancePlanOrderOutput
{
    /// <summary>
    /// 创建人
    /// </summary>
    public string creatorUserIdName { get; set; }

    /// <summary>
    /// 维保人员
    /// </summary>
    public string taskUserIdName { get; set; }



    /// <summary>
    /// 所属项目
    /// </summary>
    public string projectIdName { get; set; }
}
