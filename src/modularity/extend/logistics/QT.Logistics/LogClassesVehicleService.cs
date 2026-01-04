using QT.Logistics.Entitys;
using QT.Logistics.Entitys.Dto.LogClassesVehicle;
using QT.Logistics.Interfaces;
using QT.Systems.Entitys.Permission;

namespace QT.Logistics;

/// <summary>
/// 业务实现：发车记录.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "发车记录管理", Name = "LogClassesVehicle", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogClassesVehicleService : ILogClassesVehicleService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogClassesVehicleEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogClassesVehicleService"/>类型的新实例.
    /// </summary>
    public LogClassesVehicleService(
        ISqlSugarRepository<LogClassesVehicleEntity> logClassesVehicleRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logClassesVehicleRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取发车记录.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogClassesVehicleInfoOutput>();
    }

    /// <summary>
    /// 获取发车记录列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogClassesVehicleListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogClassesVehicleEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.cId), it => it.CId.Equals(input.cId))
            .WhereIF(!string.IsNullOrEmpty(input.vId), it => it.VId.Equals(input.vId))
            .WhereIF(!string.IsNullOrEmpty(input.dispatcher), it => it.Dispatcher.Contains(input.dispatcher))
            .WhereIF(!string.IsNullOrEmpty(input.code), it => it.Code.Contains(input.code))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.CId.Contains(input.keyword)
                || it.VId.Contains(input.keyword)
                || it.Dispatcher.Contains(input.keyword)
                || it.Code.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogClassesVehicleListOutput
            {
                id = it.Id,
                cId = it.CId,
                vId = it.VId,
                departureTime = it.DepartureTime,
                dispatcher = it.Dispatcher,
                code = it.Code,
                dispatcherName = SqlFunc.Subqueryable<UserEntity>().Where(x=>x.Id == it.Dispatcher).Select(x=>x.RealName),
                enabledMark = it.EnabledMark ?? 1
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogClassesVehicleListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建发车记录.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogClassesVehicleCrInput input)
    {
        var entity = input.Adapt<LogClassesVehicleEntity>();

        if (await _repository.AnyAsync(it=>it.Code == entity.Code))
        {
            throw Oops.Oh("车次编号已存在！");
        }

        // 发车时间大于当前时间，更新车辆的状态为 配送中
        if (input.departureTime > DateTime.Now && !string.IsNullOrEmpty(input.vId))
        {
           await _repository.Context.Updateable<LogVehicleEntity>()
                .SetColumns(it => it.TransportStatus, "配送中")
                .Where(it => it.Id == input.vId)
                .ExecuteCommandAsync();
        }

        entity.Id = SnowflakeIdHelper.NextId();
        entity.EnabledMark = 1;
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新发车记录.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogClassesVehicleUpInput input)
    {
        var entity = input.Adapt<LogClassesVehicleEntity>();
        if (await _repository.AnyAsync(it => it.Code == entity.Code && it.Id != entity.Id))
        {
            throw Oops.Oh("车次编号已存在！");
        }

        // 发车时间大于当前时间，更新车辆的状态为 配送中
        if (input.departureTime > DateTime.Now && !string.IsNullOrEmpty(input.vId))
        {
            await _repository.Context.Updateable<LogVehicleEntity>()
                 .SetColumns(it => it.TransportStatus, "配送中")
                 .Where(it => it.Id == input.vId)
                 .ExecuteCommandAsync();
        }
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除发车记录.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<LogClassesVehicleEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 下拉项 发车时间大于当前时间.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Selector")]
    public async Task<dynamic> GetSelector()
    {
        var list = await _repository
            .Where(it=>it.DepartureTime >= DateTime.Now)
            .Where(it=>it.EnabledMark == 1)
            .Select(it=> new
            {
                id=it.Id,
                no = it.Code
            }).ToListAsync();

        return new {  list };
    }

    /// <summary>
    /// 结束/到达.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/Reach/{id}")]
    [SqlSugarUnitOfWork]
    public async Task Reach(string id)
    {
        var entity = await _repository.AsQueryable().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
        if (entity.EnabledMark == 0)
        {
            throw Oops.Oh("车辆已到达！");
        }
        if (entity.DepartureTime > DateTime.Now)
        {
            throw Oops.Oh("发车时间大于当前时间！");
        }

        if (!string.IsNullOrEmpty(entity.VId))
        {
            await _repository.Context.Updateable<LogVehicleEntity>()
                .SetColumns(it => it.TransportStatus, "空闲")
                .Where(it => it.Id == entity.VId)
                .ExecuteCommandAsync();
        }

        entity.EnabledMark = 0;
        await _repository.Context.Updateable(entity).UpdateColumns(it => it.EnabledMark).ExecuteCommandAsync();


    }
}