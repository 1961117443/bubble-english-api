
using Microsoft.AspNetCore.Mvc;
using QT.Application.Entitys.Dto.FreshDelivery.ErpOutorder;

namespace QT.Application.Interfaces.FreshDelivery;

/// <summary>
/// 业务抽象：出库订单表.
/// </summary>
public interface IErpOutorderService
{
    Task Create([FromBody] ErpOutorderCrInput input);
    Task Delete(string id);
}