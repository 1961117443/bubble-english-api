using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using QT.VisualDev.Engine.Core;
using QT.VisualDev.Entitys;
using QT.VisualDev.Interfaces;
using QT.WorkFlow.Entitys.Dto.FlowEngine;
using QT.WorkFlow.Entitys.Entity;
using QT.WorkFlow.Entitys.Model;
using QT.WorkFlow.Entitys.Model.Properties;
using QT.WorkFlow.Interfaces.Repository;
using QT.WorkFlow.Interfaces.Service;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.WorkFlow.Service;

/// <summary>
/// 流程设计.
/// </summary>
[ApiDescriptionSettings(Tag = "WorkflowEngine", Name = "FlowEngine", Order = 301)]
[Route("api/workflow/Engine/[controller]")]
public class FlowEngineService : IFlowEngineService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<FlowEngineEntity> _flowEngineRepository;
    private readonly IFlowTaskRepository _flowTaskRepository;
    private readonly IDictionaryDataService _dictionaryDataService;
    private readonly IRunService _runService;
    private readonly IVisualDevService _visualDevService;
    private readonly IUserManager _userManager;
    private readonly IFileManager _fileManager;
    private readonly IDataBaseManager _dataBaseManager;
    private readonly ITenant _db;

    public FlowEngineService(
        ISqlSugarRepository<FlowEngineEntity> flowEngineRepository,
        IFlowTaskRepository flowTaskRepository,
        IDictionaryDataService dictionaryDataService,
        IRunService runService,
        IVisualDevService visualDevService,
        IUserManager userManager,
        IFileManager fileManager,
        IDataBaseManager dataBaseManager,
        ISqlSugarClient context)
    {
        _flowEngineRepository = flowEngineRepository;
        _flowTaskRepository = flowTaskRepository;
        _dictionaryDataService = dictionaryDataService;
        _runService = runService;
        _visualDevService = visualDevService;
        _userManager = userManager;
        _fileManager = fileManager;
        _dataBaseManager = dataBaseManager;
        _db = context.AsTenant();
    }

    #region GET

    /// <summary>
    /// 列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] FlowEngineListInput input)
    {
        var list = await _flowEngineRepository.Context.Queryable<FlowEngineEntity, DictionaryDataEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.Category == b.EnCode))
            .Where((a, b) => a.DeleteMark == null && b.DictionaryTypeId == "507f4f5df86b47588138f321e0b0dac7")
            .Where(a => !(a.FormType == 2 && a.Type == 1))
            .WhereIF(input.category.IsNotEmptyOrNull(), a => a.Category == input.category)
            .WhereIF(input.keyword.IsNotEmptyOrNull(), a => a.FullName.Contains(input.keyword) || a.EnCode.Contains(input.keyword))
            .Select((a, b) => new FlowEngineListAllOutput
            {
                category = b.FullName,
                id = a.Id,
                description = a.Description,
                creatorTime = a.CreatorTime,
                creatorUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.CreatorUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                enCode = a.EnCode,
                enabledMark = a.EnabledMark,
                flowTemplateJson = a.FlowTemplateJson,
                formData = a.FormTemplateJson,
                fullName = a.FullName,
                formType = a.FormType,
                icon = a.Icon,
                iconBackground = a.IconBackground,
                lastModifyTime = a.LastModifyTime,
                lastModifyUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.LastModifyUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                sortCode = a.SortCode,
                type = a.Type,
                visibleType = a.VisibleType,
            }).MergeTable().OrderBy(a => a.sortCode).OrderBy(a => a.creatorTime, OrderByType.Desc)
            .OrderByIF(!string.IsNullOrEmpty(input.keyword), t => t.lastModifyTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<FlowEngineListAllOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 列表(树形).
    /// </summary>
    /// <returns></returns>
    [HttpGet("ListAll")]
    public async Task<dynamic> GetListAll()
    {
        var list1 = await GetFlowFormList();
        var dicDataInfo = await _dictionaryDataService.GetInfo(list1.First().parentId);
        var dicDataList = await _dictionaryDataService.GetList(dicDataInfo.DictionaryTypeId);
        var list2 = new List<FlowEngineListOutput>();
        foreach (var item in dicDataList)
        {
            list2.Add(new FlowEngineListOutput()
            {
                fullName = item.FullName,
                parentId = "0",
                id = item.Id,
                num = list1.FindAll(x => x.category == item.EnCode).Count
            });
        }

        var output = list1.Union(list2).ToList().ToTree();
        return new { list = output };
    }

    /// <summary>
    /// 列表(分页).
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("PageListAll")]
    public async Task<dynamic> GetListPageAll([FromQuery] FlowEngineListInput input)
    {
        var data = await GetFlowFormList();
        if (input.category.IsNotEmptyOrNull())
            data = data.FindAll(x => x.category == input.category);
        if (input.keyword.IsNotEmptyOrNull())
            data = data.FindAll(o => o.fullName.Contains(input.keyword) || o.enCode.Contains(input.keyword));
        var pageList = new SqlSugarPagedList<FlowEngineListOutput>()
        {
            list = data.Skip(input.currentPage - 1).Take(input.pageSize).ToList(),
            pagination = new PagedModel()
            {
                PageIndex = input.currentPage,
                PageSize = input.pageSize,
                Total = data.Count
            }
        };
        return PageResult<FlowEngineListOutput>.SqlSugarPageResult(pageList);
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        return (await GetInfo(id)).Adapt<FlowEngineInfoOutput>();
    }

    /// <summary>
    /// 列表(子流程选择流程).
    /// </summary>
    /// <param name="type">(预留字段).</param>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> ListSelect([FromQuery] int type)
    {
        var list1 = await GetOutList();
        if (type.IsEmpty())
            list1 = list1.FindAll(x => x.formType == type);
        var dicDataInfo = await _dictionaryDataService.GetInfo(list1.First().parentId);
        var dicDataList = (await _dictionaryDataService.GetList(dicDataInfo.DictionaryTypeId)).FindAll(x => x.EnabledMark == 1);
        var list2 = new List<FlowEngineListOutput>();
        foreach (var item in dicDataList)
        {
            var index = list1.FindAll(x => x.category == item.EnCode).Count;
            if (index > 0)
            {
                list2.Add(new FlowEngineListOutput()
                {
                    fullName = item.FullName,
                    parentId = "0",
                    id = item.Id,
                    num = index
                });
            }
        }

        var output = list1.Union(list2).ToList().ToTree();
        return new { list = output };
    }

    /// <summary>
    /// 表单主表属性.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [HttpGet("{id}/FormDataFields")]
    public async Task<dynamic> getFormDataField(string id)
    {
        var entity = await GetInfo(id);
        List<FlowEngineFieldOutput> formDataFieldList = new List<FlowEngineFieldOutput>();
        if (entity.FormType == 1)
        {
            var dicList = entity.FormTemplateJson.ToList<Dictionary<string, object>>();
            formDataFieldList = dicList.Select(x =>
                new FlowEngineFieldOutput()
                {
                    vmodel = x.ContainsKey("filedId") ? x["filedId"].ToString() : string.Empty,
                    label = x.ContainsKey("filedName") ? x["filedName"].ToString() : string.Empty
                }).ToList();
        }
        else
        {
            var formTemplateBase = new TemplateParsingBase(entity.FormTemplateJson, entity.Tables, true);
            formDataFieldList = formTemplateBase.SingleFormData
                .Where(x => x.__config__.qtKey != "relationForm" && x.__config__.qtKey != "relationFlow")
                .Select(x => new FlowEngineFieldOutput() { vmodel = x.__vModel__, label = x.__config__.label }).ToList();
        }

        return new { list = formDataFieldList };
    }

    /// <summary>
    /// 表单列表.
    /// </summary>
    /// <param name="id">流程id.</param>
    /// <returns></returns>
    [HttpGet("{id}/FieldDataSelect")]
    public async Task<dynamic> getFormData(string id)
    {
        var flowTaskList = await _flowTaskRepository.GetTaskList(id);
        return flowTaskList.Select(x => new FlowEngineListSelectOutput()
        {
            id = x.Id,
            fullName = SqlFunc.MergeString(x.FullName, "/", x.EnCode)
        }).ToList();
    }

    /// <summary>
    /// 导出.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/Actions/ExportData")]
    public async Task<dynamic> ActionsExport(string id)
    {
        var importModel = new FlowEngineImportOutput();
        importModel.flowEngine = await _flowEngineRepository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        importModel.visibleList = await _flowEngineRepository.Context.Queryable<FlowEngineVisibleEntity>().Where(x => x.FlowId == id).ToListAsync();
        var jsonStr = importModel.ToJsonString();
        return await _fileManager.Export(jsonStr, importModel.flowEngine.FullName, ExportFileType.ffe);
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
        var flowEngineEntity = await GetInfo(id);
        if (flowEngineEntity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        if (await _flowTaskRepository.AnyFlowTask(x => x.DeleteMark == null && x.FlowId == id))
            throw Oops.Oh(ErrorCode.WF0024);
        _db.BeginTran();
        await _flowEngineRepository.Context.Deleteable<FlowEngineVisibleEntity>(a => a.FlowId == flowEngineEntity.Id).ExecuteCommandHasChangeAsync();
        var isOk = await _flowEngineRepository.Context.Updateable(flowEngineEntity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandHasChangeAsync();
        _db.CommitTran();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] FlowEngineCrInput input)
    {
        if (await _flowEngineRepository.AnyAsync(x => (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        if (input.formType == 2)
        {
            var formTemplateBase = new TemplateParsingBase(input.formData, input.tables, true);
            if (!formTemplateBase.VerifyTemplate())
                throw Oops.Oh(ErrorCode.D1401);
        }
        var flowEngineEntity = input.Adapt<FlowEngineEntity>();
        flowEngineEntity.Version = "1";
        var flowVisibleList = GetFlowEngineVisibleList(input.flowTemplateJson);
        var result = await Create(flowEngineEntity, flowVisibleList);

        _ = result ?? throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] FlowEngineUpInput input)
    {
        if (await _flowEngineRepository.AnyAsync(x => x.Id != id && (x.EnCode == input.enCode || x.FullName == input.fullName) && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        if (input.formType == 2)
        {
            var formTemplateBase = new TemplateParsingBase(input.formData, input.tables, true);
            if (!formTemplateBase.VerifyTemplate())
                throw Oops.Oh(ErrorCode.D1401);
        }

        var flowEngineEntity = input.Adapt<FlowEngineEntity>();
        flowEngineEntity.Version = ((await _flowEngineRepository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null)).Version.ParseToInt() + 1).ToString();
        var flowVisibleList = GetFlowEngineVisibleList(input.flowTemplateJson);
        var isOk = await Update(flowEngineEntity, flowVisibleList);
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 复制.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Copy")]
    public async Task ActionsCopy(string id)
    {
        var entity = await GetInfo(id);
        var random = RandomExtensions.NextLetterAndNumberString(new Random(), 5).ToLower();
        entity.FullName = string.Format("{0}副本{1}", entity.FullName, random);
        entity.EnCode = string.Format("{0}{1}", entity.EnCode, random);
        entity.Version = "1";
        if (entity.FullName.Length >= 50 || entity.EnCode.Length >= 50)
            throw Oops.Oh(ErrorCode.COM1009);
        var flowVisibleList = GetFlowEngineVisibleList(entity.FlowTemplateJson);
        var result = await Create(entity, flowVisibleList);
        _ = result ?? throw Oops.Oh(ErrorCode.WF0002);
    }

    /// <summary>
    /// 导入.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("Actions/ImportData")]
    public async Task ActionsImport(IFormFile file)
    {
        var fileType = Path.GetExtension(file.FileName).Replace(".", string.Empty);
        if (!fileType.ToLower().Equals(ExportFileType.ffe.ToString()))
            throw Oops.Oh(ErrorCode.D3006);
        var josn = _fileManager.Import(file);
        FlowEngineImportOutput model;
        try
        {
            model = josn.ToObject<FlowEngineImportOutput>();
        }
        catch
        {
            throw Oops.Oh(ErrorCode.D3006);
        }

        if (model == null)
            throw Oops.Oh(ErrorCode.D3006);
        await ImportData(model);
    }

    /// <summary>
    /// 发布.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPost("Release/{id}")]
    public async Task Release(string id)
    {
        var entity = await GetInfo(id);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _flowEngineRepository.Context.Updateable<FlowEngineEntity>().SetColumns(it => it.EnabledMark == 1).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1003);
    }

    /// <summary>
    /// 停止.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPost("Stop/{id}")]
    public async Task Stop(string id)
    {
        var entity = await GetInfo(id);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        var isOk = await _flowEngineRepository.Context.Updateable<FlowEngineEntity>().SetColumns(it => it.EnabledMark == 0).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1003);
    }
    #endregion

    #region PublicMethod

    /// <summary>
    /// 发起列表.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<FlowEngineListOutput>> GetFlowFormList()
    {
        var list = await GetOutList();
        if (_userManager.User.IsAdministrator == 0)
        {
            var data = await GetVisibleFlowList();
            data = data.Union(list.FindAll(x => x.visibleType == 0)).ToList();

            return data;
        }
        else
        {
            return list;
        }
    }
    #endregion

    #region PrivateMethod

    /// <summary>
    /// 详情.
    /// </summary>
    /// <param name="id">主键.</param>
    /// <returns></returns>
    [NonAction]
    private async Task<FlowEngineEntity> GetInfo(string id)
    {
        return await _flowEngineRepository.FirstOrDefaultAsync(a => a.Id == id && a.DeleteMark == null);
    }

    /// <summary>
    /// 新增流程.
    /// </summary>
    /// <param name="entity">流程实例.</param>
    /// <param name="visibleList">可见范围.</param>
    /// <returns></returns>
    [NonAction]
    private async Task<FlowEngineEntity> Create(FlowEngineEntity entity, List<FlowEngineVisibleEntity> visibleList)
    {
        try
        {
            _db.BeginTran();
            entity.VisibleType = visibleList.Count == 0 ? 0 : 1;
            entity.Id = SnowflakeIdHelper.NextId();

            foreach (var item in visibleList)
            {
                item.FlowId = entity.Id;
                item.SortCode = visibleList.IndexOf(item);
            }
            if (visibleList.Count > 0)
                await _flowEngineRepository.Context.Insertable(visibleList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();

            if (entity.FormType == 2)
            {
                // 无表转有表
                if (entity.Tables.IsNullOrEmpty() || entity.Tables == "[]")
                {
                    var random = RandomExtensions.NextLetterAndNumberString(new Random(), 5).ToLower();
                    // 主表名称
                    var mTableName = "wform_" + entity.EnCode + "_" + random;
                    var devEntity = new VisualDevEntity()
                    {
                        FlowId = entity.Id,
                        FormData = entity.FormTemplateJson,
                    };
                    var res = await _visualDevService.NoTblToTable(devEntity, mTableName);
                    entity.Tables = res.Tables;
                    entity.FormTemplateJson = res.FormData;
                }
            }
            var result = await _flowEngineRepository.Context.Insertable(entity).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
            if (result == null)
                throw Oops.Oh(ErrorCode.COM1005);
            _db.CommitTran();
            return result;
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            return null;
        }
    }

    /// <summary>
    /// 修改流程.
    /// </summary>
    /// <param name="entity">流程实体.</param>
    /// <param name="visibleList">可见范围.</param>
    /// <returns></returns>
    [NonAction]
    private async Task<bool> Update(FlowEngineEntity entity, List<FlowEngineVisibleEntity> visibleList)
    {
        try
        {
            _db.BeginTran();
            entity.VisibleType = visibleList.Count == 0 ? 0 : 1;
            await _flowEngineRepository.Context.Deleteable<FlowEngineVisibleEntity>(a => a.FlowId == entity.Id).ExecuteCommandHasChangeAsync();
            foreach (var item in visibleList)
            {
                item.FlowId = entity.Id;
                item.SortCode = visibleList.IndexOf(item);
            }
            if (visibleList.Count > 0)
                await _flowEngineRepository.Context.Insertable(visibleList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();

            if (entity.FormType == 2)
            {
                #region  处理旧无表数据
                //无表转有表
                var mTableName = "wform_" + entity.EnCode;//主表名称
                if (entity.Tables.IsNullOrEmpty() || entity.Tables == "[]")
                {
                    var devEntity = new VisualDevEntity()
                    {
                        FlowId = entity.Id,
                        FormData = entity.FormTemplateJson
                    };
                    var res = await _visualDevService.NoTblToTable(devEntity, mTableName);
                    entity.Tables = res.Tables;
                    entity.FormTemplateJson = res.FormData;
                    var entityData = await _runService.GetInfo(entity.Id);
                    if (entityData != null && entityData.Data != null)
                    {
                        var sqlList = _visualDevService.DataToInsertSql(mTableName, entityData.Data);
                        if (sqlList.Any())
                        {
                            var link = await _flowTaskRepository.GetLinkInfo(entity.DbLinkId) ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
                            foreach (var item in sqlList)
                            {
                                await _dataBaseManager.ExecuteSql(link, item);
                            }
                        }
                    }
                }
                #endregion
            }
            var isOk = await _flowEngineRepository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
            _db.CommitTran();
            return isOk;
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            return false;
        }
    }

    /// <summary>
    /// 解析流程可见参数.
    /// </summary>
    /// <param name="josnStr"></param>
    /// <returns></returns>
    private List<FlowEngineVisibleEntity> GetFlowEngineVisibleList(string josnStr)
    {
        var output = new List<FlowEngineVisibleEntity>();

        // 发起节点属性.
        var pro = josnStr.ToObject<FlowTemplateJsonModel>().properties.ToObject<StartProperties>();
        if (pro.initiator != null && pro.initiator.Count > 0)
        {
            var list = pro.initiator.Select(x => new FlowEngineVisibleEntity() { OperatorId = x, OperatorType = "user" }).ToList();
            output.AddRange(list);
        }

        if (pro.initiatePos != null && pro.initiatePos.Count > 0)
        {
            var list = pro.initiatePos.Select(x => new FlowEngineVisibleEntity() { OperatorId = x, OperatorType = "Position" }).ToList();
            output.AddRange(list);
        }

        if (pro.initiateRole != null && pro.initiateRole.Count > 0)
        {
            var list = pro.initiateRole.Select(x => new FlowEngineVisibleEntity() { OperatorId = x, OperatorType = "Role" }).ToList();
            output.AddRange(list);
        }

        return output;
    }

    /// <summary>
    /// 获取当前用户可见流程.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    private async Task<List<FlowEngineListOutput>> GetVisibleFlowList()
    {
        return await _flowEngineRepository.Context.Queryable<FlowEngineVisibleEntity, UserRelationEntity, FlowEngineEntity>((a, b, c) => new JoinQueryInfos(JoinType.Left, a.OperatorId == b.ObjectId, JoinType.Left, a.FlowId == c.Id))
            .Where((a, b, c) => (a.OperatorId == _userManager.UserId || b.UserId == _userManager.UserId) && c.DeleteMark == null && c.EnabledMark == 1 && c.Type == 0)
            .Select((a, b, c) => new FlowEngineListOutput
            {
                category = c.Category,
                id = c.Id,
                description = c.Description,
                creatorTime = c.CreatorTime,
                creatorUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == c.CreatorUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                enCode = c.EnCode,
                enabledMark = c.EnabledMark,
                //flowTemplateJson = c.FlowTemplateJson,
                //formData = c.FormTemplateJson,
                fullName = c.FullName,
                formType = c.FormType,
                icon = c.Icon,
                iconBackground = c.IconBackground,
                lastModifyTime = c.LastModifyTime,
                lastModifyUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == c.LastModifyUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                sortCode = a.SortCode,
                type = c.Type,
                visibleType = c.VisibleType,
                parentId = SqlFunc.Subqueryable<DictionaryDataEntity>().Where(d => d.EnCode == c.Category && d.DictionaryTypeId== "507f4f5df86b47588138f321e0b0dac7").Select(d => d.Id),
            }).MergeTable().OrderBy(a => a.sortCode).OrderBy(a => a.creatorTime, OrderByType.Desc)
             .OrderBy(a => a.lastModifyTime, OrderByType.Desc).ToListAsync();
    }

    /// <summary>
    /// 流程列表(功能流程不显示).
    /// </summary>
    /// <returns></returns>
    private async Task<List<FlowEngineListOutput>> GetOutList()
    {
        return await _flowEngineRepository.Context.Queryable<FlowEngineEntity, DictionaryDataEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.Category == b.EnCode))
             .Where((a, b) => a.DeleteMark == null && a.EnabledMark == 1 && a.Type == 0 && b.DictionaryTypeId == "507f4f5df86b47588138f321e0b0dac7")
             .Select((a, b) => new FlowEngineListOutput
             {
                 category = a.Category,
                 id = a.Id,
                 description = a.Description,
                 creatorTime = a.CreatorTime,
                 creatorUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.CreatorUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                 enCode = a.EnCode,
                 enabledMark = a.EnabledMark,
                 //flowTemplateJson = a.FlowTemplateJson, //加了查询慢很多
                 //formData = a.FormTemplateJson,
                 fullName = a.FullName,
                 formType = a.FormType,
                 icon = a.Icon,
                 iconBackground = a.IconBackground,
                 lastModifyTime = a.LastModifyTime,
                 lastModifyUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.LastModifyUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                 sortCode = a.SortCode,
                 type = a.Type,
                 visibleType = a.VisibleType,
                 parentId = b.Id
             }).MergeTable().OrderBy(a => a.sortCode).OrderBy(a => a.creatorTime, OrderByType.Desc)
             .OrderBy(a => a.lastModifyTime, OrderByType.Desc).ToListAsync();
    }

    /// <summary>
    /// 导入数据.
    /// </summary>
    /// <param name="model">导入实例.</param>
    /// <returns></returns>
    private async Task ImportData(FlowEngineImportOutput model)
    {
        try
        {
            _db.BeginTran();

            // 存在更新不存在插入 根据主键
            var stor = _flowEngineRepository.Context.Storageable(model.flowEngine).Saveable().ToStorage();

            // 执行插入
            await stor.AsInsertable.ExecuteCommandAsync();

            // await stor.AsUpdateable.ExecuteCommandAsync(); //执行更新，停用原因：Oracle 数据库环境会抛异常：ora-01704: 字符串文字太长
            // 执行更新
            await _flowEngineRepository.Context.Updateable(model.flowEngine).ExecuteCommandAsync();

            // 存在更新不存在插入 根据主键
            var stor1 = _flowEngineRepository.Context.Storageable(model.visibleList).Saveable().ToStorage();

            // 执行插入
            await stor1.AsInsertable.ExecuteCommandAsync();

            // 执行更新
            await stor1.AsUpdateable.ExecuteCommandAsync();

            _db.CommitTran();
        }
        catch (Exception ex)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.D3006);
        }
    }
    #endregion
}
