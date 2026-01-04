using QT.Application.Entitys.Dto.FreshDelivery.ErpInorder;

namespace QT.Application.Interfaces.FreshDelivery;

/// <summary>
/// 业务抽象：采购任务订单.
/// </summary>
public interface IErpBuyorderService
{
    Task UpdateOrderDetailByBuyorderdetailId(string id, UpdateStoreInfoInput input);
}