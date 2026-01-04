using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogAdministrativeDivision;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Logistics;

/// <summary>
/// 业务实现：区域管理.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "区域管理", Name = "LogAdministrativeDivision", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogAdministrativeDivisionService : ILogAdministrativeDivisionService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogAdministrativeDivisionEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogAdministrativeDivisionService"/>类型的新实例.
    /// </summary>
    public LogAdministrativeDivisionService(
        ISqlSugarRepository<LogAdministrativeDivisionEntity> logAdministrativeDivisionRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logAdministrativeDivisionRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取区域管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogAdministrativeDivisionInfoOutput>();
    }

    /// <summary>
    /// 获取区域管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogAdministrativeDivisionListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogAdministrativeDivisionEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogAdministrativeDivisionListOutput
            {
                id = it.Id,
                name = it.Name,
                description = it.Description,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogAdministrativeDivisionListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建区域管理.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogAdministrativeDivisionCrInput input)
    {
        var entity = input.Adapt<LogAdministrativeDivisionEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新区域管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogAdministrativeDivisionUpInput input)
    {
        var entity = input.Adapt<LogAdministrativeDivisionEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除区域管理.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<LogAdministrativeDivisionEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}