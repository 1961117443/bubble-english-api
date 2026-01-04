using QT.JXC.Entitys.Dto.Erp;

namespace QT.JXC.Interfaces;

/// <summary>
/// 业务抽象：采购任务订单.
/// </summary>
public interface IErpBuyorderService
{
    Task UpdateOrderDetailByBuyorderdetailId(string id, UpdateStoreInfoInput input);
}