namespace QT.Iot.Application.Dto.MaintenanceOrder;

public class MaintenanceOrderListOutput: MaintenanceOrderOutput
{
    /// <summary>
    /// 创建人
    /// </summary>
    public string creatorUserIdName { get; set; }

    /// <summary>
    /// 所属项目
    /// </summary>
    public string projectIdName { get; set; }
}
