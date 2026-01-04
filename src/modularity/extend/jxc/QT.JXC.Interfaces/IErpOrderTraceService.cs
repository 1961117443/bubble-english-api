using QT.JXC.Entitys.Dto.ErpOrderTrace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Interfaces;

public interface IErpOrderTraceService
{
    /// <summary>
    /// 根据订单明细id，获取订单出库明细
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<List<OriginInrecordInfo>> GetOrderOutList(string id);
}
