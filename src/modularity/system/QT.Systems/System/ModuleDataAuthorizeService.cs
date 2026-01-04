using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.ModuleDataAuthorize;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using QT.VisualDev.Engine;
using QT.VisualDev.Engine.Core;
using QT.VisualDev.Entitys;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SqlSugar;

namespace QT.Systems;

/// <summary>
/// 数据权限



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "ModuleDataAuthorize", Order = 214)]
[Route("api/system/[controller]")]
public class ModuleDataAuthorizeService : IModuleDataAuthorizeService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ModuleDataAuthorizeEntity> _repository;

    /// <summary>
    /// 用户管理器.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 初始化一个<see cref="ModuleDataAuthorizeService"/>类型的新实例.
    /// </summary>
    public ModuleDataAuthorizeService(
        ISqlSugarRepository<ModuleDataAuthorizeEntity> moduleDataAuthorizeRepository,
        IUserManager userManager)
    {
        _repository = moduleDataAuthorizeRepository;
        _userManager = userManager;
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
        var list = await _repository.Context
                .Queryable<ModuleDataAuthorizeEntity, ModuleEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.ModuleId == b.Id))
                .Where((a, b) => a.ModuleId == moduleId && a.DeleteMark == null && b.DeleteMark == null)
                .WhereIF(input.keyword.IsNotEmptyOrNull(), a => a.EnCode.Contains(input.keyword) || a.FullName.Contains(input.keyword))
                .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc).OrderBy(a => a.LastModifyTime, OrderByType.Desc)
                .Select((a, b) => new ModuleDataAuthorizeListOutput()
                {
                    id = a.Id,
                    fullName = a.FullName,
                    type = a.Type,
                    conditionSymbol = a.ConditionSymbol,
                    conditionText = a.ConditionText,
                    conditionSymbolName = a.ConditionSymbol.Replace("Equal", "等于").Replace("GreaterThan", "大于").Replace("LessThan", "小于").Replace("Not", "不").Replace("Or", ""),
                    bindTable = a.BindTable,
                    fieldRule = a.FieldRule,
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
    public async Task<dynamic> GetInfo_Api(string id)
    {
        var data = await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        if (await _repository.Context.Queryable<ModuleEntity>().AnyAsync(x => x.Id == data.ModuleId && x.DeleteMark == null && x.Type == 2) && data.FieldRule == 1 && data.BindTable.IsNotEmptyOrNull())
            data.EnCode = data.EnCode.Replace("jnpf_" + data.BindTable + "_jnpf_", string.Empty);
        return data.Adapt<ModuleDataAuthorizeInfoOutput>();
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
        var tInfo = new TemplateParsingBase(visualDevEntity);
        return tInfo.SingleFormData.Select(x => new { field = x.__vModel__, fieldName = x.__config__.label });
    }
    #endregion

    #region POST

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ModuleDataAuthorizeCrInput input)
    {
        var entity = input.Adapt<ModuleDataAuthorizeEntity>();
        if (await _repository.Context.Queryable<ModuleEntity>().AnyAsync(x => x.Id == input.moduleId && x.DeleteMark == null && x.Type == 2) && entity.FieldRule == 1 && entity.BindTable.IsNotEmptyOrNull())
            entity.EnCode = "jnpf_" + input.bindTable + "_jnpf_" + entity.EnCode;
        //if (await _repository.AnyAsync(x => x.EnCode.Equals(entity.EnCode) && x.DeleteMark == null && x.ModuleId == input.moduleId))
        //    throw Oops.Oh(ErrorCode.COM1004);
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
    public async Task Update(string id, [FromBody] ModuleDataAuthorizeUpInput input)
    {
        var entity = input.Adapt<ModuleDataAuthorizeEntity>();
        if (await _repository.Context.Queryable<ModuleEntity>().AnyAsync(x => x.Id == input.moduleId && x.DeleteMark == null && x.Type == 2) && entity.FieldRule == 1 && entity.BindTable.IsNotEmptyOrNull())
            entity.EnCode = "jnpf_" + input.bindTable + "_jnpf_" + entity.EnCode;
        //if (await _repository.AnyAsync(x => x.EnCode.Equals(entity.EnCode) && x.DeleteMark == null && x.ModuleId == input.moduleId && x.Id != entity.Id))
        //    throw Oops.Oh(ErrorCode.COM1004);
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
        if (!await _repository.AnyAsync(x => x.Id == id && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _repository.Context.Updateable<ModuleDataAuthorizeEntity>().SetColumns(it => new ModuleDataAuthorizeEntity()
        {
            DeleteMark = 1,
            DeleteUserId = _userManager.UserId,
            DeleteTime = SqlFunc.GetDate()
        }).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="moduleId">功能主键.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<ModuleDataAuthorizeEntity>> GetList(string? moduleId = default)
    {
        return await _repository.AsQueryable().Where(x => x.DeleteMark == null).WhereIF(!moduleId.IsNullOrEmpty(), it => it.ModuleId == moduleId).OrderBy(o => o.SortCode).ToListAsync();
    }
    #endregion
}