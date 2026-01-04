using Microsoft.AspNetCore.Mvc;
using QT.JXC.Entitys.Dto.Erp;

namespace QT.JXC.Interfaces;

/// <summary>
/// 业务抽象：商品信息.
/// </summary>
public interface IErpProductService
{
    Task<List<QueryProductSalePriceOutput>> QueryProductSalePrice([FromQuery] ErpProductSelectorQueryInput pageInput);
}