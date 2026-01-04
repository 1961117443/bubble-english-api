using QT.Common.Dtos.VisualDev;
using QT.Systems.Entitys.System;
using System.Data;

namespace QT.Systems.Interfaces.System;

/// <summary>
/// 数据接口



/// 日 期：2021-06-01.
/// </summary>
public interface IDataInterfaceService
{
    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    Task<DataInterfaceEntity> GetInfo(string id);

    /// <summary>
    /// sql接口查询.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<DataTable> GetData(DataInterfaceEntity entity);

    /// <summary>
    /// 根据不同类型请求接口.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type">0 ： 分页 1 ：详情 ，其他 原始.</param>
    /// <param name="tenantId"></param>
    /// <param name="input"></param>
    /// <param name="dicParameters">字典参数.</param>
    /// <returns></returns>
    Task<object> GetResponseByType(string id, int type, string tenantId, VisualDevDataFieldDataListInput input = null, Dictionary<string, string> dicParameters = null);

    /// <summary>
    /// 替换参数默认值.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dic"></param>
    void ReplaceParameterValue(DataInterfaceEntity entity, Dictionary<string, string> dic);
}

public interface IDataInterfaceActuator
{
    /// <summary>
    /// 根据不同类型请求接口.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type">0 ： 分页 1 ：详情 ，其他 原始.</param>
    /// <param name="tenantId"></param>
    /// <param name="input"></param>
    /// <param name="dicParameters">字典参数.</param>
    /// <returns></returns>
    Task<object> GetResponseByType(string id, int type, string tenantId, VisualDevDataFieldDataListInput input = null, Dictionary<string, string> dicParameters = null);

}