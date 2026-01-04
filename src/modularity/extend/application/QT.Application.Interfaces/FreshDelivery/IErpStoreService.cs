using QT.Application.Entitys.Dto.FreshDelivery.ErpOutrecord;

namespace QT.Application.Interfaces.FreshDelivery;

public interface IErpStoreService
{
    /// <summary>
    /// 减少库存
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<ErpOutdetailRecordUpOutput> Reduce(ErpOutdetailRecordUpInput input);

    /// <summary>
    /// 恢复库存
    /// </summary>
    /// <param name="id">出库明细id</param>
    /// <returns></returns>
    Task Restore(string id);
}