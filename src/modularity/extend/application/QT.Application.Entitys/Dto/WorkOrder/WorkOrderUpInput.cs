using QT.DependencyInjection;

namespace QT.Extend.Entitys.Dto.WorkOrder;

/// <summary>
/// 
/// </summary>
[SuppressSniffer]
public class WorkOrderUpInput : WorkOrderCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string? id { get; set; }
}


public class WorkOrderSendInput
{
    public string id { get; set; }

    public List<string> userList { get; set; }
}