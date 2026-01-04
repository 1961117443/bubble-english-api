using QT.Logistics.Entitys;

namespace QT.Logistics.Interfaces;

/// <summary>
/// 业务抽象：配送点管理.
/// </summary>
public interface ILogDeliveryPointService
{
    /// <summary>
    /// 获取当前用户绑定的配送点
    /// </summary>
    /// <returns></returns>
    Task<LogDeliveryPointEntity> GetCurrentUserPoint();
}