using QT.Apps.Entitys;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.Module;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Systems.Entitys.Entity.System;
using QT.Systems.Entitys.Enum;
using QT.Systems.Interfaces.Permission;

namespace QT.Systems;

/// <summary>
/// 菜单管理
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "Menu", Order = 212)]
[Route("api/system/[controller]")]
public class ModuleService : IModuleService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 系统功能表仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ModuleEntity> _repository;

    /// <summary>
    /// 功能按钮服务.
    /// </summary>
    private readonly IModuleButtonService _moduleButtonService;

    /// <summary>
    /// 功能列表服务.
    /// </summary>
    private readonly IModuleColumnService _moduleColumnService;

    /// <summary>
    /// 功能数据资源服务.
    /// </summary>
    private readonly IModuleDataAuthorizeSchemeService _moduleDataAuthorizeSchemeService;

    /// <summary>
    /// 功能数据方案服务.
    /// </summary>
    private readonly IModuleDataAuthorizeService _moduleDataAuthorizeSerive;

    /// <summary>
    /// 功能表单服务.
    /// </summary>
    private readonly IModuleFormService _moduleFormSerive;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IAuthorizeService _authorizeService;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="ModuleService"/>类型的新实例.
    /// </summary>
    public ModuleService(
        ISqlSugarRepository<ModuleEntity> moduleRepository,
        IFileManager fileManager,
        ModuleButtonService moduleButtonService,
        IModuleColumnService moduleColumnService,
        IModuleFormService moduleFormSerive,
        IModuleDataAuthorizeService moduleDataAuthorizeSerive,
        IModuleDataAuthorizeSchemeService moduleDataAuthorizeSchemeService,
        IUserManager userManager,
        ISqlSugarClient context,
        IAuthorizeService authorizeService)
    {
        _repository = moduleRepository;
        _fileManager = fileManager;
        _moduleButtonService = moduleButtonService;
        _moduleColumnService = moduleColumnService;
        _moduleFormSerive = moduleFormSerive;
        _moduleDataAuthorizeSchemeService = moduleDataAuthorizeSchemeService;
        _moduleDataAuthorizeSerive = moduleDataAuthorizeSerive;
        _userManager = userManager;
        _authorizeService = authorizeService;
        _db = context.AsTenant();
    }

    #region GET

    /// <summary>
    /// 获取菜单列表.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ModuleListQuery input)
    {
        var data = await GetList();
        if (!string.IsNullOrEmpty(input.category))
            data = data.FindAll(x => x.Category == input.category);
        if (!string.IsNullOrEmpty(input.keyword))
            data = data.TreeWhere(t => t.FullName.Contains(input.keyword) || t.EnCode.Contains(input.keyword) || (t.UrlAddress.IsNotEmptyOrNull() && t.UrlAddress.Contains(input.keyword)), t => t.Id, t => t.ParentId);
        var treeList = data.Adapt<List<ModuleListOutput>>();
        return new { list = treeList.ToTree("-1") };
    }

    /// <summary>
    /// 获取菜单下拉框.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="category">菜单分类（参数有Web,App），默认显示所有分类.</param>
    /// <returns></returns>
    [HttpGet("Selector/{id}")]
    public async Task<dynamic> GetSelector(string id, string category)
    {
        var data = await GetList();
        if (!string.IsNullOrEmpty(category))
            data = data.FindAll(x => x.Category == category && x.Type == 1);
        if (!id.Equals("0"))
            data.RemoveAll(x => x.Id == id);
        var treeList = data.Adapt<List<ModuleSelectorOutput>>();
        return new { list = treeList.ToTree("-1") };
    }

    /// <summary>
    /// 获取菜单列表（下拉框）.
    /// </summary>
    /// <param name="category">菜单分类（参数有Web,App）.</param>
    /// <returns></returns>
    [HttpGet("Selector/All")]
    public async Task<dynamic> GetSelectorAll(string category)
    {
        var data = await GetList();
        if (!string.IsNullOrEmpty(category))
            data = data.FindAll(x => x.Category == category);
        var treeList = data.Adapt<List<ModuleSelectorAllOutput>>();
        return new { list = treeList.ToTree("-1") };
    }

    /// <summary>
    /// 获取菜单信息.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        var data = await GetInfo(id);
        return data.Adapt<ModuleInfoOutput>();
    }

    /// <summary>
    /// 导出.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/Action/Export")]
    public async Task<dynamic> ActionsExport(string id)
    {
        var data = (await GetInfo(id)).Adapt<ModuleExportInput>();
        data.buttonEntityList = (await _moduleButtonService.GetList(id)).Adapt<List<ButtonEntityListItem>>();
        data.columnEntityList = (await _moduleColumnService.GetList(id)).Adapt<List<ColumnEntityListItem>>();
        data.authorizeEntityList = (await _moduleDataAuthorizeSerive.GetList(id)).Adapt<List<AuthorizeEntityListItem>>();
        data.schemeEntityList = (await _moduleDataAuthorizeSchemeService.GetList(id)).Adapt<List<SchemeEntityListItem>>();
        data.formEntityList = (await _moduleFormSerive.GetList(id)).Adapt<List<FromEntityListItem>>();
        var jsonStr = data.ToJsonString();
        return await _fileManager.Export(jsonStr, data.fullName, ExportFileType.bm);
    }

    /// <summary>
    /// 获取模块关联的打印模板.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/Action/Print")]
    public async Task<dynamic> ActionsPrint(string id)
    {
        var list = await _repository.Context.Queryable<ModuleRelationEntity>()
            .Where(it => it.ModuleId == id && it.ObjectType == ModuleRelationType.PrintDev)
            .Select(it => it.ObjectId)
            .ToListAsync();
        return await _repository.Context.Queryable<PrintDevEntity>()
            .Where(it=> list.Contains(it.Id) && it.EnabledMark == 1 && it.DeleteMark == null)
            .Select(it=>new { id=it.Id,fullName=it.FullName,enCode= it.EnCode,propertyJson = it.PropertyJson})
            .ToListAsync();
    }
    #endregion

    #region Post

    /// <summary>
    /// 添加菜单.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Creater([FromBody] ModuleCrInput input)
    {
        if (await _repository.AnyAsync(x => (x.EnCode == input.enCode /*|| x.FullName == input.fullName*/) && x.DeleteMark == null && x.Category == input.category && input.parentId == x.ParentId))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<ModuleEntity>();

        try
        {
            _db.BeginTran();

            // 添加字典菜单按钮
            if (entity.Type == 4)
            {
                foreach (var item in await _moduleButtonService.GetList())
                {
                    if (item.ModuleId == "-1")
                    {
                        item.ModuleId = entity.Id;
                        await _moduleButtonService.Create(item);
                    }
                }
            }

            var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
            if (isOk < 1)
                throw Oops.Oh(ErrorCode.COM1000);
            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.COM1000);
        }
    }

    /// <summary>
    /// 修改菜单.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ModuleUpInput input)
    {
        if (await _repository.AnyAsync(x => x.Id != id && (x.EnCode == input.enCode /*|| x.FullName == input.fullName*/) && x.DeleteMark == null && x.Category == input.category && input.parentId == x.ParentId))
            throw Oops.Oh(ErrorCode.COM1004);
        if (await _repository.AnyAsync(x => x.ParentId == id && x.DeleteMark == null&& x.Type == 1 && x.Type != input.type))
            throw Oops.Oh(ErrorCode.D4008);
        var entity = input.Adapt<ModuleEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除菜单.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        try
        {
            var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
            if (entity == null || await _repository.AnyAsync(x => x.ParentId == id && x.DeleteMark == null))
                throw Oops.Oh(ErrorCode.D1007);

            _db.BeginTran();

            if (entity.Category.Equals("App"))
            {
                await _repository.Context.Updateable<AppDataEntity>().SetColumns(it => new AppDataEntity()
                {
                    DeleteMark = 1,
                    DeleteUserId = _userManager.UserId,
                    DeleteTime = SqlFunc.GetDate()
                }).Where(it => it.ObjectId == id && it.CreatorUserId==_userManager.UserId).ExecuteCommandAsync();
            }

            var isOk = await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandHasChangeAsync();
            if (!isOk)
                throw Oops.Oh(ErrorCode.COM1002);

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.COM1002);
        }
    }

    /// <summary>
    /// 更新菜单状态.
    /// </summary>
    /// <param name="id">菜单id.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task ActionsState(string id)
    {
        var isOk = await _repository.Context.Updateable<ModuleEntity>().SetColumns(it => new ModuleEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1003);
    }

    /// <summary>
    /// 导入.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("Action/Import")]
    public async Task ActionsImport(IFormFile file)
    {
        var fileType = Path.GetExtension(file.FileName).Replace(".", string.Empty);
        if (!fileType.ToLower().Equals(ExportFileType.bm.ToString()))
            throw Oops.Oh(ErrorCode.D3006);
        var josn = _fileManager.Import(file);
        var moduleModel = josn.ToObject<ModuleExportInput>();
        if (moduleModel == null || moduleModel.linkTarget.IsNullOrEmpty())
            throw Oops.Oh(ErrorCode.D3006);
        if (moduleModel.parentId != "-1" && !await _repository.AnyAsync(x => x.Id == moduleModel.parentId && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D3007);
        if (await _repository.AnyAsync(x => x.EnCode == moduleModel.enCode && x.DeleteMark == null) || await _repository.AnyAsync(x => x.FullName == moduleModel.fullName && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D4000);
        await ImportData(moduleModel);
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 列表.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<ModuleEntity>> GetList()
    {
        return await _repository.AsQueryable().Where(x => x.DeleteMark == null).OrderBy(o => o.SortCode).ToListAsync();
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<ModuleEntity> GetInfo(string id)
    {
        return await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
    }

    /// <summary>
    /// 获取用户树形模块功能列表.
    /// </summary>
    /// <param name="type">登录类型.</param>
    [NonAction]
    public async Task<List<ModuleNodeOutput>> GetUserTreeModuleList(string type)
    {
        /*
        var output = new List<ModuleNodeOutput>();
        if (!_userManager.IsAdministrator)
        {
            var roles = _userManager.CurrentUserAndRole; //.CurrentRoles; // _userManager.Roles;
            if (roles.Any())
            {
                var items = await _repository.Context.Queryable<AuthorizeEntity>()
                    .In(a => a.ObjectId, roles)
                    .Where(a => a.ItemType == "module")
                    .Select(a => a.ItemId).ToListAsync();

                // 获取禁用的权限
                var disableItems = await _repository.Context.Queryable<AuthorizeDisableEntity>()
                    .In(a => a.ObjectId, roles)
                    .Where(a => a.ItemType == "module")
                    .Select(a => a.ItemId).ToListAsync();
                if (disableItems.IsAny())
                {
                    items = items.Except(disableItems).ToList();
                }
                

                var menus = await _repository.AsQueryable().Where(a => items.Contains(a.Id)).Where(a => a.EnabledMark == 1 && a.Category.Equals(type) && a.DeleteMark == null).Select<ModuleEntity>().OrderBy(q => q.ParentId).OrderBy(q => q.SortCode).ToListAsync();
                output = menus.Adapt<List<ModuleNodeOutput>>();
            }
        }
        else
        {
            var menus = await _repository.AsQueryable().Where(a => a.EnabledMark == 1 && a.Category.Equals(type) && a.DeleteMark == null).Select<ModuleEntity>().OrderBy(q => q.ParentId).OrderBy(q => q.SortCode).ToListAsync();
            output = menus.Adapt<List<ModuleNodeOutput>>();
        }*/
        var output = (await GetUserModueEntityList(type))?.Adapt<List<ModuleNodeOutput>>() ?? new List<ModuleNodeOutput>();
        // 处理路由地址
        var groups = output.Where(it => it.urlAddress.IsNotEmptyOrNull()).GroupBy(it => it.urlAddress.Split("?")[0]).Where(x => x.Count() > 1);
        foreach (var g in groups)
        {
            foreach (var item in g)
            {
                var arr = item.urlAddress.Split("?");
                item.urlRoute = $"{arr[0]}/{item.id}";
                if (arr.Length>1)
                {
                    item.urlRoute += $"?{arr[1]}";
                }
            }
        }

        return output.ToTree("-1");
    }

    /// <summary>
    /// 获取用户菜单模块功能列表.
    /// </summary>
    /// <param name="type">登录类型.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<dynamic> GetUserModueList(string type)
    {
        /*
        var output = new List<ModuleOutput>();
        if (!_userManager.IsAdministrator)
        {
            var roles = _userManager.CurrentUserAndRole; //.CurrentRoles; //_userManager.Roles;
            if (roles.Any())
            {
                var items = await _repository.Context.Queryable<AuthorizeEntity>().In(a => a.ObjectId, roles).Where(a => a.ItemType == "module").GroupBy(it => new { it.ItemId }).Select(it => it.ItemId).ToListAsync();

                // 获取禁用的权限
                var disableItems = await _repository.Context.Queryable<AuthorizeDisableEntity>()
                    .In(a => a.ObjectId, roles)
                    .Where(a => a.ItemType == "module")
                    .Select(a => a.ItemId).ToListAsync();
                if (disableItems.IsAny())
                {
                    items = items.Except(disableItems).ToList();
                }

                output = await _repository.AsQueryable().Where(a => items.Contains(a.Id)).Where(a => a.EnabledMark == 1 && a.Category.Equals(type) && a.DeleteMark == null).Select(a => new { Id = a.Id, FullName = a.FullName, SortCode = a.SortCode }).MergeTable().OrderBy(o => o.SortCode).Select<ModuleOutput>().ToListAsync();
            }
        }
        else
        {
            output = await _repository.AsQueryable().Where(a => a.EnabledMark == 1 && a.Category.Equals(type) && a.DeleteMark == null).Select(a => new { Id = a.Id, FullName = a.FullName, SortCode = a.SortCode }).MergeTable().OrderBy(o => o.SortCode).Select<ModuleOutput>().ToListAsync();
        }
        return output;
        */
        return (await GetUserModueEntityList(type))?.Adapt<List<ModuleOutput>>() ?? new List<ModuleOutput>();
    }

    private List<ModuleEntity> _moduleEntities;
    /// <summary>
    /// 获取用户菜单 scoped 缓存
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [NonAction]
    private async Task<List<ModuleEntity>> GetUserModueEntityList(string type)
    {
        if (!_moduleEntities.IsAny())
        {
            if (!_userManager.IsAdministrator)
            {
                var roles = _userManager.CurrentUserAndRole; //.CurrentRoles; // _userManager.Roles;
                if (roles.Any())
                {
                    /*
                    var items = await _repository.Context.Queryable<AuthorizeEntity>()
                        .In(a => a.ObjectId, roles)
                        .Where(a => a.ItemType == "module")
                        .Select(a => a.ItemId).ToListAsync();

                    // 获取禁用的权限
                    var disableItems = await _repository.Context.Queryable<AuthorizeDisableEntity>()
                        .In(a => a.ObjectId, roles)
                        .Where(a => a.ItemType == "module")
                        .Select(a => a.ItemId).ToListAsync();
                    if (disableItems.IsAny())
                    {
                        items = items.Except(disableItems).ToList();
                    }
                    */

                    //var items = await _authorizeService.GetCurrentUserAuthorizeList("module");


                    _moduleEntities = await _repository.AsQueryable()
                        //.Where(a => items.Contains(a.Id))
                        .Where(a=>SqlFunc.Subqueryable<AuthorizeEntity>().Where(it=>it.ItemId == a.Id && roles.Contains(it.ObjectId) && it.ItemType == "module").Any())
                        .Where(a => SqlFunc.Subqueryable<AuthorizeDisableEntity>().Where(it => it.ItemId == a.Id && roles.Contains(it.ObjectId) && it.ItemType == "module").NotAny())
                        .Where(a => a.EnabledMark == 1 && a.Category.Equals(type) && a.DeleteMark == null)
                        .Select<ModuleEntity>().OrderBy(q => q.ParentId)
                        .OrderBy(q => q.SortCode).ToListAsync();
                }
            }
            else
            {
                _moduleEntities = await _repository.AsQueryable().Where(a => a.EnabledMark == 1 && a.Category.Equals(type) && a.DeleteMark == null).Select<ModuleEntity>().OrderBy(q => q.ParentId).OrderBy(q => q.SortCode).ToListAsync();
            }
        }
        
        return _moduleEntities;
    }
    #endregion

    #region PrivateMethod

    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private async Task ImportData(ModuleExportInput data)
    {
        try
        {
            var module = data.Adapt<ModuleEntity>();
            var button = data.buttonEntityList.Adapt<List<ModuleButtonEntity>>();
            var colum = data.buttonEntityList.Adapt<List<ModuleColumnEntity>>();
            var dataAuthorize = data.buttonEntityList.Adapt<List<ModuleDataAuthorizeEntity>>();
            var dataAuthorizeScheme = data.buttonEntityList.Adapt<List<ModuleDataAuthorizeSchemeEntity>>();
            var form = data.formEntityList.Adapt<List<ModuleFormEntity>>();

            _db.BeginTran();

            var isOk1 = await _repository.Context.Storageable(button).ExecuteCommandAsync();
            var isOk2 = await _repository.Context.Storageable(colum).ExecuteCommandAsync();
            var isOk3 = await _repository.Context.Storageable(dataAuthorize).ExecuteCommandAsync();
            var isOk4 = await _repository.Context.Storageable(dataAuthorizeScheme).ExecuteCommandAsync();
            var isOk5 = await _repository.Context.Storageable(form).ExecuteCommandAsync();
            var isOk6 = await _repository.Context.Storageable(module).ExecuteCommandAsync();

            if (isOk1 < 1 && isOk2 < 1 && isOk3 < 1 && isOk4 < 1 && isOk5 < 1 && isOk6 < 1)
                throw Oops.Oh(ErrorCode.D3008);
            _db.CommitTran();
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D3008);
        }
    }

    #endregion
}