using Microsoft.AspNetCore.Mvc;
using QT.Logistics.Entitys.Dto.LogOrder;

namespace QT.Logistics.Interfaces;

/// <summary>
/// 业务抽象：订单管理.
/// </summary>
public interface ILogOrderService
{
    Task<dynamic> GetList([FromQuery] LogOrderListQueryInput input);
}