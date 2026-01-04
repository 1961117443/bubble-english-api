using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.ModuleButton;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Systems.Interfaces.Permission;

namespace QT.Systems;

/// <summary>
/// 功能按钮



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "ModuleButton", Order = 212)]
[Route("api/system/[controller]")]
public class ModuleButtonService : IModuleButtonService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ModuleButtonEntity> _repository;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IAuthorizeService _authorizeService;

    /// <summary>
    /// 初始化一个<see cref="ModuleButtonService"/>类型的新实例.
    /// </summary>
    public ModuleButtonService(
        ISqlSugarRepository<ModuleButtonEntity> moduleButtonRepository,
        IUserManager userManager, IAuthorizeService authorizeService)
    {
        _repository = moduleButtonRepository;
        _userManager = userManager;
        _authorizeService = authorizeService;
    }

    #region GET

    /// <summary>
    /// 获取按钮权限列表.
    /// </summary>
    /// <param name="moduleId">功能id.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("{moduleId}/List")]
    public async Task<dynamic> GetList(string moduleId, [FromQuery] KeywordInput input)
    {
        var list = await GetList(moduleId);
        if (input.keyword.IsNotEmptyOrNull())
            list = list.FindAll(t => t.EnCode.Contains(input.keyword) || t.FullName.Contains(input.keyword));
        var treeList = list.Adapt<List<ModuleButtonListOutput>>();
        return new { list = treeList.ToTree("-1") };
    }

    /// <summary>
    /// 获取按钮权限下拉列表.
    /// </summary>
    /// <param name="moduleId">菜单ID.</param>
    /// <returns></returns>
    [HttpGet("{moduleId}/Selector")]
    public async Task<dynamic> GetSelector(string moduleId)
    {
        var treeList = (await GetList(moduleId)).Adapt<List<ModuleButtonSelectorOutput>>();
        return new { list = treeList.ToTree("-1") };
    }

    /// <summary>
    /// 获取按钮信息.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var data = await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        return data.Adapt<ModuleButtonInfoOutput>();
    }

    #endregion

    #region Post

    /// <summary>
    /// 添加按钮.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ModuleButtonCrInput input)
    {
        var entity = input.Adapt<ModuleButtonEntity>();
        if (await _repository.AnyAsync(x => (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null && x.ModuleId == input.moduleId))
            throw Oops.Oh(ErrorCode.COM1004);
        var isOk = await Create(entity);
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 修改按钮.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ModuleButtonUpInput input)
    {
        var entity = input.Adapt<ModuleButtonEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除按钮.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if (!await _repository.AnyAsync(r => r.Id == id && r.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1005);
        if (await _repository.AnyAsync(x => x.ParentId == id && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D1007);
        var isOk = await _repository.Context.Updateable<ModuleButtonEntity>().SetColumns(it => new ModuleButtonEntity()
        {
            DeleteMark = 1,
            DeleteUserId = _userManager.UserId,
            DeleteTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();

        if (!isOk) throw Oops.Oh(ErrorCode.COM1003);
    }

    /// <summary>
    /// 更新按钮状态.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task ActionsState(string id)
    {
        if (!await _repository.AnyAsync(r => r.Id == id && r.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1005);

        var isOk = await _repository.Context.Updateable<ModuleButtonEntity>().SetColumns(it => new ModuleButtonEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();

        if (!isOk) throw Oops.Oh(ErrorCode.COM1003);
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 列表.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<ModuleButtonEntity>> GetList(string? moduleId = default)
    {
        return await _repository.AsQueryable().Where(x => x.DeleteMark == null).WhereIF(moduleId.IsNotEmptyOrNull(), it => it.ModuleId == moduleId).OrderBy(o => o.SortCode).ToListAsync();
    }

    /// <summary>
    /// 创建.
    /// </summary>
    /// <param name="entity">实体对象.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<int> Create(ModuleButtonEntity entity)
    {
        return await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取用户模块按钮.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<dynamic> GetUserModuleButtonList()
    {
        var output = new List<ModuleButtonOutput>();
        if (!_userManager.IsAdministrator)
        {
            var roles = _userManager.CurrentUserAndRole; //.CurrentRoles; //_userManager.Roles;
            if (roles.Any())
            {
                //var items = await _repository.Context.Queryable<AuthorizeEntity>().In(a => a.ObjectId, roles).Where(a => a.ItemType == "button").Select(a => a.ItemId).ToListAsync();
                //// 获取禁用的权限
                //var disableItems = await _repository.Context.Queryable<AuthorizeDisableEntity>()
                //    .In(a => a.ObjectId, roles)
                //    .Where(a => a.ItemType == "button")
                //    .Select(a => a.ItemId).ToListAsync();
                //if (disableItems.IsAny())
                //{
                //    items = items.Except(disableItems).ToList();
                //}

                //var items = await _authorizeService.GetCurrentUserAuthorizeList("button");

                var buttons = await _repository.AsQueryable()
                        //.Where(a => items.Contains(a.Id))
                        .Where(a => SqlFunc.Subqueryable<AuthorizeEntity>().Where(it => it.ItemId == a.Id && roles.Contains(it.ObjectId) && it.ItemType == "button").Any())
                        .Where(a => SqlFunc.Subqueryable<AuthorizeDisableEntity>().Where(it => it.ItemId == a.Id && roles.Contains(it.ObjectId) && it.ItemType == "button").NotAny())
                    .Where(a => a.EnabledMark == 1 && a.DeleteMark == null)
                    .Select<ModuleButtonEntity>().OrderBy(q => q.SortCode)
                    .ToListAsync();
                output = buttons.Adapt<List<ModuleButtonOutput>>();
            }
        }
        else
        {
            var buttons = await _repository.AsQueryable().Where(a => a.EnabledMark == 1 && a.DeleteMark == null).Select<ModuleButtonEntity>().OrderBy(q => q.SortCode).ToListAsync();
            output = buttons.Adapt<List<ModuleButtonOutput>>();
        }

        return output;
    }

    #endregion
}