
using Microsoft.AspNetCore.Mvc;
using QT.Application.Entitys.Dto.FreshDelivery.ErpInorder;
using QT.Application.Entitys.Dto.FreshDelivery.ErpInrecord;

namespace QT.Application.Interfaces.FreshDelivery;

/// <summary>
/// 业务抽象：入库订单表.
/// </summary>
public interface IErpInorderService
{
    Task Create([FromBody] ErpInorderCrInput input);
    Task Delete(string id);

    /// <summary>
    /// 根据规格获取库存
    /// </summary>
    /// <param name="gid"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<List<ErpStoreInfoOutput>> GetStore([FromRoute] string gid, [FromQuery] ErpStoreListQueryInput input);
}