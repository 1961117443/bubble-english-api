using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Emp.Entitys.Dto.BaseModuleremind;
using QT.Emp.Entitys;
using QT.Emp.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Emp;

/// <summary>
/// 业务实现：模块提醒.
/// </summary>
[ApiDescriptionSettings(Tag = "Emp", Name = "BaseModuleremind", Order = 200)]
[Route("api/Emp/[controller]")]
public class BaseModuleremindService : IBaseModuleremindService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<BaseModuleremindEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="BaseModuleremindService"/>类型的新实例.
    /// </summary>
    public BaseModuleremindService(
        ISqlSugarRepository<BaseModuleremindEntity> baseModuleremindRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = baseModuleremindRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取模块提醒.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<BaseModuleremindInfoOutput>();
    }

    /// <summary>
    /// 获取模块提醒列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] BaseModuleremindListQueryInput input)
    {
        var data = await _repository.Context.Queryable<BaseModuleremindEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.fullName), it => it.FullName.Contains(input.fullName))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.FullName.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new BaseModuleremindListOutput
            {
                id = it.Id,
                fullName = it.FullName,
                description = it.Description,
                sortCode = it.SortCode,
                enabledMark = SqlFunc.IIF(it.EnabledMark == 0, "关", "开"),
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<BaseModuleremindListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建模块提醒.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] BaseModuleremindCrInput input)
    {
        var entity = input.Adapt<BaseModuleremindEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新模块提醒.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] BaseModuleremindUpInput input)
    {
        var entity = input.Adapt<BaseModuleremindEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除模块提醒.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<BaseModuleremindEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}