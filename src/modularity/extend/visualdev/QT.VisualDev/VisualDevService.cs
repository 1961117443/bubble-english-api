using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Options;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Model.DataBase;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using QT.VisualDev.Engine;
using QT.VisualDev.Engine.Core;
using QT.VisualDev.Engine.Model;
using QT.VisualDev.Engine.Security;
using QT.VisualDev.Entitys;
using QT.VisualDev.Entitys.Dto.VisualDev;
using QT.VisualDev.Entitys.Dto.VisualDevModelData;
using QT.VisualDev.Interfaces;
using QT.WorkFlow.Entitys.Entity;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Yitter.IdGenerator;
using QT.DistributedIDGenerator;
using QT.Common.Core.Security;
using QT.Common.Configuration;
using QT.Common.Core.Manager.Tenant;
using QT.RemoteRequest.Extensions;
using QT.UnifyResult;

namespace QT.VisualDev;

/// <summary>
/// 可视化开发基础.
/// </summary>
[ApiDescriptionSettings(Tag = "VisualDev", Name = "Base", Order = 171)]
[Route("api/visualdev/[controller]")]
public class VisualDevService : IVisualDevService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<VisualDevEntity> _visualDevRepository;

    /// <summary>
    /// 字典服务.
    /// </summary>
    private readonly IDictionaryDataService _dictionaryDataService;

    /// <summary>
    /// 切库.
    /// </summary>
    private readonly IDataBaseManager _changeDataBase;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 在线开发运行服务.
    /// </summary>
    private readonly IRunService _runService;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="VisualDevService"/>类型的新实例.
    /// </summary>
    public VisualDevService(
        ISqlSugarRepository<VisualDevEntity> visualDevRepository,
        IDataBaseManager changeDataBase,
        IUserManager userManager,
        IRunService runService,
        IDictionaryDataService dictionaryDataService,
        ISqlSugarClient context)
    {
        _visualDevRepository = visualDevRepository;
        _dictionaryDataService = dictionaryDataService;
        _userManager = userManager;
        _runService = runService;
        _changeDataBase = changeDataBase;
        _db = context.AsTenant();
    }

    #region Get

    /// <summary>
    /// 获取功能列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] VisualDevListQueryInput input)
    {
        SqlSugarPagedList<VisualDevListOutput>? data = await _visualDevRepository.Context.Queryable<VisualDevEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.FullName.Contains(input.keyword) || a.EnCode.Contains(input.keyword))
            .WhereIF(!string.IsNullOrEmpty(input.category), a => a.Category == input.category)
            .Where(a => a.DeleteMark == null && a.Type == input.type)
            .OrderBy(a => a.SortCode, OrderByType.Asc)
            .OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select(a => new VisualDevListOutput
            {
                id = a.Id,
                fullName = a.FullName,
                enCode = a.EnCode,
                state = a.State,
                type = a.Type,
                webType = a.WebType,
                tables = a.Tables,
                description = a.Description,
                creatorTime = a.CreatorTime,
                lastModifyTime = a.LastModifyTime,
                deleteMark = a.DeleteMark,
                sortCode = a.SortCode,
                parentId = a.Category,
                pcIsRelease = SqlFunc.Subqueryable<ModuleEntity>().Where(m => m.ModuleId == a.Id && m.Category == "Web" && m.DeleteMark == null).Count(),
                appIsRelease = SqlFunc.Subqueryable<ModuleEntity>().Where(m => m.ModuleId == a.Id && m.Category == "App" && m.DeleteMark == null).Count(),
                category = SqlFunc.Subqueryable<DictionaryDataEntity>().Where(d => d.Id == a.Category).Select(d => d.FullName),
                creatorUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.CreatorUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                lastModifyUser = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.LastModifyUserId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account))
            }).ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<VisualDevListOutput>.SqlSugarPageResult(data);

    }

    /// <summary>
    /// 获取功能列表下拉框.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector([FromQuery] VisualDevSelectorInput input)
    {
        List<VisualDevEntity>? data = await _visualDevRepository.AsQueryable().Where(v => v.Type == input.type && v.State == 1 && v.DeleteMark == null).
            OrderBy(a => a.Category).OrderBy(a => a.SortCode).ToListAsync();
        List<VisualDevSelectorOutput>? output = data.Adapt<List<VisualDevSelectorOutput>>();
        IEnumerable<string>? parentIds = output.Select(x => x.parentId).ToList().Distinct();
        List<VisualDevSelectorOutput>? pList = new List<VisualDevSelectorOutput>();
        List<DictionaryDataEntity>? parentData = await _visualDevRepository.Context.Queryable<DictionaryDataEntity>().Where(d => parentIds.Contains(d.Id) && d.DeleteMark == null).OrderBy(x => x.SortCode).ToListAsync();
        foreach (DictionaryDataEntity? item in parentData)
        {
            VisualDevSelectorOutput? pData = item.Adapt<VisualDevSelectorOutput>();
            pData.parentId = "-1";
            pList.Add(pData);
        }

        return new { list = output.Union(pList).ToList().ToTree("-1") };
    }

    /// <summary>
    /// 获取功能信息.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        VisualDevEntity? data = await _visualDevRepository.AsQueryable().FirstAsync(v => v.Id == id && v.DeleteMark == null);
        return data.Adapt<VisualDevInfoOutput>();
    }

    /// <summary>
    /// 获取表单主表属性下拉框.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/FormDataFields")]
    public async Task<dynamic> GetFormDataFields(string id)
    {
        VisualDevEntity? templateEntity = await _visualDevRepository.AsQueryable().FirstAsync(v => v.Id == id && v.DeleteMark == null);
        TemplateParsingBase? tInfo = new TemplateParsingBase(templateEntity); // 解析模板
        List<FieldsModel>? fieldsModels = tInfo.SingleFormData.FindAll(x => x.__vModel__.IsNotEmptyOrNull() && !QtKeyConst.RELATIONFORM.Equals(x.__config__.qtKey));
        List<VisualDevFormDataFieldsOutput>? output = fieldsModels.Select(x => new VisualDevFormDataFieldsOutput()
        {
            label = x.__config__.label,
            vmodel = x.__vModel__
        }).ToList();
        return new { list = output };
    }

    /// <summary>
    /// 获取表单主表属性列表.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("{id}/FieldDataSelect")]
    public async Task<dynamic> GetFieldDataSelect(string id, [FromQuery] VisualDevDataFieldDataListInput input)
    {
        Dictionary<string, object> queryDic = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(input.relationField) && !string.IsNullOrWhiteSpace(input.keyword)) queryDic.Add(input.relationField, input.keyword);
        VisualDevEntity? templateEntity = await _visualDevRepository.AsQueryable().FirstAsync(v => v.Id == id && v.DeleteMark == null); // 取数据
        TemplateParsingBase? tInfo = new TemplateParsingBase(templateEntity); // 解析模板

        // 指定查询字段
        if (input.IsNotEmptyOrNull() && input.columnOptions.IsNotEmptyOrNull())
        {
            List<string>? showFieldList = input.columnOptions.Split(',').ToList(); // 显示的所有 字段
            List<FieldsModel>? flist = new List<FieldsModel>();
            List<IndexGridFieldModel>? clist = new List<IndexGridFieldModel>();

            // 获取 调用 该功能表单 的功能模板
            FieldsModel? smodel = tInfo.FieldsModelList.Where(x => x.__vModel__ == input.relationField).First();
            smodel.searchType = 2;
            flist.Add(smodel); // 添加 关联查询字段
            if (tInfo.ColumnData == null)
            {
                tInfo.ColumnData = new ColumnDesignModel()
                {
                    columnList = new List<IndexGridFieldModel>() { new IndexGridFieldModel() { prop = input.relationField, label = input.relationField } },
                    searchList = new List<IndexSearchFieldModel>() { smodel.Adapt<IndexSearchFieldModel>() }
                };
            }

            if (!tInfo.ColumnData.columnList.Where(x => x.prop == input.relationField).Any())
                tInfo.ColumnData.columnList.Add(new IndexGridFieldModel() { prop = input.relationField, label = input.relationField });
            if (tInfo.ColumnData.defaultSidx.IsNotEmptyOrNull() && tInfo.FieldsModelList.Any(x => x.__vModel__ == tInfo.ColumnData?.defaultSidx))
                flist.Add(tInfo.FieldsModelList.Where(x => x.__vModel__ == tInfo.ColumnData?.defaultSidx).FirstOrDefault()); // 添加 关联排序字段

            tInfo.FieldsModelList.ForEach(item =>
            {
                if (showFieldList.Find(x => x == item.__vModel__) != null) flist.Add(item);
            });
            clist.Add(tInfo.ColumnData.columnList.Where(x => x.prop == input.relationField).FirstOrDefault()); // 添加 关联查询字段
            if (tInfo.ColumnData.defaultSidx.IsNotEmptyOrNull() && tInfo.FieldsModelList.Any(x => x.__vModel__ == tInfo.ColumnData?.defaultSidx))
                clist.Add(tInfo.ColumnData.columnList.Where(x => x.prop == tInfo.ColumnData?.defaultSidx).FirstOrDefault()); // 添加 关联排序字段
            showFieldList.ForEach(item =>
            {
                if (!tInfo.ColumnData.columnList.Where(x => x.prop == item).Any())
                    clist.Add(new IndexGridFieldModel() { prop = item, label = item });
                else
                    clist.Add(tInfo.ColumnData.columnList.Find(x => x.prop == item));
            });

            if (flist.Count > 0)
            {
                tInfo.FormModel.fields = flist.Distinct().ToList();
                templateEntity.FormData = tInfo.FormModel.ToJsonString();
            }

            if (clist.Count > 0)
            {
                tInfo.ColumnData.columnList = clist.Distinct().ToList();
                templateEntity.ColumnData = tInfo.ColumnData.ToJsonString();
            }
        }

        // 获取值 无分页
        VisualDevModelListQueryInput listQueryInput = new VisualDevModelListQueryInput
        {
            queryJson = queryDic.ToJsonString(),
            currentPage = input.currentPage > 0 ? input.currentPage : 1,
            pageSize = input.pageSize > 0 ? input.pageSize : 20,
            dataType = "1",
            sidx = tInfo.ColumnData.defaultSidx,
            sort = tInfo.ColumnData.sort
        };

        return await _runService.GetRelationFormList(templateEntity, listQueryInput, "List");
    }

    #endregion

    #region Post

    /// <summary>
    /// 新建功能信息.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] VisualDevCrInput input)
    {
        VisualDevEntity? entity = input.Adapt<VisualDevEntity>();
        try
        {
            if (string.IsNullOrEmpty(entity.EnCode))
            {
                entity.EnCode = IDGen.NextID().ToString();
            }
            // 验证名称和编码是否重复
            if (await _visualDevRepository.AnyAsync(x => x.DeleteMark == null && (x.FullName == input.fullName || x.EnCode == input.enCode))) throw Oops.Oh(ErrorCode.D1406);

            TemplateParsingBase? tInfo = new TemplateParsingBase(entity); // 解析模板
            if(!tInfo.VerifyTemplate()) throw Oops.Oh(ErrorCode.D1401); // 验证模板
            _db.BeginTran(); // 开启事务

            if (input.webType == 3)
            {
                DictionaryDataEntity? categoryData = await _dictionaryDataService.GetInfo(entity.Category);
                FlowEngineEntity? flowEngine = new FlowEngineEntity();
                flowEngine.FlowTemplateJson = entity.FlowTemplateJson;
                flowEngine.EnCode = "#visualDev" + entity.EnCode;
                flowEngine.Type = 1;
                flowEngine.FormType = 2;
                flowEngine.FullName = entity.FullName;
                flowEngine.Category = categoryData.EnCode;
                flowEngine.VisibleType = 0;
                flowEngine.Icon = "icon-qt icon-qt-node";
                flowEngine.IconBackground = "#008cff";
                flowEngine.Tables = entity.Tables;
                flowEngine.DbLinkId = entity.DbLinkId;
                flowEngine.FormTemplateJson = entity.FormData;
                flowEngine.Version = "1";
                // 添加流程引擎
                FlowEngineEntity? engineEntity = await _visualDevRepository.Context.Insertable(flowEngine).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteReturnEntityAsync();
                entity.FlowId = engineEntity.Id;
                entity.Id = engineEntity.Id;
            }

            VisualDevEntity? visualDev = await _visualDevRepository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();

            // 无表转有表
            if (entity.Tables.IsNullOrEmpty() || entity.Tables == "[]")
            {
                string? mTableName = "mt" + entity.Id; // 主表名称
                VisualDevEntity? res = await NoTblToTable(entity, mTableName);
            }

            _db.CommitTran(); // 提交事务
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 修改接口.
    /// </summary>
    /// <param name="id">主键id</param>
    /// <param name="input">参数</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] VisualDevUpInput input)
    {
        VisualDevEntity? entity = input.Adapt<VisualDevEntity>();
        entity.FlowId = entity.Id;
        try
        {
            // 验证名称和编码是否重复
            if (await _visualDevRepository.AnyAsync(x => x.DeleteMark == null && x.Id != entity.Id && (x.FullName == input.fullName || x.EnCode == input.enCode))) throw Oops.Oh(ErrorCode.D1406);

            TemplateParsingBase? tInfo = new TemplateParsingBase(entity); // 解析模板
            if (!tInfo.VerifyTemplate()) throw Oops.Oh(ErrorCode.D1401); // 验证模板
            _db.BeginTran(); // 开启事务

            #region  处理旧无表数据

            // 无表转有表
            string? mTableName = "mt" + entity.Id; // 主表名称
            if (entity.Tables.IsNullOrEmpty() || entity.Tables == "[]")
            {
                VisualDevEntity? res = await NoTblToTable(entity, mTableName);

                VisualDevModelDataEntity? entityData = await _runService.GetInfo(id);
                if (entityData != null && entityData.Data != null)
                {
                    List<string>? sqlList = DataToInsertSql(mTableName, entityData.Data);

                    try
                    {
                        if (sqlList.Any())
                        {
                            _db.BeginTran(); // 执行事务
                            DbLinkEntity? link = await _visualDevRepository.Context.Queryable<DbLinkEntity>().FirstAsync(m => m.Id == entity.DbLinkId && m.DeleteMark == null);
                            if (link == null) link = _changeDataBase.GetTenantDbLink(_userManager.TenantId,_userManager.TenantDbName); // 当前数据库连接
                            string? DbType = link?.DbType != null ? link.DbType : _visualDevRepository.Context.CurrentConnectionConfig.DbType.ToString(); // 当前数据库类型
                            foreach (string? item in sqlList) await _changeDataBase.ExecuteSql(link, item);
                            _db.CommitTran(); // 提交事务
                        }
                    }
                    catch
                    {
                        _db.RollbackTran(); // 回滚事务
                    }
                }
            }

            #endregion

            if (input.webType == 3)
            {
                #region 表单或列表 转 流程
                VisualDevEntity? oldEntity = await _visualDevRepository.AsQueryable().FirstAsync(v => v.Id == id && v.DeleteMark == null);

                if (oldEntity.WebType < 3)
                {
                    DbLinkEntity? link = await _visualDevRepository.Context.Queryable<DbLinkEntity>().FirstAsync(m => m.Id == oldEntity.DbLinkId && m.DeleteMark == null);
                    if (link == null) link = _changeDataBase.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName); // 当前数据库连接
                    string? dbType = link?.DbType != null ? link.DbType : _visualDevRepository.Context.CurrentConnectionConfig.DbType.ToString(); // 当前数据库类型
                    string? mainTable = entity.Tables.ToList<TableModel>().Find(m => m.typeId.Equals("1"))?.table; // 主表名称
                    List<DbTableFieldModel>? fieldList = _changeDataBase.GetFieldList(link, mainTable); // 获取主表所有列

                    if (!fieldList.Any(x => SqlFunc.ToLower(x.field) == "f_flowid"))
                    {
                        List<DbTableFieldModel>? pFieldList = new List<DbTableFieldModel>() { new DbTableFieldModel() { field = "F_FlowId", fieldName = "流程Id", dataType = "varchar", dataLength = "50", allowNull = 1 } };
                        _changeDataBase.AddTableColumn(mainTable, pFieldList);
                    }

                    // Flow_Id 赋予默认值(主表Id 值)
                    string? sql = string.Format("update {0} set {1}={2};", mainTable, "F_FlowId", fieldList.FirstOrDefault(t => t.primaryKey)?.field);
                    await _changeDataBase.ExecuteSql(link, sql);
                }
                #endregion

                DictionaryDataEntity? categoryData = await _dictionaryDataService.GetInfo(entity.Category);
                FlowEngineEntity? engineEntity = await _visualDevRepository.Context.Queryable<FlowEngineEntity>().FirstAsync(f => f.Id == entity.FlowId);
                if (engineEntity == null) engineEntity = new FlowEngineEntity();
                engineEntity.FlowTemplateJson = input.flowTemplateJson;
                engineEntity.EnCode = "#visualDev" + entity.EnCode;
                engineEntity.Type = 1;
                engineEntity.FormType = 2;
                engineEntity.FullName = entity.FullName;
                engineEntity.Category = categoryData.EnCode;
                engineEntity.VisibleType = 0;
                engineEntity.Icon = "icon-qt icon-qt-node";
                engineEntity.IconBackground = "#008cff";
                engineEntity.Tables = entity.Tables;
                engineEntity.DbLinkId = entity.DbLinkId;
                engineEntity.FormTemplateJson = entity.FormData;
                engineEntity.Version = (engineEntity.Version.ParseToInt() + 1).ToString();
                if (engineEntity.Id.IsNotEmptyOrNull())
                {
                    await _visualDevRepository.Context.Updateable(engineEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                }
                else
                {
                    engineEntity.Id = entity.Id;
                    await _visualDevRepository.Context.Insertable(engineEntity).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
                }
            }

            await _visualDevRepository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
            _db.CommitTran(); // 关闭事务
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 删除接口.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        VisualDevEntity? entity = await _visualDevRepository.AsQueryable().FirstAsync(v => v.Id == id && v.DeleteMark == null);
        if (await _visualDevRepository.Context.Queryable<FlowTaskEntity>().AnyAsync(x => x.DeleteMark == null && x.FlowId == id))
            throw Oops.Oh(ErrorCode.WF0024);
        await _visualDevRepository.Context.Updateable(entity).CallEntityMethod(m => m.Delete()).UpdateColumns(it => new { it.DeleteMark, it.DeleteTime, it.DeleteUserId }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 复制.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Copy")]
    public async Task ActionsCopy(string id)
    {
        string? random = new Random().NextLetterAndNumberString(5);
        VisualDevEntity? entity = await _visualDevRepository.AsQueryable().FirstAsync(v => v.Id == id && v.DeleteMark == null);
        entity.FullName = entity.FullName + "副本" + random;
        entity.EnCode += random;
        entity.State = 0;
        entity.Id = null; // 复制的数据需要把Id清空，否则会主键冲突错误
        if (entity.WebType == 3)
        {
            DictionaryDataEntity? categoryData = await _dictionaryDataService.GetInfo(entity.Category);
            FlowEngineEntity? flowEngine = new FlowEngineEntity();
            flowEngine.FlowTemplateJson = entity.FlowTemplateJson;
            flowEngine.EnCode = "#visualDev" + entity.EnCode;
            flowEngine.Type = 1;
            flowEngine.FormType = 2;
            flowEngine.FullName = entity.FullName;
            flowEngine.Category = categoryData.EnCode;
            flowEngine.VisibleType = 0;
            flowEngine.Icon = "icon-qt icon-qt-node";
            flowEngine.IconBackground = "#008cff";
            flowEngine.Tables = entity.Tables;
            flowEngine.DbLinkId = entity.DbLinkId;
            flowEngine.FormTemplateJson = entity.FormData;
            flowEngine.Version = "1";
            try
            {
                // 添加流程引擎
                FlowEngineEntity? engineEntity = await _visualDevRepository.Context.Insertable(flowEngine).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteReturnEntityAsync();
                entity.FlowId = engineEntity.Id;
                entity.Id = engineEntity.Id;
                await _visualDevRepository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
            }
            catch
            {
                if (entity.FullName.Length >= 100 || entity.EnCode.Length >= 50) throw Oops.Oh(ErrorCode.D1403); // 数据长度超过 字段设定长度
                else throw;
            }
        }
        else
        {
            try
            {
                await _visualDevRepository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
            }
            catch
            {
                if (entity.FullName.Length >= 100 || entity.EnCode.Length >= 50) throw Oops.Oh(ErrorCode.D1403); // 数据长度超过 字段设定长度
                else throw;
            }
        }
    }

    /// <summary>
    /// 功能同步菜单.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Release")]
    public async Task FuncToMenu(string id, [FromBody] VisualDevToMenuInput input)
    {
        input.id = id;
        VisualDevEntity? entity = await _visualDevRepository.AsQueryable().FirstAsync(x => x.Id == input.id);

        if (entity.State == 0) throw Oops.Oh(ErrorCode.D1405);

        TemplateParsingBase? tInfo = new TemplateParsingBase(entity); // 解析模板
        ColumnDesignModel? columnData = new ColumnDesignModel();

        // 列配置模型
        if (!string.IsNullOrWhiteSpace(entity.ColumnData))
        {
            columnData = entity.ColumnData.ToObject<ColumnDesignModel>();
        }
        else
        {
            columnData = new ColumnDesignModel()
            {
                btnsList = new List<ButtonConfigModel>(),
                columnBtnsList = new List<ButtonConfigModel>(),
                customBtnsList = new List<ButtonConfigModel>(),
                columnList = new List<IndexGridFieldModel>(),
                defaultColumnList = new List<IndexGridFieldModel>()
            };
        }

        columnData.btnsList = columnData.btnsList.Union(columnData.columnBtnsList).ToList();
        if (columnData.customBtnsList != null && columnData.customBtnsList.Any()) columnData.btnsList = columnData.btnsList.Union(columnData.customBtnsList).ToList();

        ColumnDesignModel? appColumnData = new ColumnDesignModel();

        // App列配置模型
        if (!string.IsNullOrWhiteSpace(entity.AppColumnData))
        {
            appColumnData = tInfo.AppColumnData;
        }
        else
        {
            appColumnData = new ColumnDesignModel()
            {
                btnsList = new List<ButtonConfigModel>(),
                columnBtnsList = new List<ButtonConfigModel>(),
                customBtnsList = new List<ButtonConfigModel>(),
                columnList = new List<IndexGridFieldModel>(),
                defaultColumnList = new List<IndexGridFieldModel>()
            };
        }

        appColumnData.btnsList = appColumnData.btnsList.Union(appColumnData.columnBtnsList).ToList();
        if (appColumnData.customBtnsList != null && appColumnData.customBtnsList.Any()) appColumnData.btnsList = appColumnData.btnsList.Union(appColumnData.customBtnsList).ToList();

        try
        {
            _db.BeginTran();

            #region 旧的菜单、权限数据
            var oldWebModule = await _visualDevRepository.Context.Queryable<ModuleEntity>().FirstAsync(x => x.ModuleId == input.id && x.Category == "Web" && x.DeleteMark == null);
            var oldWebModuleButtonEntity = await _visualDevRepository.Context.Queryable<ModuleButtonEntity>().Where(x => x.DeleteMark == null)
                .WhereIF(oldWebModule != null, x => x.ModuleId == oldWebModule.Id).WhereIF(oldWebModule == null, x => x.ModuleId == "0").ToListAsync();
            var oldWebModuleColumnEntity = await _visualDevRepository.Context.Queryable<ModuleColumnEntity>().Where(x => x.DeleteMark == null)
                .WhereIF(oldWebModule != null, x => x.ModuleId == oldWebModule.Id).WhereIF(oldWebModule == null, x => x.ModuleId == "0").ToListAsync();
            var oldWebModuleFormEntity = await _visualDevRepository.Context.Queryable<ModuleFormEntity>().Where(x => x.DeleteMark == null)
                .WhereIF(oldWebModule != null, x => x.ModuleId == oldWebModule.Id).WhereIF(oldWebModule == null, x => x.ModuleId == "0").ToListAsync();

            var oldAppModule = await _visualDevRepository.Context.Queryable<ModuleEntity>().FirstAsync(x => x.ModuleId == input.id && x.Category == "App" && x.DeleteMark == null);
            var oldAppModuleButtonEntity = await _visualDevRepository.Context.Queryable<ModuleButtonEntity>().Where(x => x.DeleteMark == null)
                .WhereIF(oldAppModule != null, x => x.ModuleId == oldAppModule.Id).WhereIF(oldAppModule == null, x => x.ModuleId == "0").ToListAsync();
            var oldAppModuleColumnEntity = await _visualDevRepository.Context.Queryable<ModuleColumnEntity>().Where(x => x.DeleteMark == null)
                .WhereIF(oldAppModule != null, x => x.ModuleId == oldAppModule.Id).WhereIF(oldAppModule == null, x => x.ModuleId == "0").ToListAsync();
            var oldAppModuleFormEntity = await _visualDevRepository.Context.Queryable<ModuleFormEntity>().Where(x => x.DeleteMark == null)
                .WhereIF(oldAppModule != null, x => x.ModuleId == oldAppModule.Id).WhereIF(oldAppModule == null, x => x.ModuleId == "0").ToListAsync();
            #endregion

            #region 菜单组装
            var moduleModel = new ModuleEntity();
            moduleModel.Id = oldWebModule != null ? oldWebModule.Id : YitIdHelper.NextId().ToString();
            moduleModel.ModuleId = input.id;
            moduleModel.ParentId = oldWebModule != null ? oldWebModule.ParentId : input.pcModuleParentId;//父级菜单节点
            moduleModel.Category = "Web";
            moduleModel.FullName = entity.FullName;
            moduleModel.EnCode = entity.EnCode;
            moduleModel.Icon = "icon-qt icon-qt-webForm";
            moduleModel.UrlAddress = "model/" + moduleModel.EnCode;
            moduleModel.Type = 3;
            moduleModel.EnabledMark = 1;
            moduleModel.IsColumnAuthorize = 1;
            moduleModel.IsButtonAuthorize = 1;
            moduleModel.IsFormAuthorize = 1;
            moduleModel.IsDataAuthorize = 1;
            moduleModel.SortCode = 999;
            moduleModel.PropertyJson = (new { moduleId = input.id, iconBackgroundColor = string.Empty, isTree = 0 }).ToJsonString();
            #endregion
            #region 配置权限

            // 按钮权限
            var btnAuth = new List<ModuleButtonEntity>();
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, ParentId = "-1", EnCode = "btn_add", FullName = "新增", Icon = "el-icon-plus", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, ParentId = "-1", EnCode = "btn_download", FullName = "导出", Icon = "el-icon-download", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, ParentId = "-1", EnCode = "btn_batchRemove", FullName = "批量删除", Icon = "el-icon-delete", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, ParentId = "-1", EnCode = "btn_edit", FullName = "编辑", Icon = "el-icon-edit", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, ParentId = "-1", EnCode = "btn_remove", FullName = "删除", Icon = "el-icon-delete", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, ParentId = "-1", EnCode = "btn_detail", FullName = "详情", Icon = "el-icon-tickets", ModuleId = moduleModel.Id });
            columnData.customBtnsList.ForEach(item =>
            {
                btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 1, ParentId = "-1", EnCode = item.value, FullName = item.label, ModuleId = moduleModel.Id });
            });

            columnData.btnsList.ForEach(item =>
            {
                var aut = btnAuth.Find(x => x.FullName == item.label);
                if (aut != null) aut.EnabledMark = 1;
            });

            // 表单权限
            var columnAuth = new List<ModuleColumnEntity>();
            var fieldList = tInfo.FieldsModelList;
            var formAuth = new List<ModuleFormEntity>();
            fieldList.Where(x => x.__vModel__.IsNotEmptyOrNull()).ToList().ForEach(item =>
            {
                var fRule = item.__vModel__.Contains("_qt_") ? 1 : 0;
                formAuth.Add(new ModuleFormEntity() { ParentId = "-1", EnCode = item.__vModel__, FieldRule = fRule, ModuleId = moduleModel.Id, FullName = item.__config__.label, EnabledMark = 1, SortCode = 0 });
            });

            // 列表权限
            columnData.defaultColumnList.ForEach(item =>
            {
                var itemModel = fieldList.FirstOrDefault(x => x.__config__.qtKey == item.qtKey && x.__vModel__ == item.prop);
                if (itemModel != null)
                {
                    var fRule = itemModel.__vModel__.Contains("_qt_") ? 1 : 0;
                    columnAuth.Add(new ModuleColumnEntity() { ParentId = "-1", EnCode = itemModel.__vModel__, FieldRule = fRule, ModuleId = moduleModel.Id, FullName = itemModel.__config__.label, EnabledMark = 0, SortCode = 0 });
                }
            });

            columnData.columnList.ForEach(item =>
            {
                var aut = columnAuth.Find(x => x.EnCode == item.prop);
                if (aut != null) aut.EnabledMark = 1;
            });

            #endregion

            // 添加PC菜单和权限
            if (input.pc == 1)
            {
                var storModuleModel = _visualDevRepository.Context.Storageable(moduleModel).Saveable().ToStorage(); // 存在更新不存在插入 根据主键
                await storModuleModel.AsInsertable.ExecuteCommandAsync(); // 执行插入
                await storModuleModel.AsUpdateable.ExecuteCommandAsync(); // 执行更新

                #region 表单权限
                if (columnData.useFormPermission)
                {
                    if (!oldWebModuleFormEntity.Any())
                    {
                        await _visualDevRepository.Context.Insertable(formAuth).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                    }
                    else
                    {
                        var formAuthAddList = new List<ModuleFormEntity>();
                        formAuth.ForEach(item =>
                        {
                            if (!oldWebModuleFormEntity.Any(x => x.EnCode == item.EnCode)) formAuthAddList.Add(item);
                        });
                        if (formAuthAddList.Any()) await _visualDevRepository.Context.Insertable(formAuthAddList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                        oldWebModuleFormEntity.ForEach(item =>
                        {
                            var it = formAuth.FirstOrDefault(x => x.EnCode == item.EnCode);
                            if (it == null) item.DeleteMark = 1; // 删除标识
                            else item.EnabledMark = 1; // 显示标识
                        });
                        await _visualDevRepository.Context.Updateable(oldWebModuleFormEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    }
                }
                #endregion

                #region 按钮权限
                if (columnData.useBtnPermission)
                {
                    if (!oldWebModuleButtonEntity.Any()) // 新增数据
                    {
                        await _visualDevRepository.Context.Insertable(btnAuth).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                    }
                    else // 修改增加数据权限
                    {
                        var btnAuthAddList = new List<ModuleButtonEntity>();
                        btnAuth.ForEach(item =>
                        {
                            if (!oldWebModuleButtonEntity.Any(x => x.EnCode == item.EnCode)) btnAuthAddList.Add(item);
                        });
                        if (btnAuthAddList.Any()) await _visualDevRepository.Context.Insertable(btnAuthAddList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();

                        oldWebModuleButtonEntity.ForEach(item =>
                        {
                            var it = btnAuth.FirstOrDefault(x => x.EnCode == item.EnCode);
                            if (it == null) item.DeleteMark = 1; // 删除标识
                            else item.EnabledMark = it.EnabledMark; // 显示标识
                        });
                        await _visualDevRepository.Context.Updateable(oldWebModuleButtonEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    }
                }
                #endregion

                #region 列表权限
                if (columnData.useColumnPermission)
                {
                    if (!oldWebModuleColumnEntity.Any()) // 新增数据
                    {
                        await _visualDevRepository.Context.Insertable(columnAuth).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                    }
                    else // 修改增加数据权限
                    {
                        var columnAuthAddList = new List<ModuleColumnEntity>();
                        columnAuth.ForEach(item =>
                        {
                            if (!oldWebModuleColumnEntity.Any(x => x.EnCode == item.EnCode)) columnAuthAddList.Add(item);
                        });
                        if (columnAuthAddList.Any()) await _visualDevRepository.Context.Insertable(columnAuthAddList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                        oldWebModuleColumnEntity.ForEach(item =>
                        {
                            var it = columnAuth.FirstOrDefault(x => x.EnCode == item.EnCode);
                            if (it == null) item.DeleteMark = 1; // 删除标识
                            else item.EnabledMark = it.EnabledMark; // 显示标识
                        });
                        await _visualDevRepository.Context.Updateable(oldWebModuleColumnEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    }
                }
                #endregion

                #region 数据权限
                if (columnData.useDataPermission)
                {
                    if (!_visualDevRepository.Context.Queryable<ModuleDataAuthorizeSchemeEntity>().Where(x => x.EnCode == "qt_alldata" && x.ModuleId == moduleModel.Id && x.DeleteMark == null).Any())
                    {
                        // 全部数据权限方案
                        var AllDataAuthScheme = new ModuleDataAuthorizeSchemeEntity()
                        {
                            FullName = "全部数据",
                            EnCode = "qt_alldata",
                            ConditionText = string.Empty,
                            ConditionJson = string.Empty,
                            ModuleId = moduleModel.Id
                        };
                        await _visualDevRepository.Context.Insertable(AllDataAuthScheme).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
                    }

                    // 创建用户和所属组织权限方案
                    if (fieldList.Any(x => x.__config__.qtKey == "createUser" || x.__config__.qtKey == "currOrganize"))
                    {
                        // 只添加 主表控件的数据权限
                        var fList = fieldList.Where(x => !x.__vModel__.Contains("_qt_") && x.__vModel__.IsNotEmptyOrNull() && x.__config__.visibility.Contains("pc"))
                            .Where(x => x.__config__.qtKey == "createUser" || x.__config__.qtKey == "currOrganize").ToList();

                        var authList = await MenuMergeDataAuth(moduleModel.Id, fList);
                        await MenuMergeDataAuthScheme(moduleModel.Id, authList, fList);
                    }
                }
                #endregion
            }

            #region App菜单、权限组装
            moduleModel.Id = oldAppModule != null ? oldAppModule.Id : YitIdHelper.NextId().ToString();
            moduleModel.ModuleId = input.id;
            moduleModel.ParentId = oldAppModule != null ? oldAppModule.ParentId : input.appModuleParentId; // 父级菜单节点
            moduleModel.Category = "App";
            moduleModel.UrlAddress = "/pages/apply/dynamicModel/index?id=" + moduleModel.Id;

            btnAuth = new List<ModuleButtonEntity>();
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, ParentId = "-1", EnCode = "btn_add", FullName = "新增", Icon = "el-icon-plus", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, ParentId = "-1", EnCode = "btn_edit", FullName = "编辑", Icon = "el-icon-edit", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, ParentId = "-1", EnCode = "btn_remove", FullName = "删除", Icon = "el-icon-delete", ModuleId = moduleModel.Id });
            btnAuth.Add(new ModuleButtonEntity() { EnabledMark = 0, ParentId = "-1", EnCode = "btn_detail", FullName = "详情", Icon = "el-icon-tickets", ModuleId = moduleModel.Id });

            appColumnData.btnsList.ForEach(item =>
            {
                var aut = btnAuth.Find(x => x.FullName == item.label);
                if (aut != null) aut.EnabledMark = 1;
            });

            formAuth.Clear();
            fieldList.Where(x => x.__vModel__.IsNotEmptyOrNull()).ToList().ForEach(item =>
            {
                var fRule = item.__vModel__.Contains("_qt_") ? 1 : 0;
                formAuth.Add(new ModuleFormEntity() { ParentId = "-1", EnCode = item.__vModel__, FieldRule = fRule, ModuleId = moduleModel.Id, FullName = item.__config__.label, EnabledMark = 1, SortCode = 0 });
            });

            columnAuth.Clear();
            appColumnData.defaultColumnList.ForEach(item =>
            {
                var itemModel = fieldList.FirstOrDefault(x => x.__config__.qtKey == item.qtKey && x.__vModel__ == item.prop);
                if (itemModel != null)
                {
                    var fRule = itemModel.__vModel__.Contains("_qt_") ? 1 : 0;
                    columnAuth.Add(new ModuleColumnEntity() { ParentId = "-1", EnCode = itemModel.__vModel__, FieldRule = fRule, ModuleId = moduleModel.Id, FullName = itemModel.__config__.label, EnabledMark = 0, SortCode = 0 });
                }
            });
            appColumnData.columnList.ForEach(item =>
            {
                var aut = columnAuth.Find(x => x.EnCode == item.prop);
                if (aut != null) aut.EnabledMark = 1;
            });

            columnAuth.ForEach(item => { item.ModuleId = moduleModel.Id; });
            #endregion

            // 添加App菜单和权限
            if (input.app == 1)
            {
                var storModuleModel = _visualDevRepository.Context.Storageable(moduleModel).Saveable().ToStorage(); // 存在更新不存在插入 根据主键
                await storModuleModel.AsInsertable.ExecuteCommandAsync(); // 执行插入
                await storModuleModel.AsUpdateable.ExecuteCommandAsync(); // 执行更新

                #region 表单权限
                if (appColumnData.useFormPermission)
                {
                    if (!oldAppModuleFormEntity.Any())
                    {
                        await _visualDevRepository.Context.Insertable(formAuth).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                    }
                    else
                    {
                        var formAuthAddList = new List<ModuleFormEntity>();
                        formAuth.ForEach(item =>
                        {
                            if (!oldAppModuleFormEntity.Any(x => x.EnCode == item.EnCode)) formAuthAddList.Add(item);
                        });
                        if (formAuthAddList.Any()) await _visualDevRepository.Context.Insertable(formAuthAddList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                        oldAppModuleFormEntity.ForEach(item =>
                        {
                            var it = formAuth.FirstOrDefault(x => x.EnCode == item.EnCode);
                            if (it == null) item.DeleteMark = 1; // 删除标识
                            else item.EnabledMark = 1; // 显示标识
                        });
                        await _visualDevRepository.Context.Updateable(oldAppModuleFormEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    }
                }
                #endregion

                #region 按钮权限
                if (appColumnData.useBtnPermission)
                {
                    if (!oldAppModuleButtonEntity.Any()) // 新增数据
                    {
                        await _visualDevRepository.Context.Insertable(btnAuth).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                    }
                    else // 修改增加数据权限
                    {
                        var btnAuthAddList = new List<ModuleButtonEntity>();
                        btnAuth.ForEach(item =>
                        {
                            if (!oldAppModuleButtonEntity.Any(x => x.EnCode == item.EnCode)) btnAuthAddList.Add(item);
                        });
                        if (btnAuthAddList.Any()) await _visualDevRepository.Context.Insertable(btnAuthAddList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();

                        oldAppModuleButtonEntity.ForEach(item =>
                        {
                            var it = btnAuth.FirstOrDefault(x => x.EnCode == item.EnCode);
                            if (it == null) item.DeleteMark = 1; // 删除标识
                            else item.EnabledMark = it.EnabledMark; // 显示标识
                        });
                        await _visualDevRepository.Context.Updateable(oldAppModuleButtonEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    }
                }
                #endregion

                #region 列表权限
                if (appColumnData.useColumnPermission)
                {
                    if (!oldAppModuleColumnEntity.Any()) // 新增数据
                    {
                        await _visualDevRepository.Context.Insertable(columnAuth).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
                    }
                    else // 修改增加数据权限
                    {
                        var columnAuthAddList = new List<ModuleColumnEntity>();
                        columnAuth.ForEach(item =>
                        {
                            if (!oldAppModuleColumnEntity.Any(x => x.EnCode == item.EnCode)) columnAuthAddList.Add(item);
                        });
                        if (columnAuthAddList.Any()) await _visualDevRepository.Context.Insertable(columnAuthAddList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();

                        oldAppModuleColumnEntity.ForEach(item =>
                        {
                            var it = columnAuth.FirstOrDefault(x => x.EnCode == item.EnCode);
                            if (it == null) item.DeleteMark = 1; // 删除标识
                            else item.EnabledMark = it.EnabledMark; // 显示标识
                        });
                        await _visualDevRepository.Context.Updateable(oldAppModuleColumnEntity).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
                    }
                }
                #endregion

                #region 数据权限
                if (appColumnData.useDataPermission)
                {
                    // 全部数据权限
                    if (!_visualDevRepository.Context.Queryable<ModuleDataAuthorizeSchemeEntity>().Where(x => x.EnCode == "qt_alldata" && x.ModuleId == moduleModel.Id && x.DeleteMark == null).Any())
                    {
                        // 全部数据权限方案
                        var AllDataAuthScheme = new ModuleDataAuthorizeSchemeEntity()
                        {
                            FullName = "全部数据",
                            EnCode = "qt_alldata",
                            ConditionText = string.Empty,
                            ConditionJson = string.Empty,
                            ModuleId = moduleModel.Id
                        };
                        await _visualDevRepository.Context.Insertable(AllDataAuthScheme).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
                    }

                    // 创建用户和所属组织权限方案
                    if (fieldList.Any(x => x.__config__.qtKey == "createUser" || x.__config__.qtKey == "currOrganize"))
                    {
                        // 只添加 主表控件的数据权限
                        var fList = fieldList.Where(x => !x.__vModel__.Contains("_qt_") && x.__vModel__.IsNotEmptyOrNull() && x.__config__.visibility.Contains("app"))
                            .Where(x => x.__config__.qtKey == "createUser" || x.__config__.qtKey == "currOrganize").ToList();

                        var authList = await MenuMergeDataAuth(moduleModel.Id, fList);
                        await MenuMergeDataAuthScheme(moduleModel.Id, authList, fList);
                    }
                }
                #endregion
            }

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw;
        }
    }


    /// <summary>
    /// 功能生成菜单.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/GenerateMenu")]
    public async Task GenerateMenu(string id, [FromBody] VisualDevGenerateMenuInput input)
    {
        input.id = id;
        VisualDevEntity? entity = await _visualDevRepository.AsQueryable().FirstAsync(x => x.Id == input.id);
        var oldWebModule = await _visualDevRepository.Context.Queryable<ModuleEntity>().FirstAsync(x => x.ModuleId == input.id && x.Category == "Web" && x.DeleteMark == null);
        if (oldWebModule != null)
        {
            return;
        }
        var prefix = input.prefix ?? "model";
        #region 菜单组装
        var moduleModel = new ModuleEntity();
        moduleModel.Id = YitIdHelper.NextId().ToString();
        moduleModel.ModuleId = input.id;
        moduleModel.ParentId = input.pcModuleParentId;//父级菜单节点
        moduleModel.Category = "Web";
        moduleModel.FullName = entity.FullName;
        moduleModel.EnCode = entity.EnCode;
        moduleModel.Icon = "icon-qt icon-qt-webForm";
        moduleModel.UrlAddress = $"{prefix}/{moduleModel.Id}";
        moduleModel.Type = input.type;
        moduleModel.EnabledMark = 1;
        moduleModel.IsColumnAuthorize = 1;
        moduleModel.IsButtonAuthorize = 1;
        moduleModel.IsFormAuthorize = 1;
        moduleModel.IsDataAuthorize = 1;
        moduleModel.SortCode = 999;
        moduleModel.PropertyJson = (new { moduleId = input.id, iconBackgroundColor = string.Empty, isTree = 0 }).ToJsonString();
        #endregion
        await _visualDevRepository.Context.Insertable<ModuleEntity>(moduleModel).ExecuteCommandAsync();
    }

    /// <summary>
    /// 功能同步到租户云端.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Public")]
    public async Task ReleaseToCloud(string id)
    {
        if (KeyVariable.MultiTenancy)
        {
            var tenantId = TenantScoped.TenantId;
            var response = await $"{KeyVariable.MultiTenancyHost}/api/visualdev/base/{tenantId}/Actions/Release/{id}"
                 .SetHeaders(new Dictionary<string, object>
                 {
                    {"user-key", KeyVariable.MultiTenancyUserKey }
                 })
                 .PostAsAsync<RESTfulResult<object>>();

            if (response.code!=200)
            {
                throw Oops.Oh(response.msg);
            }
        }
    }
    #endregion

    #region PublicMethod

    /// <summary>
    /// 获取功能信息.
    /// </summary>
    /// <param name="id">主键ID.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<VisualDevEntity> GetInfoById(string id)
    {
        return await _visualDevRepository.AsQueryable().FirstAsync(x => x.Id == id);
    }

    /// <summary>
    /// 判断功能ID是否存在.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<bool> GetDataExists(string id)
    {
        return await _visualDevRepository.AnyAsync(it => it.Id == id && it.DeleteMark == null);
    }

    /// <summary>
    /// 判断是否存在编码、名称相同的数据.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<bool> GetDataExists(string enCode, string fullName)
    {
        return await _visualDevRepository.AnyAsync(it => it.EnCode == enCode && it.FullName == fullName && it.DeleteMark == null);
    }

    /// <summary>
    /// 新增导入数据.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [NonAction]
    public async Task CreateImportData(VisualDevEntity input)
    {
        try
        {
            _db.BeginTran(); // 开启事务

            if (input.WebType == 3)
            {
                DictionaryDataEntity? categoryData = await _dictionaryDataService.GetInfo(input.Category);
                FlowEngineEntity? flowEngine = new FlowEngineEntity();
                flowEngine.FlowTemplateJson = input.FlowTemplateJson;
                flowEngine.EnCode = "#visualDev" + input.EnCode;
                flowEngine.Type = 1;
                flowEngine.FormType = 2;
                flowEngine.FullName = input.FullName;
                flowEngine.Category = categoryData.EnCode;
                flowEngine.VisibleType = 0;
                flowEngine.Icon = "icon-qt icon-qt-node";
                flowEngine.IconBackground = "#008cff";
                flowEngine.Tables = input.Tables;
                flowEngine.DbLinkId = input.DbLinkId;
                flowEngine.FormTemplateJson = input.FormData;

                // 添加流程引擎
                FlowEngineEntity? engineEntity = await _visualDevRepository.Context.Insertable(flowEngine).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteReturnEntityAsync();
                input.FlowId = engineEntity.Id;
                input.Id = engineEntity.Id;
            }

            StorageableResult<VisualDevEntity>? stor = _visualDevRepository.Context.Storageable(input).Saveable().ToStorage(); // 存在更新不存在插入 根据主键
            await stor.AsInsertable.ExecuteCommandAsync(); // 执行插入
            await _visualDevRepository.Context.Updateable(input).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();

            _db.CommitTran(); // 关闭事务
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw;
        }
    }

    /// <summary>
    /// 功能模板 无表 转 有表.
    /// </summary>
    /// <param name="vEntity">功能实体.</param>
    /// <param name="mainTableName">主表名称.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<VisualDevEntity> NoTblToTable(VisualDevEntity vEntity, string mainTableName)
    {
        var dbtype = App.Configuration["ConnectionStrings:DBType"]; // 读取数据库连接配置
        var isUpper = false; // 是否大写
        if (dbtype.ToLower().Equals("oracle") || dbtype.ToLower().Equals("dm") || dbtype.ToLower().Equals("dm8")) isUpper = true;
        else isUpper = false;

        // Oracle和Dm数据库 表名全部大写, 其他全部小写
        mainTableName = isUpper ? mainTableName.ToUpper() : mainTableName.ToLower();

        FormDataModel formModel = vEntity.FlowId.IsNotEmptyOrNull() ? TemplateKeywordsHelper.ReplaceKeywords(vEntity.FormData).ToObject<FormDataModel>() : vEntity.FormData.ToObject<FormDataModel>();
        List<FieldsModel>? fieldsModelList = TemplateAnalysis.AnalysisTemplateData(formModel.fields);

        #region 创表信息组装

        List<DbTableAndFieldModel>? addTableList = new List<DbTableAndFieldModel>(); // 表集合

        // 主表信息
        DbTableAndFieldModel? mainInfo = new DbTableAndFieldModel();
        mainInfo.table = mainTableName;
        mainInfo.tableName = vEntity.FullName;
        mainInfo.FieldList = FieldsModelToTableFile(fieldsModelList);

        // 子表信息
        Dictionary<string, string>? childTableDic = new Dictionary<string, string>();
        fieldsModelList.Where(x => x.__config__.qtKey == "table").ToList().ForEach(item =>
        {
            DbTableAndFieldModel? childTInfo = new DbTableAndFieldModel();
            childTInfo.table = "ct" + YitIdHelper.NextId().ToString();
            childTInfo.table = isUpper ? childTInfo.table.ToUpper() : childTInfo.table.ToLower();
            childTableDic.Add(item.__vModel__, childTInfo.table);
            childTInfo.tableName = vEntity.FullName + "_子表";
            childTInfo.FieldList = FieldsModelToTableFile(item.__config__.children);
            childTInfo.FieldList.Add(new DbTableFieldModel() { dataLength = "50", allowNull = 1, dataType = "varchar", field = "F_Relation_Id", fieldName = vEntity.FullName + "_关联外键" });
            addTableList.Add(childTInfo);
        });

        #endregion

        #region 修改功能模板 有表改无表
        List<TableModel>? modelTableList = new List<TableModel>();

        // 处理主表
        TableModel? mainTable = new TableModel();
        mainTable.fields = new List<EntityFieldModel>();
        mainTable.table = mainInfo.table;
        mainTable.tableName = mainInfo.tableName;
        mainTable.typeId = "1";
        mainInfo.FieldList.ForEach(item => // 表字段
        {
            EntityFieldModel? etFieldModel = new EntityFieldModel();
            etFieldModel.DataLength = item.dataLength;
            etFieldModel.PrimaryKey = 1;
            etFieldModel.DataType = item.dataType;
            etFieldModel.Field = item.field;
            etFieldModel.FieldName = item.fieldName;
            mainTable.fields.Add(etFieldModel);
        });

        // 处理子表
        addTableList.ForEach(item =>
        {
            TableModel? childInfo = new TableModel();
            childInfo.fields = new List<EntityFieldModel>();
            childInfo.table = item.table;
            childInfo.tableName = item.tableName;
            childInfo.tableField = "F_Relation_Id"; // 关联外键
            childInfo.relationField = "F_Id"; // 关联主键
            childInfo.typeId = "0";
            item.FieldList.ForEach(it => // 子表字段
            {
                EntityFieldModel? etFieldModel = new EntityFieldModel();
                etFieldModel.DataLength = it.dataLength;
                etFieldModel.PrimaryKey = 0;
                etFieldModel.DataType = it.dataType;
                etFieldModel.Field = it.field;
                etFieldModel.FieldName = it.fieldName;
                childInfo.fields.Add(etFieldModel);
            });
            modelTableList.Add(childInfo);
        });
        modelTableList.Add(mainTable);

        #region 给控件绑定 tableName、relationTable 属性

        // 用字典反序列化， 避免多增加不必要的属性
        Dictionary<string, object>? dicFormModel = vEntity.FormData.ToObject<Dictionary<string, object>>();
        List<Dictionary<string, object>>? dicFieldsModelList = dicFormModel.FirstOrDefault(x => x.Key == "fields").Value.ToObject<List<Dictionary<string, object>>>();

        // 主表
        MainFieldsBindTable(dicFieldsModelList, childTableDic, mainTableName);

        // 子表
        ChildFieldBindTable(dicFieldsModelList, childTableDic, mainTableName);

        #endregion

        dicFormModel["fields"] = dicFieldsModelList; // 修改表单控件
        vEntity.FormData = dicFormModel.ToJsonString(); // 修改模板
        vEntity.Tables = modelTableList.ToJsonString(); // 修改模板涉及表

        addTableList.Add(mainInfo);

        #endregion

        try
        {
            _db.BeginTran(); // 执行事务

            foreach (DbTableAndFieldModel? item in addTableList)
            {
                bool res = await _changeDataBase.Create(null, item, item.FieldList);
                if (!res) throw null;
            }

            await _visualDevRepository.Context.Updateable(vEntity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();

            _db.CommitTran(); // 提交事务

            return vEntity;
        }
        catch (Exception e)
        {
            _db.RollbackTran(); // 回滚事务
            return null;
        }
    }

    /// <summary>
    /// 数据 转 插入Sql语句.
    /// </summary>
    /// <param name="tableName">表名.</param>
    /// <param name="dataStr">数据包字符串.</param>
    /// <returns></returns>
    public List<string> DataToInsertSql(string tableName, string dataStr)
    {
        List<string>? sqlList = new List<string>();
        string? sql = "insert into [{0}] ({1}) values('{2}');";

        List<Dictionary<string, string>>? dataMap = dataStr.ToObject<List<Dictionary<string, string>>>();

        List<string>? fielsKeyList = new List<string>();
        List<string>? fieldValueList = new List<string>();

        dataMap.ForEach(item =>
        {
            string? fielsKey = item.Keys.FirstOrDefault();
            fielsKeyList.Add(fielsKey);
            fieldValueList.Add(item[fielsKey]);
        });

        sqlList.Add(string.Format(sql, tableName, string.Join(",", fielsKeyList), string.Join("','", fieldValueList)));

        return sqlList;
    }

    #endregion

    #region Private

    /// <summary>
    /// 组件转换表字段.
    /// </summary>
    /// <returns></returns>
    [NonAction]
    private List<DbTableFieldModel> FieldsModelToTableFile(List<FieldsModel> fmList)
    {
        List<DbTableFieldModel>? fieldList = new List<DbTableFieldModel>(); // 表字段
        List<FieldsModel>? mList = fmList.Where(x => x.__config__.qtKey.IsNotEmptyOrNull())
            .Where(x => x.__config__.qtKey != "relationFormAttr" && x.__config__.qtKey != "popupAttr")
            .Where(x => x.__config__.qtKey != "qrcode" && x.__config__.qtKey != "barcode" && x.__config__.qtKey != "table").ToList(); // 非存储字段

        fieldList.Add(new DbTableFieldModel()
        {
            primaryKey = true,
            dataType = "varchar",
            dataLength = "50",
            field = "F_Id",
            fieldName = "主键"
        });

        mList.ForEach(item =>
        {
            DbTableFieldModel? field = new DbTableFieldModel();
            field.field = item.__vModel__;
            field.fieldName = item.__config__.label;

            switch (item.__config__.qtKey)
            {
                case "numInput":
                    field.dataType = item.precision > 1 ? "decimal" : "int";
                    field.dataLength = "50";
                    field.allowNull = 1;
                    break;
                case "date":
                    field.dataType = "DateTime";
                    field.dataLength = "50";
                    field.allowNull = 1;
                    break;
                case "time":
                    field.dataType = "DateTime";
                    field.dataLength = "50";
                    field.allowNull = 1;
                    break;
                case "createTime":
                    field.dataType = "DateTime";
                    field.dataLength = "50";
                    field.allowNull = 1;
                    break;
                case "modifyTime":
                    field.dataType = "DateTime";
                    field.dataLength = "50";
                    field.allowNull = 1;
                    break;
                case "editor":
                    field.dataType = "text";
                    field.dataLength = "50";
                    field.allowNull = 1;
                    break;
                default:
                    field.dataType = "varchar";
                    field.dataLength = "500";
                    field.allowNull = 1;
                    break;
            }

            if (field.field.IsNotEmptyOrNull()) fieldList.Add(field);
        });

        return fieldList;
    }

    /// <summary>
    /// 组装菜单 数据权限 字段管理数据.
    /// </summary>
    /// <param name="menuId">菜单ID.</param>
    /// <param name="fields">功能模板控件集合.</param>
    /// <returns></returns>
    private async Task<List<ModuleDataAuthorizeEntity>> MenuMergeDataAuth(string menuId, List<FieldsModel> fields)
    {
        // 旧的自动生成的 字段管理
        List<ModuleDataAuthorizeEntity>? oldDataAuth = await _visualDevRepository.Context.Queryable<ModuleDataAuthorizeEntity>()
            .Where(x => x.ModuleId == menuId && x.ConditionSymbol == "Equal" && x.DeleteMark == null)
            .Where(x => x.ConditionText == "@organizationAndSuborganization" || x.ConditionText == "@organizeId" || x.ConditionText == "@userAraSubordinates" || x.ConditionText == "@userId")
            .ToListAsync();

        List<ModuleDataAuthorizeEntity>? authList = new List<ModuleDataAuthorizeEntity>(); // 字段管理
        List<ModuleDataAuthorizeEntity>? noDelData = new List<ModuleDataAuthorizeEntity>(); // 记录未删除

        // 当前用户
        FieldsModel? item = fields.FirstOrDefault(x => x.__config__.qtKey == "createUser");
        if (item != null)
        {
            // 新增
            if (!oldDataAuth.Any(x => x.EnCode == item.__vModel__ && x.ConditionText == "@userId"))
            {
                authList.Add(new ModuleDataAuthorizeEntity()
                {
                    Id = YitIdHelper.NextId().ToString(),
                    ConditionSymbol = "Equal", // 条件符号
                    Type = "varchar", // 字段类型
                    FullName = item.__config__.label, // 字段说明
                    EnCode = item.__vModel__, // 字段名称
                    ConditionText = "@userId", // 条件内容（当前用户）
                    EnabledMark = 1,
                    FieldRule = item.__vModel__.Contains("_qt_") ? 0 : 1, // 主表/副表
                    BindTable = item.__config__.tableName,
                    ModuleId = menuId
                });
            }

            if (!oldDataAuth.Any(x => x.EnCode == item.__vModel__ && x.ConditionText == "@userAraSubordinates"))
            {
                authList.Add(new ModuleDataAuthorizeEntity()
                {
                    Id = YitIdHelper.NextId().ToString(),
                    ConditionSymbol = "Equal", // 条件符号
                    Type = "varchar", // 字段类型
                    FullName = item.__config__.label, // 字段说明
                    EnCode = item.__vModel__, // 字段名称
                    ConditionText = "@userAraSubordinates", // 条件内容（当前用户及下属）
                    EnabledMark = 1,
                    FieldRule = item.__vModel__.Contains("_qt_") ? 0 : 1, // 主表/副表
                    BindTable = item.__config__.tableName,
                    ModuleId = menuId
                });
            }

            // 删除
            List<ModuleDataAuthorizeEntity>? delData = oldDataAuth.Where(x => x.EnCode != item.__vModel__ && (x.ConditionText == "@userId" || x.ConditionText == "@userAraSubordinates")).ToList();
            await _visualDevRepository.Context.Deleteable(delData).ExecuteCommandAsync();

            noDelData = oldDataAuth.Except(delData).ToList(); // 记录未删除
        }
        else
        {
            // 删除
            List<ModuleDataAuthorizeEntity>? delData = oldDataAuth.Where(x => x.ConditionText == "@userId" || x.ConditionText == "@userAraSubordinates").ToList();
            await _visualDevRepository.Context.Deleteable(delData).ExecuteCommandAsync();
        }

        // 所属组织
        item = fields.FirstOrDefault(x => x.__config__.qtKey == "currOrganize");
        if (item != null)
        {
            // 新增
            if (!oldDataAuth.Any(x => x.EnCode == item.__vModel__ && x.ConditionText == "@organizeId"))
            {
                authList.Add(new ModuleDataAuthorizeEntity()
                {
                    Id = YitIdHelper.NextId().ToString(),
                    ConditionSymbol = "Equal", // 条件符号
                    Type = "varchar", // 字段类型
                    FullName = item.__config__.label, // 字段说明
                    EnCode = item.__vModel__, // 字段名称
                    ConditionText = "@organizeId", // 条件内容（当前组织）
                    EnabledMark = 1,
                    FieldRule = item.__vModel__.Contains("_qt_") ? 0 : 1, // 主表/副表
                    BindTable = item.__config__.tableName,
                    ModuleId = menuId
                });
            }

            if (!oldDataAuth.Any(x => x.EnCode == item.__vModel__ && x.ConditionText == "@organizationAndSuborganization"))
            {
                authList.Add(new ModuleDataAuthorizeEntity()
                {
                    Id = YitIdHelper.NextId().ToString(),
                    ConditionSymbol = "Equal", // 条件符号
                    Type = "varchar", // 字段类型
                    FullName = item.__config__.label, // 字段说明
                    EnCode = item.__vModel__, // 字段名称
                    ConditionText = "@organizationAndSuborganization", // 条件内容（当前组织及组织）
                    EnabledMark = 1,
                    FieldRule = item.__vModel__.Contains("_qt_") ? 0 : 1, // 主表/副表
                    BindTable = item.__config__.tableName,
                    ModuleId = menuId
                });
            }

            // 删除
            List<ModuleDataAuthorizeEntity>? delData = oldDataAuth.Where(x => x.EnCode != item.__vModel__ && (x.ConditionText == "@organizeId" || x.ConditionText == "@organizationAndSuborganization")).ToList();
            await _visualDevRepository.Context.Deleteable(delData).ExecuteCommandAsync();

            noDelData = oldDataAuth.Except(delData).ToList(); // 记录未删除
        }
        else
        {
            // 删除
            List<ModuleDataAuthorizeEntity>? delData = oldDataAuth.Where(x => x.ConditionText == "@organizeId" || x.ConditionText == "@organizationAndSuborganization").ToList();
            await _visualDevRepository.Context.Deleteable(delData).ExecuteCommandAsync();
        }

        if (authList.Any()) await _visualDevRepository.Context.Insertable(authList).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();
        if (noDelData.Any()) authList.AddRange(noDelData);
        return authList.Any() ? authList : oldDataAuth;
    }

    /// <summary>
    /// 组装菜单 数据权限 方案管理数据.
    /// </summary>
    /// <param name="menuId">菜单ID.</param>
    /// <param name="authList">字段管理列表.</param>
    /// <param name="fields">功能模板控件集合.</param>
    /// <returns></returns>
    private async Task MenuMergeDataAuthScheme(string menuId, List<ModuleDataAuthorizeEntity> authList, List<FieldsModel> fields)
    {
        // 旧的自动生成的 方案管理
        List<ModuleDataAuthorizeSchemeEntity>? oldDataAuthScheme = await _visualDevRepository.Context.Queryable<ModuleDataAuthorizeSchemeEntity>()
            .Where(x => x.ModuleId == menuId && x.DeleteMark == null)
            .Where(x => x.FullName == "当前用户" || x.FullName == "当前组织" || x.FullName == "当前用户及下属" || x.FullName == "当前组织及子组织")
            .Where(x => x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userId\"")
            || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userAraSubordinates\"")
            || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizeId\"")
            || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizationAndSuborganization\""))
            .ToListAsync();

        List<ModuleDataAuthorizeSchemeEntity>? authSchemeList = new List<ModuleDataAuthorizeSchemeEntity>(); // 方案管理

        // 当前用户
        FieldsModel? item = fields.FirstOrDefault(x => x.__config__.qtKey == "createUser");
        if (item != null)
        {
            ModuleDataAuthorizeEntity? model = authList.FirstOrDefault(x => x.EnCode == item.__vModel__);

            if (model != null)
            {
                // 新增
                if (!oldDataAuthScheme.Any(x => x.EnCode == item.__vModel__ && x.ConditionText == "【{" + item.__config__.label + "} {等于} {@userId}】"))
                {
                    authSchemeList.Add(new ModuleDataAuthorizeSchemeEntity()
                    {
                        FullName = "当前用户",
                        EnCode = item.__vModel__,
                        ConditionText = "【{" + item.__config__.label + "} {等于} {@userId}】",
                        ConditionJson = "[{\"logic\":\"and\",\"groups\":[{\"id\":\"" + model.Id + "\",\"field\":\"" + item.__vModel__ + "\",\"type\":\"varchar\",\"op\":\"Equal\",\"value\":\"@userId\"}]}]",
                        ModuleId = menuId
                    });
                }

                if (!oldDataAuthScheme.Any(x => x.EnCode == item.__vModel__ && x.ConditionText == "【{" + item.__config__.label + "} {等于} {@userAraSubordinates}】"))
                {
                    authSchemeList.Add(new ModuleDataAuthorizeSchemeEntity()
                    {
                        FullName = "当前用户及下属",
                        EnCode = item.__vModel__,
                        ConditionText = "【{" + item.__config__.label + "} {等于} {@userAraSubordinates}】",
                        ConditionJson = "[{\"logic\":\"and\",\"groups\":[{\"id\":\"" + model.Id + "\",\"field\":\"" + item.__vModel__ + "\",\"type\":\"varchar\",\"op\":\"Equal\",\"value\":\"@userAraSubordinates\"}]}]",
                        ModuleId = menuId
                    });
                }

                // 删除
                List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme.Where(x => x.EnCode != item.__vModel__
                && (x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userId\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userAraSubordinates\""))).ToList();
                await _visualDevRepository.Context.Deleteable(delData).ExecuteCommandAsync();
            }
            else
            {
                // 删除
                List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme
                    .Where(x => x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userId\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userAraSubordinates\"")).ToList();
                await _visualDevRepository.Context.Deleteable(delData).ExecuteCommandAsync();
            }
        }
        else
        {
            // 删除
            List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme
                .Where(x => x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userId\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@userAraSubordinates\"")).ToList();
            await _visualDevRepository.Context.Deleteable(delData).ExecuteCommandAsync();
        }

        // 所属组织
        item = fields.FirstOrDefault(x => x.__config__.qtKey == "currOrganize");
        if (item != null)
        {
            ModuleDataAuthorizeEntity? model = authList.FirstOrDefault(x => x.EnCode == item.__vModel__);

            if (model != null)
            {
                // 新增
                if (!oldDataAuthScheme.Any(x => x.EnCode == item.__vModel__ && x.ConditionText == "【{" + item.__config__.label + "} {等于} {@organizeId}】"))
                {
                    authSchemeList.Add(new ModuleDataAuthorizeSchemeEntity()
                    {
                        FullName = "当前组织",
                        EnCode = item.__vModel__,
                        ConditionText = "【{" + item.__config__.label + "} {等于} {@organizeId}】",
                        ConditionJson = "[{\"logic\":\"and\",\"groups\":[{\"id\":\"" + model.Id + "\",\"field\":\"" + item.__vModel__ + "\",\"type\":\"varchar\",\"op\":\"Equal\",\"value\":\"@organizeId\"}]}]",
                        ModuleId = menuId
                    });
                }

                if (!oldDataAuthScheme.Any(x => x.EnCode == item.__vModel__ && x.ConditionText == "【{" + item.__config__.label + "} {等于} {@organizationAndSuborganization}】"))
                {
                    authSchemeList.Add(new ModuleDataAuthorizeSchemeEntity()
                    {
                        FullName = "当前组织及子组织",
                        EnCode = item.__vModel__,
                        ConditionText = "【{" + item.__config__.label + "} {等于} {@organizationAndSuborganization}】",
                        ConditionJson = "[{\"logic\":\"and\",\"groups\":[{\"id\":\"" + model.Id + "\",\"field\":\"" + item.__vModel__ + "\",\"type\":\"varchar\",\"op\":\"Equal\",\"value\":\"@organizationAndSuborganization\"}]}]",
                        ModuleId = menuId
                    });
                }

                // 删除
                List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme.Where(x => x.EnCode != item.__vModel__
                && (x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizeId\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizationAndSuborganization\""))).ToList();
                await _visualDevRepository.Context.Deleteable(delData).ExecuteCommandAsync();
            }
            else
            {
                // 删除
                List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme
                    .Where(x => x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizeId\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizationAndSuborganization\"")).ToList();
                await _visualDevRepository.Context.Deleteable(delData).ExecuteCommandAsync();
            }
        }
        else
        {
            // 删除
            List<ModuleDataAuthorizeSchemeEntity>? delData = oldDataAuthScheme
                .Where(x => x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizeId\"") || x.ConditionJson.Contains("\"op\":\"Equal\",\"value\":\"@organizationAndSuborganization\"")).ToList();
            await _visualDevRepository.Context.Deleteable(delData).ExecuteCommandAsync();
        }

        if (authSchemeList.Any()) await _visualDevRepository.Context.Insertable(authSchemeList).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
    }

    /// <summary>
    /// 无限递归 给控件绑定tableName (绕过 布局控件).
    /// </summary>
    /// <param name="dicFieldsModelList"></param>
    /// <param name="childTableDic"></param>
    /// <param name="tableName"></param>
    private void MainFieldsBindTable(List<Dictionary<string, object>> dicFieldsModelList, Dictionary<string, string> childTableDic, string tableName)
    {
        foreach (Dictionary<string, object>? item in dicFieldsModelList)
        {
            var obj = item["__config__"].ToObject<Dictionary<string, object>>();

            if (obj.ContainsKey("qtKey") && obj["qtKey"].Equals("table")) obj["tableName"] = childTableDic[item["__vModel__"].ToString()];
            else if(obj.ContainsKey("tableName")) obj["tableName"] = tableName;

            // 关联表单属性和弹窗属性
            if (obj.ContainsKey("qtKey") && (obj["qtKey"].Equals("relationFormAttr") || obj["qtKey"].Equals("popupAttr")))
            {
                string relationField = Convert.ToString(item["relationField"]);
                string? rField = relationField.ReplaceRegex(@"_qtTable_(\w+)", string.Empty);
                item["relationField"] = string.Format("{0}{1}{2}{3}", rField, "_qtTable_", tableName, "1");
            }

            // 递归
            if (obj.ContainsKey("children"))
            {
                var fmList = obj["children"].ToObject<List<Dictionary<string, object>>>();
                MainFieldsBindTable(fmList, childTableDic, tableName);
                obj["children"] = fmList;
            }

            item["__config__"] = obj;
        }
    }

    /// <summary>
    /// 子表绑定 tableName.
    /// </summary>
    private void ChildFieldBindTable(List<Dictionary<string, object>> dicFieldsModelList, Dictionary<string, string> childTableDic, string tableName)
    {
        foreach (var item in dicFieldsModelList)
        {
            var obj = item["__config__"].ToObject<Dictionary<string, object>>();
            if (obj.ContainsKey("qtKey") && obj["qtKey"].Equals(QtKeyConst.TABLE))
            {
                var cList = obj["children"].ToObject<List<Dictionary<string, object>>>();
                foreach (var child in cList)
                {
                    var cObj = child["__config__"].ToObject<Dictionary<string, object>>();
                    if (cObj.ContainsKey("relationTable")) cObj["relationTable"] = childTableDic[item["__vModel__"].ToString()];
                    else cObj.Add("relationTable", childTableDic[item["__vModel__"].ToString()]);

                    if (cObj.ContainsKey("tableName")) cObj["tableName"] = tableName;

                    // 关联表单属性和弹窗属性
                    if (cObj.ContainsKey("qtKey") && (cObj["qtKey"].Equals("relationFormAttr") || cObj["qtKey"].Equals("popupAttr")))
                    {
                        string relationField = Convert.ToString(child["relationField"]);
                        string? rField = relationField.ReplaceRegex(@"_qtTable_(\w+)", string.Empty);
                        if (child.ContainsKey("relationField")) child["relationField"] = string.Format("{0}{1}{2}{3}", rField, "_qtTable_", cObj["tableName"], "0");
                        else child.Add("relationField", string.Format("{0}{1}{2}{3}", rField, "_qtTable_", cObj["tableName"], "0"));
                    }

                    child["__config__"] = cObj;
                }

                obj["children"] = cList;
            }

            // 递归
            if (obj.ContainsKey("children"))
            {
                var fmList = obj["children"].ToObject<List<Dictionary<string, object>>>();
                ChildFieldBindTable(fmList, childTableDic, tableName);
                obj["children"] = fmList;
            }

            item["__config__"] = obj;
        }
    }
    #endregion
}
