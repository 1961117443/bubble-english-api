using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderTrace;

namespace QT.Application.Interfaces.FreshDelivery;

public interface IErpOrderTraceService
{
    /// <summary>
    /// 根据订单明细id，获取订单出库明细
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<List<OriginInrecordInfo>> GetOrderOutList(string id);
}
