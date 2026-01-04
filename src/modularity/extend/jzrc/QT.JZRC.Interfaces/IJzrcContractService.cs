using QT.JZRC.Entitys;

namespace QT.JZRC.Interfaces;

/// <summary>
/// 业务抽象：建筑人才合同管理.
/// </summary>
public interface IJzrcContractService
{
    Task SetProperty(JzrcContractEntity entity);
}