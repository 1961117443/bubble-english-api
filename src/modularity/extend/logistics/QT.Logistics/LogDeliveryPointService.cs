using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogDeliveryPoint;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Systems.Entitys.Permission;
using QT.Logistics.Entitys.Dto.LogStoreroom;

namespace QT.Logistics;

/// <summary>
/// 业务实现：配送点管理.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "配送点管理", Name = "LogDeliveryPoint", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogDeliveryPointService : ILogDeliveryPointService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogDeliveryPointEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogDeliveryPointService"/>类型的新实例.
    /// </summary>
    public LogDeliveryPointService(
        ISqlSugarRepository<LogDeliveryPointEntity> logDeliveryPointRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logDeliveryPointRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取配送点管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogDeliveryPointInfoOutput>();
    }

    /// <summary>
    /// 获取配送点管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogDeliveryPointListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogDeliveryPointEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.code), it => it.Code.Contains(input.code))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Code.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogDeliveryPointListOutput
            {
                id = it.Id,
                name = it.Name,
                address = it.Address,
                phone = it.Phone,
                leader = it.Leader,
                code = it.Code,
                adminId = it.AdminId,
                adminIdName = SqlFunc.Subqueryable<UserEntity>().Where(x=>x.Id == it.AdminId).Select(x=>x.RealName)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogDeliveryPointListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建配送点管理.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogDeliveryPointCrInput input)
    {
        var entity = input.Adapt<LogDeliveryPointEntity>();

        if (await _repository.Where(it => it.Code == entity.Code).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        if (await _repository.Where(it => it.Name == entity.Name).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }

        if (!string.IsNullOrEmpty(entity.AdminId))
        {
            if (await _repository.Where(it => it.AdminId == entity.AdminId).AnyAsync())
            {
                throw Oops.Oh("该管理员已绑定配送点，不能重复绑定！");
            }
        }

        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新配送点管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogDeliveryPointUpInput input)
    {
        var entity = input.Adapt<LogDeliveryPointEntity>();
        if (await _repository.Where(it => it.Code == entity.Code && it.Id!=entity.Id).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        if (await _repository.Where(it => it.Name == entity.Name && it.Id != entity.Id).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }

        if (!string.IsNullOrEmpty(entity.AdminId))
        {
            if (await _repository.Where(it => it.AdminId == entity.AdminId && it.Id != entity.Id).AnyAsync())
            {
                throw Oops.Oh("该管理员已绑定配送点，不能重复绑定！");
            }
        }

        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除配送点管理.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<LogDeliveryPointEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 下拉选择
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Selector")]
    public async Task<dynamic> Selector()
    {
        var data = await _repository.AsQueryable().Select(it => new { id= it.Id,name= it.Name }).ToListAsync();

        return new { list = data };
    }

    /// <summary>
    /// 获取当前用户绑定的配送点
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Current")]
    public async Task<LogDeliveryPointEntity> GetCurrentUserPoint()
    {
        return await _repository.Where(it => it.AdminId == _userManager.UserId).FirstAsync();
    }
}