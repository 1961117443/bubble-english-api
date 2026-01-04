
using Microsoft.AspNetCore.Mvc;
using QT.JXC.Entitys.Dto.Erp;

namespace QT.JXC.Interfaces;

/// <summary>
/// 业务抽象：出库订单表.
/// </summary>
public interface IErpOutorderService
{
    /// <summary>
    /// inid: 入库id
    /// </summary>
    /// <param name="inid"></param>
    /// <returns></returns>
    Task CheckTsRecord(string inid);
    Task<string> Create([FromBody] ErpOutorderCrInput input);
    Task Delete(string id);
}