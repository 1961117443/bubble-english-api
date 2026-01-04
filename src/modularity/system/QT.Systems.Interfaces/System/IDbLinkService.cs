using QT.Systems.Entitys.Dto.DbLink;
using QT.Systems.Entitys.System;

namespace QT.Systems.Interfaces.System;

/// <summary>
/// 数据连接



/// 日 期：2021-06-01.
/// </summary>
public interface IDbLinkService
{
    /// <summary>
    /// 列表.
    /// </summary>
    /// <returns></returns>
    Task<List<DbLinkListOutput>> GetList();

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    Task<DbLinkEntity> GetInfo(string id);
}