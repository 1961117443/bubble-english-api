using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JZRC.Entitys.Dto.JzrcTransactionRule;
using QT.JZRC.Entitys;
using QT.JZRC.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Common.Const;

namespace QT.JZRC;

/// <summary>
/// 业务实现：交易规则.
/// </summary>
[ApiDescriptionSettings(ModuleConst.JZRC, Tag = "JZRC", Name = "JzrcTransactionRule", Order = 200)]
[Route("api/JZRC/[controller]")]
public class JzrcTransactionRuleService : IJzrcTransactionRuleService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<JzrcTransactionRuleEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="JzrcTransactionRuleService"/>类型的新实例.
    /// </summary>
    public JzrcTransactionRuleService(
        ISqlSugarRepository<JzrcTransactionRuleEntity> jzrcTransactionRuleRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = jzrcTransactionRuleRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// 获取交易规则.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<JzrcTransactionRuleInfoOutput>();
    }

    /// <summary>
    /// 获取交易规则列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] JzrcTransactionRuleListQueryInput input)
    {
        var data = await _repository.Context.Queryable<JzrcTransactionRuleEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.code), it => it.Code.Contains(input.code))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.Code.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new JzrcTransactionRuleListOutput
            {
                id = it.Id,
                name = it.Name,
                code = it.Code,
                remark = it.Remark,
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<JzrcTransactionRuleListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建交易规则.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] JzrcTransactionRuleCrInput input)
    {
        var entity = input.Adapt<JzrcTransactionRuleEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新交易规则.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] JzrcTransactionRuleUpInput input)
    {
        var entity = input.Adapt<JzrcTransactionRuleEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除交易规则.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.Context.Queryable<JzrcTransactionRuleEntity>().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);

        _repository.Context.Tracking(entity);
        entity.Delete();
        var isOk = await _repository.Context.Updateable(entity).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);

        //var isOk = await _repository.Context.Deleteable<JzrcTransactionRuleEntity>().Where(it => it.Id.Equals(id)).ExecuteCommandAsync();
        //if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 获取默认的规则
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<JzrcTransactionRuleInfoOutput> GetDefaultRule()
    {
        var list = await _repository.AsQueryable().WithCache().ToListAsync();
        if (!list.IsAny())
        {
            throw Oops.Oh("请设置规则！");
        }
        var entity = list.First();
        return entity.Adapt<JzrcTransactionRuleInfoOutput>();
    }
}