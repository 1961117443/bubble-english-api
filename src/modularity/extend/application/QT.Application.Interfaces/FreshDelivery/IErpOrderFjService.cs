using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderFj;

namespace QT.Application.Interfaces.FreshDelivery;

/// <summary>
/// 业务抽象：订单信息.
/// </summary>
public interface IErpOrderFjService
{
    Task<ErpOrderFjSubmitCheckOutput> HandleSubmitCheck(string id);
}