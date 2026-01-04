using QT.JXC.Entitys.Entity.ERP;
using QT.JXC.Entitys.Enums;

namespace QT.JXC.Interfaces;

/// <summary>
/// 业务抽象：订单信息.
/// </summary>
public interface IErpOrderService
{
    /// <summary>
    /// 提交订单
    /// </summary>
    /// <param name="id"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    Task ProcessOrder(string id, OrderStateEnum state);

    /// <summary>
    /// 判断order是否为子单，并且判断所有子单是否为已收货
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    Task ProcessReceivingSubOrder(ErpOrderEntity order);
}