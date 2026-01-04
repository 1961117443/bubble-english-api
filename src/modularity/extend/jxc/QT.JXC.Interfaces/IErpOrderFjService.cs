using QT.JXC.Entitys.Dto.Erp.OrderFj;

namespace QT.JXC.Interfaces;

/// <summary>
/// 业务抽象：订单信息.
/// </summary>
public interface IErpOrderFjService
{
    Task<ErpOrderFjSubmitCheckOutput> HandleSubmitCheck(string id);
}