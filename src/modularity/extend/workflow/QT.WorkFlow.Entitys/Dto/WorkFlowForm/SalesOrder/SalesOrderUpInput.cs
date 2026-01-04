using QT.DependencyInjection;

namespace QT.WorkFlow.Entitys.Dto.WorkFlowForm.SalesOrder;

[SuppressSniffer]
public class SalesOrderUpInput : SalesOrderCrInput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }
}
