using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.ComFields;
using QT.Systems.Entitys.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Systems;

/// <summary>
/// 常用字段



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "CommonFields", Order = 201)]
[Route("api/system/[controller]")]
public class ComFieldsService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ComFieldsEntity> _repository;

    /// <summary>
    /// 初始化一个<see cref="ComFieldsService"/>类型的新实例.
    /// </summary>
    public ComFieldsService(ISqlSugarRepository<ComFieldsEntity> comFieldsRepository)
    {
        _repository = comFieldsRepository;
    }

    #region Get

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">请求参数.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var data = await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        return data.Adapt<ComFieldsInfoOutput>();
    }

    /// <summary>
    /// 列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList()
    {
        var data = (await _repository.ToListAsync(x => x.DeleteMark == null)).OrderBy(x => x.SortCode).ToList();
        return new { list = data.Adapt<List<ComFieldsListOutput>>() };
    }
    #endregion

    #region Post

    /// <summary>
    /// 新增.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ComFieldsCrInput input)
    {
        if (await _repository.AnyAsync(x => x.Field.ToLower() == input.field.ToLower() && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<ComFieldsEntity>();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 修改.
    /// </summary>
    /// <param name="id">id.</param>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update_Api(string id, [FromBody] ComFieldsUpInput input)
    {
        if (await _repository.AnyAsync(x => x.Id != id && x.Field.ToLower() == input.field.ToLower() && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<ComFieldsEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }
    #endregion
}