using QT.Apps.Entitys.Dto;
using QT.Apps.Entitys;
using QT.Apps.Interfaces;
using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.WorkFlow.Interfaces.Service;
using QT.WorkFlow.Entitys.Dto.FlowEngine;
using QT.Common.Filter;
using QT.Common.Extension;

namespace QT.Apps;

/// <summary>
/// App常用数据
/// </summary>
[ApiDescriptionSettings(Tag = "App", Name = "Data", Order = 800)]
[Route("api/App/[controller]")]
public class AppDataService : IAppDataService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<AppDataEntity> _repository; // App常用数据

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 字典数据.
    /// </summary>
    private readonly IDictionaryDataService _dictionaryDataService;

    /// <summary>
    /// 流程管理.
    /// </summary>
    private readonly IFlowEngineService _flowEngineService;

    /// <summary>
    /// 构造.
    /// </summary>
    /// <param name="appDataRepository"></param>
    /// <param name="userManager"></param>
    /// <param name="dictionaryDataService"></param>
    /// <param name="flowEngineService"></param>
    public AppDataService(
        ISqlSugarRepository<AppDataEntity> appDataRepository,
        IUserManager userManager,
        IDictionaryDataService dictionaryDataService,
        IFlowEngineService flowEngineService)
    {
        _repository = appDataRepository;
        _userManager = userManager;
        _dictionaryDataService = dictionaryDataService;
        _flowEngineService = flowEngineService;
    }

    #region Get

    /// <summary>
    /// 常用数据.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] string type)
    {
        List<AppDataEntity>? list = await GetListByType(type);
        List<AppDataListOutput>? output = list.Adapt<List<AppDataListOutput>>();
        return new { list = output };
    }

    /// <summary>
    /// 所有流程.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getFlowList")]
    public async Task<dynamic> GetFlowList([FromQuery] FlowEngineListInput input)
    {
        List<FlowEngineListOutput> list1 = await _flowEngineService.GetFlowFormList();
        if (input.category.IsNotEmptyOrNull())
            list1 = list1.FindAll(x => x.category == input.category);
        if (input.keyword.IsNotEmptyOrNull())
            list1 = list1.FindAll(o => o.fullName.Contains(input.keyword) || o.enCode.Contains(input.keyword));

        List<AppFlowListAllOutput>? appList = list1.Adapt<List<AppFlowListAllOutput>>();
        foreach (AppFlowListAllOutput? item in appList)
        {
            item.isData = _repository.Any(x => x.ObjectType == "1" && x.CreatorUserId == _userManager.UserId && x.ObjectId == item.id && x.DeleteMark == null);
        }

        var pageList = new SqlSugarPagedList<AppFlowListAllOutput>()
        {
            list = appList.Skip(input.currentPage - 1).Take(input.pageSize).ToList(),
            pagination = new PagedModel()
            {
                PageIndex = input.currentPage,
                PageSize = input.pageSize,
                Total = appList.Count
            }
        };
        return PageResult<AppFlowListAllOutput>.SqlSugarPageResult(pageList);
    }

    /// <summary>
    /// 所有流程.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getDataList")]
    public async Task<dynamic> GetDataList(string keyword)
    {
        List<AppDataListAllOutput>? list = (await GetAppMenuList(keyword)).Adapt<List<AppDataListAllOutput>>();
        foreach (AppDataListAllOutput? item in list)
        {
            item.isData = _repository.Any(x => x.ObjectType == "2" && x.CreatorUserId == _userManager.UserId && x.ObjectId == item.id && x.DeleteMark == null);
        }

        List<AppDataListAllOutput>? output = list.ToTree("-1");
        return new { list = output };
    }
    #endregion

    #region Post

    /// <summary>
    /// 新增.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] AppDataCrInput input)
    {
        AppDataEntity? entity = input.Adapt<AppDataEntity>();
        int isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="objectId"></param>
    /// <returns></returns>
    [HttpDelete("{objectId}")]
    public async Task Delete(string objectId)
    {
        AppDataEntity? entity = await _repository.SingleAsync(x => x.ObjectId == objectId && x.CreatorUserId == _userManager.UserId && x.DeleteMark == null);
        var isOk = await _repository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private async Task<List<AppDataEntity>> GetListByType(string type)
    {
        return await _repository.AsQueryable().Where(x => x.ObjectType == type && x.CreatorUserId == _userManager.UserId && x.DeleteMark == null).ToListAsync();
    }

    /// <summary>
    /// 菜单列表.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<ModuleEntity>> GetAppMenuList(string keyword)
    {
        List<ModuleEntity>? menuList = new List<ModuleEntity>();
        if (_userManager.IsAdministrator)
        {
            menuList = await _repository.Context.Queryable<ModuleEntity>()
                .Where(x => x.EnabledMark == 1 && x.Category == "App" && x.DeleteMark == null)
                .WhereIF(!string.IsNullOrEmpty(keyword), x => x.FullName.Contains(keyword) || x.ParentId == "-1")
                .ToListAsync();
        }
        else
        {
            string[]? objectIds = (await _userManager.GetUserInfo()).roleIds;
            if (objectIds.Length == 0)
                return menuList;
            List<string>? ids = await _repository.Context.Queryable<AuthorizeEntity>()
                .Where(x => objectIds.Contains(x.ObjectId) && x.ObjectType == "Role" && x.ItemType == "module").Select(x => x.ItemId).ToListAsync();
            if (ids.Count == 0)
                return menuList;
            menuList = await _repository.Context.Queryable<ModuleEntity>()
                .Where(x => ids.Contains(x.Id) && x.EnabledMark == 1 && x.Category == "App" && x.DeleteMark == null)
                .WhereIF(!string.IsNullOrEmpty(keyword), x => x.FullName.Contains(keyword) || x.ParentId == "-1")
                .ToListAsync();
        }

        return menuList;
    }

    #endregion
}