using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Interfaces;

public interface IErpConfigService
{
    /// <summary>
    /// 验证客户下单时间
    /// </summary>
    /// <returns></returns>
    Task ValidCustomerOrder();
}
