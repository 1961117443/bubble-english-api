using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Logistics.Entitys.Dto.LogEnterpriseSupplier;
using QT.Logistics.Entitys;
using QT.Logistics.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Systems.Interfaces.System;

namespace QT.Logistics;

/// <summary>
/// 业务实现：供应商.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "商家供应商管理", Name = "LogEnterpriseSupplier", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogEnterpriseSupplierService : ILogEnterpriseSupplierService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<LogEnterpriseSupplierEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IModuleDataAuthorizeSchemeService _moduleDataAuthorizeSchemeService;

    /// <summary>
    /// 初始化一个<see cref="LogEnterpriseSupplierService"/>类型的新实例.
    /// </summary>
    public LogEnterpriseSupplierService(
        ISqlSugarRepository<LogEnterpriseSupplierEntity> logEnterpriseSupplierRepository,
        ISqlSugarClient context,
        IUserManager userManager,
        IModuleDataAuthorizeSchemeService moduleDataAuthorizeSchemeService)
    {
        _repository = logEnterpriseSupplierRepository;
        _userManager = userManager;
        _moduleDataAuthorizeSchemeService = moduleDataAuthorizeSchemeService;
    }

    #region 增删改查
    /// <summary>
    /// 获取供应商.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        return (await _repository.FirstOrDefaultAsync(x => x.Id == id)).Adapt<LogEnterpriseSupplierInfoOutput>();
    }

    /// <summary>
    /// 获取供应商列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] LogEnterpriseSupplierListQueryInput input)
    {
        //var list = _userManager.DataScope;
        //var c = _userManager.GetConditionAsync<LogEnterpriseSupplierEntity>(input.menuId);
        var authList = await _moduleDataAuthorizeSchemeService.GetList(input.menuId);
        var qur = _repository.Context.Queryable<LogEnterpriseSupplierEntity>();
        if (authList.Any(it => it.EnCode == "qt_alldata"))
        {
            qur = qur.ClearFilter<ILogEnterpriseEntity>();
        }
        var data = await qur
            .WhereIF(!string.IsNullOrEmpty(input.name), it => it.Name.Contains(input.name))
            .WhereIF(!string.IsNullOrEmpty(input.firstChar), it => it.FirstChar.Contains(input.firstChar))
            .WhereIF(!string.IsNullOrEmpty(input.keyword), it =>
                it.Name.Contains(input.keyword)
                || it.FirstChar.Contains(input.keyword)
                )
            .OrderByIF(string.IsNullOrEmpty(input.sidx), it => it.Id)
            .Select(it => new LogEnterpriseSupplierListOutput
            {
                id = it.Id,
                name = it.Name,
                firstChar = it.FirstChar,
                address = it.Address,
                admin = it.Admin,
                admintel = it.Admintel,
                eIdName = SqlFunc.Subqueryable<LogEnterpriseEntity>().Where(d=>d.Id == it.EId).Select(d=>d.Name)
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogEnterpriseSupplierListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建供应商.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogEnterpriseSupplierCrInput input)
    {
        var entity = input.Adapt<LogEnterpriseSupplierEntity>();
        if (await _repository.Where(it => it.Name == entity.Name).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        entity.Id = SnowflakeIdHelper.NextId();
        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新供应商.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] LogEnterpriseSupplierUpInput input)
    {
        var entity = input.Adapt<LogEnterpriseSupplierEntity>();
        if (await _repository.Where(it => it.Name == entity.Name && it.Id != entity.Id).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.COM1004);
        }
        entity.FirstChar = PinyinHelper.PinyinString(entity.Name);
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除供应商.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Updateable<LogEnterpriseSupplierEntity>()
            .CallEntityMethod(it => it.Delete())
            .UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId })
            .ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }
    #endregion

    /// <summary>
    /// 下拉选择
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/Selector")]
    public async Task<dynamic> Selector()
    {
        var data = await _repository.AsQueryable().Select(it=>new {id=it.Id,name=it.Name}).ToListAsync();

        return new { list = data };
    }
}