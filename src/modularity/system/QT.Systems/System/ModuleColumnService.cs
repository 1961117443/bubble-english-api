using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.ModuleColumn;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using QT.VisualDev.Engine;
using QT.VisualDev.Entitys;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SqlSugar;
using QT.Systems.Interfaces.Permission;

namespace QT.Systems;

/// <summary>
/// 功能列表



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "ModuleColumn", Order = 213)]
[Route("api/system/[controller]")]
public class ModuleColumnService : IModuleColumnService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 系统功能按钮表仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ModuleColumnEntity> _repository;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IAuthorizeService _authorizeService;

    /// <summary>
    /// 初始化一个<see cref="ModuleColumnService"/>类型的新实例.
    /// </summary>
    public ModuleColumnService(
        ISqlSugarRepository<ModuleColumnEntity> moduleColumnRepository,
        IUserManager userManager, IAuthorizeService authorizeService)
    {
        _repository = moduleColumnRepository;
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
    [HttpGet("{moduleId}/Fields")]
    public async Task<dynamic> GetList(string moduleId, [FromQuery] KeywordInput input)
    {
        var list = await _repository.Context
                .Queryable<ModuleColumnEntity, ModuleEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.ModuleId == b.Id))
                .Where((a, b) => a.ModuleId == moduleId && a.DeleteMark == null && b.DeleteMark == null)
                .WhereIF(input.keyword.IsNotEmptyOrNull(), a => a.EnCode.Contains(input.keyword) || a.FullName.Contains(input.keyword))
                .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc)
                .Select((a, b) => new ModuleColumnListOutput()
                {
                    bindTable = a.BindTable,
                    enabledMark = a.EnabledMark,
                    fullName = a.FullName,
                    enCode = b.Type == 2 && a.FieldRule == 1 && !SqlFunc.IsNullOrEmpty(a.BindTable) ? a.EnCode.Replace("qt_" + a.BindTable + "_qt_", "") : a.EnCode,
                    id = a.Id,
                    sortCode = a.SortCode
                }).ToListAsync();
        return new { list = list };
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var data = await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        if (await _repository.Context.Queryable<ModuleEntity>().AnyAsync(x => x.Id == data.ModuleId && x.DeleteMark == null && x.Type == 2) && data.FieldRule == 1 && data.BindTable.IsNotEmptyOrNull())
            data.EnCode = data.EnCode.Replace("qt_" + data.BindTable + "_qt_", string.Empty);
        return data.Adapt<ModuleColumnInfoOutput>();
    }

    /// <summary>
    /// 字段列表.
    /// </summary>
    /// <param name="moduleId">菜单id.</param>
    /// <returns></returns>
    [HttpGet("{moduleId}/FieldList")]
    public async Task<dynamic> GetFieldList(string moduleId)
    {
        var moduleEntity = await _repository.Context.Queryable<ModuleEntity>().FirstAsync(x => x.Id == moduleId && x.DeleteMark == null);
        var visualDevId = moduleEntity.PropertyJson.ToObject<JObject>()["moduleId"].ToString();
        var visualDevEntity = await _repository.Context.Queryable<VisualDevEntity>().FirstAsync(x => x.Id == visualDevId && x.DeleteMark == null);
        var defaultColumnList = visualDevEntity.ColumnData.ToObject<ColumnDesignModel>().defaultColumnList;
        var uelessList = new List<string>() { "PsdInput", "colorPicker", "rate", "slider", "divider", "uploadImg", "uploadFz", "editor", "QTText", "relationFormAttr", "popupAttr", "groupTitle" };
        return defaultColumnList?.Where(x => !uelessList.Contains(x.qtKey)).Select(x => new { field = x.prop, fieldName = x.label }).ToList();
    }
    #endregion

    #region POST

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ModuleColumnCrInput input)
    {
        var entity = input.Adapt<ModuleColumnEntity>();
        if (await _repository.Context.Queryable<ModuleEntity>().AnyAsync(x => x.Id == input.moduleId && x.DeleteMark == null && x.Type == 2) && entity.FieldRule == 1 && entity.BindTable.IsNotEmptyOrNull())
            entity.EnCode = "qt_" + input.bindTable + "_qt_" + entity.EnCode;
        if (await _repository.AnyAsync(x => x.EnCode.Equals(entity.EnCode) && x.DeleteMark == null && x.ModuleId == input.moduleId))
            throw Oops.Oh(ErrorCode.COM1004);
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ModuleColumnUpInput input)
    {
        var entity = input.Adapt<ModuleColumnEntity>();
        if (await _repository.Context.Queryable<ModuleEntity>().AnyAsync(x => x.Id == input.moduleId && x.DeleteMark == null && x.Type == 2) && entity.FieldRule == 1 && entity.BindTable.IsNotEmptyOrNull())
            entity.EnCode = "qt_" + input.bindTable + "_qt_" + entity.EnCode;
        if (await _repository.AnyAsync(x => x.EnCode.Equals(entity.EnCode) && x.DeleteMark == null && x.ModuleId == input.moduleId && x.Id != entity.Id))
            throw Oops.Oh(ErrorCode.COM1004);
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        if (await _repository.AnyAsync(x => x.ParentId == id && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.D1007);
        if (!await _repository.AnyAsync(x => x.Id == id && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _repository.Context.Updateable<ModuleColumnEntity>().SetColumns(it => new ModuleColumnEntity()
        {
            DeleteMark = 1,
            DeleteUserId = _userManager.UserId,
            DeleteTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 批量新建.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("Actions/Batch")]
    public async Task BatchCreate([FromBody] ModuleColumnActionsBatchInput input)
    {
        var entitys = new List<ModuleColumnEntity>();
        foreach (var item in input.columnJson)
        {
            var entity = input.Adapt<ModuleColumnEntity>();
            entity.Id = SnowflakeIdHelper.NextId();
            entity.CreatorTime = DateTime.Now;
            entity.EnabledMark = 1;
            entity.CreatorUserId = _userManager.UserId;
            entity.EnCode = item.enCode;
            entity.FullName = item.fullName;
            entitys.Add(entity);
        }

        var newDic = await _repository.Context.Insertable(entitys).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();
        _ = newDic ?? throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 更新字段状态.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task ActionsState(string id)
    {
        var isOk = await _repository.Context.Updateable<ModuleColumnEntity>().SetColumns(it => new ModuleColumnEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1003);
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 列表.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<ModuleColumnEntity>> GetList(string? moduleId = default)
    {
        return await _repository.AsQueryable().Where(x => x.DeleteMark == null).WhereIF(moduleId.IsNotEmptyOrNull(), it => it.ModuleId == moduleId).OrderBy(o => o.SortCode).ToListAsync();
    }

    /// <summary>
    /// 获取用户功能列表权限.
    /// </summary>
    [NonAction]
    public async Task<dynamic> GetUserModuleColumnList()
    {
        var output = new List<ModuleColumnOutput>();
        if (!_userManager.IsAdministrator)
        {
            var roles = _userManager.CurrentUserAndRole; //.CurrentRoles; //_userManager.Roles;
            if (roles.Any())
            {
                //var items = await _repository.Context.Queryable<AuthorizeEntity>().In(a => a.ObjectId, roles).Where(a => a.ItemType == "column").GroupBy(it => new { it.ItemId }).Select(it => it.ItemId).ToListAsync();

                //// 获取禁用的权限
                //var disableItems = await _repository.Context.Queryable<AuthorizeDisableEntity>()
                //    .In(a => a.ObjectId, roles)
                //    .Where(a => a.ItemType == "column")
                //    .Select(a => a.ItemId).ToListAsync();
                //if (disableItems.IsAny())
                //{
                //    items = items.Except(disableItems).ToList();
                //}
                //var items = await _authorizeService.GetCurrentUserAuthorizeList("column");

                var columns = await _repository.AsQueryable()
                    //.Where(a => items.Contains(a.Id))
                        .Where(a => SqlFunc.Subqueryable<AuthorizeEntity>().Where(it => it.ItemId == a.Id && roles.Contains(it.ObjectId) && it.ItemType == "column").Any())
                        .Where(a => SqlFunc.Subqueryable<AuthorizeDisableEntity>().Where(it => it.ItemId == a.Id && roles.Contains(it.ObjectId) && it.ItemType == "column").NotAny())
                    .Where(a => a.EnabledMark == 1 && a.DeleteMark == null)
                    .Select<ModuleColumnEntity>()
                    .OrderBy(q => q.SortCode, OrderByType.Asc)
                    .ToListAsync();
                output = columns.Adapt<List<ModuleColumnOutput>>();
            }
        }
        else
        {
            var buttons = await _repository.AsQueryable().Where(a => a.EnabledMark == 1 && a.DeleteMark == null).Select<ModuleColumnEntity>().OrderBy(q => q.SortCode).ToListAsync();
            output = buttons.Adapt<List<ModuleColumnOutput>>();
        }

        return output;
    }
    #endregion
}