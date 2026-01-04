using Microsoft.AspNetCore.Mvc;

namespace QT.JXC.Interfaces;

/// <summary>
/// 业务抽象：客户信息.
/// </summary>
public interface IErpCustomerService
{
    Task<bool> Check([FromRoute] string id);
}