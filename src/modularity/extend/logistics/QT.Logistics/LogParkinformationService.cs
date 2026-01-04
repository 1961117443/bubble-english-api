using QT.Logistics.Entitys;
using QT.Logistics.Entitys.Dto.LogParkinformation;
using QT.Logistics.Interfaces;

namespace QT.Logistics;

/// <summary>
/// 业务实现：物流园信息.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "物流园信息管理", Name = "LogParkinformation", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogParkinformationService : ILogParkinformationService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogParkinformationEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogParkinformationService"/>类型的新实例.
    /// </summary>
    public LogParkinformationService(
        ISqlSugarRepository<LogParkinformationEntity> logParkinformationRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logParkinformationRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取物流园信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogParkinformationInfoOutput>();
    }

    /// <summary>
    /// 获取物流园信息列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogParkinformationListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogParkinformationEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogParkinformationListOutput
            {
                id = it.Id,
                name = it.Name,
                address = it.Address,
                description = it.Description,
                phone = it.Phone,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogParkinformationListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建物流园信息.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogParkinformationCrInput input)
    {
        var entity = input.Adapt<LogParkinformationEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新物流园信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogParkinformationUpInput input)
    {
        var entity = input.Adapt<LogParkinformationEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除物流园信息.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<LogParkinformationEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}