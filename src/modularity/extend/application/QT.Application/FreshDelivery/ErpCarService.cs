using QT.Application.Entitys.Dto.FreshDelivery.ErpCarService;
using QT.Application.Entitys.FreshDelivery;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.DependencyInjection;
using QT.DynamicApiController;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：车辆管理.
/// </summary>
[ApiDescriptionSettings("生鲜配送", Tag = "车辆管理", Name = "ErpCar", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class ErpCarService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpCarEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="ErpCarService"/>类型的新实例.
    /// </summary>
    public ErpCarService(
        ISqlSugarRepository<ErpCarEntity> erpCarRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = erpCarRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取车辆管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<ErpCarInfoOutput>();
    }

    /// <summary>
    /// 获取车辆管理列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpCarListQueryInput input)
    {
        var data = await _repository.Context.Queryable<ErpCarEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.carNo), it => it.CarNo.Contains(input.carNo))
            .WhereIF(!string.IsNullOrEmpty(input.driver), it => it.Driver.Contains(input.driver))
            .WhereIF(!string.IsNullOrEmpty(input.phone), it => it.Phone.Contains(input.phone))
            .WhereIF(!string.IsNullOrEmpty(input.status), it => it.Status.Contains(input.status))
            .WhereIF(input.deviceId == "100",it => !string.IsNullOrEmpty(it.DeviceId)) // 已绑定
            .WhereIF(input.deviceId == "101", it => string.IsNullOrEmpty(it.DeviceId)) // 未绑定
            .WhereIF(!string.IsNullOrEmpty(input.deviceId) && input.deviceId != "100" && input.deviceId != "101", it => it.DeviceId == input.deviceId)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.CarNo.Contains(input.keyword)
                || it.Driver.Contains(input.keyword)
                || it.Phone.Contains(input.keyword)
                || it.Status.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new ErpCarListOutput
            {
                id = it.Id,
                carNo = it.CarNo,
                driver = it.Driver,
                phone = it.Phone,
                status = it.Status,
                deviceId = it.DeviceId
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ErpCarListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建车辆管理.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ErpCarCrInput input)
    {
        var entity = input.Adapt<ErpCarEntity>();
        // 判断车牌号码是否存在
        if (await _repository.AnyAsync(it => it.CarNo == entity.CarNo))
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新车辆管理.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ErpCarUpInput input)
    {
        var entity = input.Adapt<ErpCarEntity>();
        // 判断车牌号码是否存在
        if (await _repository.AnyAsync(it => it.CarNo == entity.CarNo && it.Id != entity.Id))
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除车辆管理.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<ErpCarEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }


    /// <summary>
    /// 获取下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector()
    {
        var data = await _repository.AsQueryable()
            .OrderBy(a => a.CreatorTime, OrderByType.Desc).ToListAsync();

        return new { list = data.Adapt<List<ErpCarListOutput>>() };
    }
}