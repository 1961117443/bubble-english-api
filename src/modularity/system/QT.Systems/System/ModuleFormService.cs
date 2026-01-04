using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.ModuleForm;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using QT.VisualDev.Engine;
using QT.VisualDev.Engine.Core;
using QT.VisualDev.Engine.Security;
using QT.VisualDev.Entitys;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SqlSugar;
using QT.Systems.Interfaces.Permission;

namespace QT.Systems;

/// <summary>
/// 功能表单.



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "ModuleForm", Order = 212)]
[Route("api/system/[controller]")]
public class ModuleFormService : IModuleFormService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 系统功能表单表仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ModuleFormEntity> _repository;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly IAuthorizeService _authorizeService;

    /// <summary>
    /// 初始化一个<see cref="ModuleFormService"/>类型的新实例.
    /// </summary>
    public ModuleFormService(
        ISqlSugarRepository<ModuleFormEntity> moduleFormRepository,
        IUserManager userManager, IAuthorizeService authorizeService)
    {
        _repository = moduleFormRepository;
        _userManager = userManager;
        _authorizeService = authorizeService;
    }

    #region Get

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
                .Queryable<ModuleFormEntity, ModuleEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.ModuleId == b.Id))
                .Where((a, b) => a.ModuleId == moduleId && a.DeleteMark == null && b.DeleteMark == null)
                .WhereIF(input.keyword.IsNotEmptyOrNull(), a => a.EnCode.Contains(input.keyword) || a.FullName.Contains(input.keyword))
                .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderByIF(!string.IsNullOrEmpty(input.keyword), t => t.LastModifyTime, OrderByType.Desc)
                .Select((a, b) => new ModuleFormListOutput()
                {
                    id = a.Id,
                    fullName = a.FullName,
                    enabledMark = a.EnabledMark,
                    description = a.Description,
                    moduleId = a.ModuleId,
                    fieldRule = a.FieldRule,
                    bindTable = a.BindTable,
                    sortCode = a.SortCode,
                    enCode = b.Type == 2 && a.FieldRule == 1 && !SqlFunc.IsNullOrEmpty(a.BindTable) ? a.EnCode.Replace("jnpf_" + a.BindTable + "_jnpf_", "") : a.EnCode,
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
        var moduleEntity = await _repository.Context.Queryable<ModuleEntity>().FirstAsync(x => x.Id == data.ModuleId && x.DeleteMark == null);
        if (moduleEntity != null && moduleEntity.Type == 2 && data.FieldRule == 1 && data.BindTable.IsNotEmptyOrNull())
        {
            data.EnCode = data.EnCode.Replace("jnpf_" + data.BindTable + "_jnpf_", string.Empty);
        }
        return data.Adapt<ModuleFormListOutput>();
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
        var columnList = visualDevEntity.ColumnData.ToObject<ColumnDesignModel>().columnList;
        var formDataModel = TemplateKeywordsHelper.ReplaceKeywords(visualDevEntity.FormData).ToObject<FormDataModel>();
        var fields = formDataModel.fields.Where(x => x.__vModel__.IsNotEmptyOrNull()).ToList();
        var childFieldList = fields.Select(x => new { field = x.__vModel__, fieldName = x.__config__.label }).ToList();
        return columnList.Select(x => new { field = x.prop, fieldName = x.label }).Union(childFieldList).ToList();
    }
    #endregion

    #region Post

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ModuleFormCrInput input)
    {
        var entity = input.Adapt<ModuleFormEntity>();
        if (await _repository.Context.Queryable<ModuleEntity>().AnyAsync(x => x.Id == input.moduleId && x.DeleteMark == null && x.Type == 2) && entity.FieldRule == 1 && entity.BindTable.IsNotEmptyOrNull())
            entity.EnCode = "jnpf_" + input.bindTable + "_jnpf_" + entity.EnCode;
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
    public async Task Update(string id, [FromBody] ModuleFormUpInput input)
    {
        var entity = input.Adapt<ModuleFormEntity>();
        if (await _repository.Context.Queryable<ModuleEntity>().AnyAsync(x => x.Id == input.moduleId && x.DeleteMark == null && x.Type == 2) && entity.FieldRule == 1 && entity.BindTable.IsNotEmptyOrNull())
            entity.EnCode = "jnpf_" + input.bindTable + "_jnpf_" + entity.EnCode;
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
        var isOk = await _repository.Context.Updateable<ModuleFormEntity>().SetColumns(it => new ModuleFormEntity()
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
    /// <param name="input">请求参数</param>
    /// <returns></returns>
    [HttpPost("Actions/Batch")]
    public async Task BatchCreate([FromBody] ModuleFormActionsBatchInput input)
    {
        var entitys = new List<ModuleFormEntity>();
        foreach (var item in input.formJson)
        {
            var entity = input.Adapt<ModuleFormEntity>();
            entity.Id = SnowflakeIdHelper.NextId();
            entity.CreatorTime = DateTime.Now;
            entity.EnabledMark = 1;
            entity.CreatorUserId = _userManager.UserId;
            entity.EnCode = item.enCode;
            entity.FullName = item.fullName;
            entitys.Add(entity);
        }

        var newDic = await _repository.Context.Insertable(entitys).IgnoreColumns(ignoreNullColumn: true).ExecuteReturnEntityAsync();
        _ = newDic ?? throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 更新字段状态.
    /// </summary>
    /// <param name="id">id</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task ActionsState(string id)
    {
        var isOk = await _repository.Context.Updateable<ModuleFormEntity>().SetColumns(it => new ModuleFormEntity()
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
    /// <param name="moduleId"></param>
    /// <returns></returns>
    public async Task<List<ModuleFormEntity>> GetList(string? moduleId = default)
    {
        return await _repository.AsQueryable().Where(x => x.ModuleId == moduleId && x.DeleteMark == null).ToListAsync();
    }

    /// <summary>
    /// 获取用户表单列表.
    /// </summary>
    [NonAction]
    public async Task<dynamic> GetUserModuleFormList()
    {
        var output = new List<ModuleFormOutput>();
        if (!_userManager.IsAdministrator)
        {
            var roles = _userManager.CurrentUserAndRole; //.CurrentRoles; //_userManager.Roles;
            if (roles.Any())
            {
                //var items = await _repository.Context.Queryable<AuthorizeEntity>().In(a => a.ObjectId, roles).Where(a => a.ItemType == "form").GroupBy(it => new { it.ItemId }).Select(a => a.ItemId).ToListAsync();
                //// 获取禁用的权限
                //var disableItems = await _repository.Context.Queryable<AuthorizeDisableEntity>()
                //    .In(a => a.ObjectId, roles)
                //    .Where(a => a.ItemType == "form")
                //    .Select(a => a.ItemId).ToListAsync();
                //if (disableItems.IsAny())
                //{
                //    items = items.Except(disableItems).ToList();
                //}
                //var items = await _authorizeService.GetCurrentUserAuthorizeList("form");

                var forms = await _repository.AsQueryable()
                    //.Where(a => items.Contains(a.Id))
                        .Where(a => SqlFunc.Subqueryable<AuthorizeEntity>().Where(it => it.ItemId == a.Id && roles.Contains(it.ObjectId) && it.ItemType == "form").Any())
                        .Where(a => SqlFunc.Subqueryable<AuthorizeDisableEntity>().Where(it => it.ItemId == a.Id && roles.Contains(it.ObjectId) && it.ItemType == "form").NotAny())
                    .Where(a => a.EnabledMark == 1 && a.DeleteMark == null)
                    .Select<ModuleFormEntity>().OrderBy(q => q.SortCode)
                    .ToListAsync();
                output = forms.Adapt<List<ModuleFormOutput>>();
            }
        }
        else
        {
            var forms = await _repository.AsQueryable().Where(a => a.EnabledMark == 1 && a.DeleteMark == null).Select<ModuleFormEntity>().OrderBy(q => q.SortCode).ToListAsync();
            output = forms.Adapt<List<ModuleFormOutput>>();
        }

        return output;
    }

    #endregion
}