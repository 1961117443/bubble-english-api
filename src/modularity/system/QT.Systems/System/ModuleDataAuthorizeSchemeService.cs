using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.ModuleDataAuthorizeScheme;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Common.Extension;
using QT.Systems.Interfaces.Permission;

namespace QT.Systems;

/// <summary>
/// 数据权限



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "ModuleDataAuthorizeScheme", Order = 214)]
[Route("api/system/[controller]")]
public class ModuleDataAuthorizeSchemeService : IModuleDataAuthorizeSchemeService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ModuleDataAuthorizeSchemeEntity> _repository;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IAuthorizeService _authorizeService;

    /// <summary>
    /// 初始化一个<see cref="ModuleDataAuthorizeSchemeService"/>类型的新实例.
    /// </summary>
    /// <param name="moduleDataAuthorizeSchemeRepository"></param>
    /// <param name="userManager"></param>
    public ModuleDataAuthorizeSchemeService(
        ISqlSugarRepository<ModuleDataAuthorizeSchemeEntity> moduleDataAuthorizeSchemeRepository,
        IUserManager userManager, IAuthorizeService authorizeService)
    {
        _repository = moduleDataAuthorizeSchemeRepository;
        _userManager = userManager;
        _authorizeService = authorizeService;
    }

    #region GET

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="moduleId">功能主键.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("{moduleId}/List")]
    public async Task<dynamic> GetList(string moduleId, [FromQuery] KeywordInput input)
    {
        var list = await GetList(moduleId);
        if (!string.IsNullOrEmpty(input.keyword))
            list = list.FindAll(t => t.EnCode.Contains(input.keyword) || t.FullName.Contains(input.keyword));
        return new { list = list.Adapt<List<ModuleDataAuthorizeSchemeListOutput>>() };
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        var data = await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        return data.Adapt<ModuleDataAuthorizeSchemeInfoOutput>();
    }

    #endregion

    #region POST

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if (!await _repository.AnyAsync(x => x.Id == id && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _repository.Context.Updateable<ModuleDataAuthorizeSchemeEntity>().SetColumns(it => new ModuleDataAuthorizeSchemeEntity()
        {
            DeleteMark = 1,
            DeleteUserId = _userManager.UserId,
            DeleteTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ModuleDataAuthorizeSchemeCrInput input)
    {
        if (await _repository.AnyAsync(x => "jnpf_alldata".Equals(x.EnCode) && x.DeleteMark == null && x.ModuleId == input.moduleId) && "jnpf_alldata".Equals(input.enCode))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<ModuleDataAuthorizeSchemeEntity>();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 编辑.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ModuleDataAuthorizeSchemeUpInput input)
    {
        var entity = input.Adapt<ModuleDataAuthorizeSchemeEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="moduleId">功能主键.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<ModuleDataAuthorizeSchemeEntity>> GetList(string moduleId)
    {
        return await _repository.AsQueryable().Where(x => x.DeleteMark == null && x.ModuleId == moduleId).OrderBy(o => o.SortCode).ToListAsync();
    }

    /// <summary>
    /// 获取用户资源列表.
    /// </summary>
    [NonAction]
    public async Task<dynamic> GetResourceList()
    {
        var output = new List<ModuleDataAuthorizeSchemeOutput>();
        if (!_userManager.IsAdministrator)
        {
            var roles = _userManager.CurrentUserAndRole; //.CurrentRoles; //_userManager.Roles;
            if (roles.Any())
            {
                //var items = await _repository.Context.Queryable<AuthorizeEntity>().In(a => a.ObjectId, roles).Where(a => a.ItemType == "resource").Select(a => a.ItemId).ToListAsync();
                //// 获取禁用的权限
                //var disableItems = await _repository.Context.Queryable<AuthorizeDisableEntity>()
                //    .In(a => a.ObjectId, roles)
                //    .Where(a => a.ItemType == "resource")
                //    .Select(a => a.ItemId).ToListAsync();
                //if (disableItems.IsAny())
                //{
                //    items = items.Except(disableItems).ToList();
                //}
                //var items = await _authorizeService.GetCurrentUserAuthorizeList("resource");

                var buttons = await _repository.AsQueryable()
                    //.Where(a => items.Contains(a.Id))
                        .Where(a => SqlFunc.Subqueryable<AuthorizeEntity>().Where(it => it.ItemId == a.Id && roles.Contains(it.ObjectId) && it.ItemType == "resource").Any())
                        .Where(a => SqlFunc.Subqueryable<AuthorizeDisableEntity>().Where(it => it.ItemId == a.Id && roles.Contains(it.ObjectId) && it.ItemType == "resource").NotAny())
                    .Where(a => a.EnabledMark == 1 && a.DeleteMark == null)
                    .Select<ModuleDataAuthorizeSchemeEntity>().OrderBy(q => q.SortCode)
                    .ToListAsync();
                output = buttons.Adapt<List<ModuleDataAuthorizeSchemeOutput>>();
            }
        }
        else
        {
            var buttons = await _repository.AsQueryable().Where(a => a.EnabledMark == 1 && a.DeleteMark == null).Select<ModuleDataAuthorizeSchemeEntity>().OrderBy(q => q.SortCode).ToListAsync();
            output = buttons.Adapt<List<ModuleDataAuthorizeSchemeOutput>>();
        }

        return output;
    }

    #endregion
}