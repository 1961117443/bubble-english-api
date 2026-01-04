using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogDeliveryMember;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Logistics;

/// <summary>
/// 业务实现：配送点会员.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "配送点会员管理", Name = "LogDeliveryMember", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogDeliveryMemberService : ILogDeliveryMemberService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogDeliveryMemberEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="LogDeliveryMemberService"/>类型的新实例.
    /// </summary>
    public LogDeliveryMemberService(
        ISqlSugarRepository<LogDeliveryMemberEntity> logDeliveryMemberRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = logDeliveryMemberRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取配送点会员.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogDeliveryMemberInfoOutput>();
    }

    /// <summary>
    /// 获取配送点会员列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogDeliveryMemberListQueryInput input)
    {
        var data = await _repository.Context.Queryable<LogDeliveryMemberEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.pointId), it => it.PointId.Equals(input.pointId))
            .WhereIF(!string.IsNullOrEmpty(input.memberId), it => it.MemberId.Equals(input.memberId))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.PointId.Contains(input.keyword)
                || it.MemberId.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogDeliveryMemberListOutput
            {
                id = it.Id,
                pointId = it.PointId,
                memberId = it.MemberId,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogDeliveryMemberListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建配送点会员.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogDeliveryMemberCrInput input)
    {
        var entity = input.Adapt<LogDeliveryMemberEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新配送点会员.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogDeliveryMemberUpInput input)
    {
        var entity = input.Adapt<LogDeliveryMemberEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除配送点会员.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<LogDeliveryMemberEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
}