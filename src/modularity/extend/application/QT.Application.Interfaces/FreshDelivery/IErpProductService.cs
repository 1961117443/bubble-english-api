using Microsoft.AspNetCore.Mvc;
using QT.Application.Entitys.Dto.FreshDelivery.ErpProduct;
using QT.Application.Entitys.Dto.FreshDelivery.ErpProductmodel;

namespace QT.Application.Interfaces.FreshDelivery;

/// <summary>
/// 业务抽象：商品信息.
/// </summary>
public interface IErpProductService
{
    Task<List<QueryProductSalePriceOutput>> QueryProductSalePrice([FromQuery] ErpProductSelectorQueryInput pageInput);
}