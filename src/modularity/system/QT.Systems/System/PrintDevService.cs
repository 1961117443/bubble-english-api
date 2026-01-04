using System.Data;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.PrintDev;
using QT.Systems.Entitys.Model.PrintDev;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using QT.WorkFlow.Entitys.Entity;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Systems.Entitys.Entity.System;
using QT.Systems.Entitys.Dto.System.PrintDev;
using QT.Systems.Entitys.Enum;
using System.Text.RegularExpressions;
using QT.Common;
using QT.Common.Models.User;

namespace QT.Systems;

/// <summary>
/// 打印模板配置
/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "PrintDev", Order = 200)]
[Route("api/system/[controller]")]
public class PrintDevService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<PrintDevEntity> _repository;

    /// <summary>
    /// 数据字典服务.
    /// </summary>
    private readonly IDictionaryDataService _dictionaryDataService;

    /// <summary>
    /// 数据连接服务.
    /// </summary>
    private readonly IDbLinkService _dbLinkService;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 数据库管理.
    /// </summary>
    private readonly IDataBaseManager _dataBaseManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 系统参数
    /// </summary>
    private static Dictionary<string, Func<object>> _sysParameters = new();
    static PrintDevService()
    {
        _sysParameters.Add("@当前用户", () => App.GetService<IUserManager>()?.RealName ?? "-");
        _sysParameters.Add("@当前时间", () => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    /// <summary>
    /// 初始化一个<see cref="PrintDevService"/>类型的新实例.
    /// </summary>
    public PrintDevService(
        ISqlSugarRepository<PrintDevEntity> printDevRepository,
        IDictionaryDataService dictionaryDataService,
        IFileManager fileManager,
        IDataBaseManager dataBaseManager,
        IUserManager userManager,
        IDbLinkService dbLinkService,
        ISqlSugarClient context)
    {
        _repository = printDevRepository;
        _dictionaryDataService = dictionaryDataService;
        _dbLinkService = dbLinkService;
        _fileManager = fileManager;
        _dataBaseManager = dataBaseManager;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    #region Get

    /// <summary>
    /// 列表(分页).
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList_Api([FromQuery] PrintDevListInput input)
    {
        var list = await _repository.Context.Queryable<PrintDevEntity, UserEntity, UserEntity, DictionaryDataEntity>((a, b, c, d) =>
         new JoinQueryInfos(JoinType.Left, b.Id == a.CreatorUserId, JoinType.Left, c.Id == a.LastModifyUserId, JoinType.Left, a.Category == d.EnCode))
            .Where((a, b, c, d) => a.DeleteMark == null && d.DictionaryTypeId == "202931027482510597").WhereIF(input.category.IsNotEmptyOrNull(), a => a.Category == input.category)
            .WhereIF(input.keyword.IsNotEmptyOrNull(), a => a.FullName.Contains(input.keyword) || a.EnCode.Contains(input.keyword))
            .WhereIF(input.moduleName.IsNotEmptyOrNull(), a=> SqlFunc.Subqueryable<ModuleEntity>().Where(it => it.FullName.Contains(input.moduleName) && SqlFunc.Subqueryable<ModuleRelationEntity>()
                                                                                .Where(mre => it.Id == mre.ModuleId && mre.ObjectType == ModuleRelationType.PrintDev && mre.ObjectId == a.Id).Any()).Any())
            .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select((a, b, c, d) => new PrintDevListOutput
            {
                category = d.FullName,
                id = a.Id,
                fullName = a.FullName,
                creatorTime = a.CreatorTime,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                enCode = a.EnCode,
                enabledMark = a.EnabledMark,
                lastModifyTime = a.LastModifyTime,
                lastModifyUser = SqlFunc.MergeString(c.RealName, "/", c.Account),
                sortCode = a.SortCode,
                type = a.Type,
            }).ToPagedListAsync(input.currentPage, input.pageSize);

        if (list.list.IsAny())
        {
            var printIds = list.list.Select(x => x.id).ToArray();
            var prints = await _repository.Context.Queryable<ModuleEntity, ModuleRelationEntity>((a, b) => new JoinQueryInfos(JoinType.Inner, a.Id == b.ModuleId))
                .Where((a, b) => b.ObjectType == ModuleRelationType.PrintDev && printIds.Contains(b.ObjectId))
                .Select((a, b) => new { a.FullName, b.ObjectId })
                .ToListAsync();

            if (prints.IsAny())
            {
                foreach (var item in list.list)
                {
                    item.moduleName = prints.Where(it=>it.ObjectId == item.id).Select(it=>it.FullName).ToArray();
                }
            }
        }

        return PageResult<PrintDevListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetList_Api([FromQuery] string type)
    {
        var list = await _repository.Context.Queryable<PrintDevEntity, UserEntity, UserEntity, DictionaryDataEntity>((a, b, c, d) => new JoinQueryInfos(JoinType.Left, b.Id == a.CreatorUserId, JoinType.Left, c.Id == a.LastModifyUserId, JoinType.Left, a.Category == d.EnCode))
            .Where((a, b, c, d) => a.DeleteMark == null && d.DictionaryTypeId == "202931027482510597" && a.EnabledMark == 1).OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .WhereIF(type.IsNotEmptyOrNull(), (a) => a.Type == type.ParseToInt())
            .Select((a, b, c, d) => new PrintDevListOutput
            {
                category = a.Category,
                id = a.Id,
                fullName = a.FullName,
                creatorTime = a.CreatorTime,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                enCode = a.EnCode,
                enabledMark = a.EnabledMark,
                lastModifyTime = a.LastModifyTime,
                lastModifyUser = SqlFunc.MergeString(c.RealName, "/", c.Account),
                sortCode = a.SortCode,
                type = a.Type,
                parentId = d.Id,
            }).ToListAsync();

        // 数据库分类
        var dbTypeList = (await _dictionaryDataService.GetList("printDev")).FindAll(x => x.EnabledMark == 1);
        var result = new List<PrintDevListOutput>();
        foreach (var item in dbTypeList)
        {
            var index = list.FindAll(x => x.category.Equals(item.EnCode)).Count;
            if (index > 0)
            {
                result.Add(new PrintDevListOutput()
                {
                    id = item.Id,
                    parentId = "0",
                    fullName = item.FullName,
                    num = index
                });
            }
        }

        return new { list = result.OrderBy(x => x.sortCode).Union(list).ToList().ToTree() };
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        return (await GetInfo(id)).Adapt<PrintDevInfoOutput>();
    }

    /// <summary>
    /// 导出.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}/Actions/Export")]
    public async Task<dynamic> ActionsExport(string id)
    {
        var importModel = await GetInfo(id);
        var jsonStr = importModel.ToJsonString();
        return await _fileManager.Export(jsonStr, importModel.FullName, ExportFileType.bp);
    }

    /// <summary>
    /// 表单字段.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("Fields")]
    public async Task<dynamic> GetFields([FromBody] PrintDevFieldsQuery input)
    {
        var link = await _dbLinkService.GetInfo(input.dbLinkId);
        var tenantLink = link ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);

        var parameter = new List<SugarParameter>()
        {
                new SugarParameter("@formId", null)
        };
        var sqlList = input.sqlTemplate.ToList<PrintDevSqlModel>();
        var output = new Dictionary<string, object>();
        var index = 0;

        // 匹配自定义的变量 {}
        string pattern = @"\{([^\{\}]+)\}";
        foreach (var item in sqlList)
        {
            if (item.sql.IsNullOrEmpty())
                throw Oops.Oh(ErrorCode.COM1005);

            // 匹配自定义的变量 @[^\s]+\s?            
            foreach (Match match in Regex.Matches(item.sql, pattern))
            {
                var p = match.Groups[1].Value;
                if (!string.IsNullOrEmpty(p))
                {
                    // 判断是否@开头，统一整理成@开头的变量
                    if (!p.StartsWith("@"))
                    {
                        p = $"@{p}";
                    }

                    if (!parameter.Any(x=>x.ParameterName == p))
                    {
                        parameter.Add(new SugarParameter(p, null));
                    }

                    item.sql = Regex.Replace(item.sql, match.Groups[0].Value, p);
                }

            }

            var dataTable = _dataBaseManager.GetInterFaceData(tenantLink, item.sql, parameter.ToArray());
            var fieldModes = GetFieldModels(dataTable);
            if (index == 0)
                output.Add("headTable", fieldModes);
            else
                output.Add("childrenDataTable" + (index - 1), fieldModes);
            ++index;
        }

        output.Add("sysTable", _sysParameters.Select(it => new PrintDevFieldModel
        {
            field = it.Key,
            fieldName = it.Key
        }).ToList());

        return output;
    }

    /// <summary>
    /// 模板数据.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet("Data")]
    public async Task<dynamic> GetData([FromQuery] PrintDevSqlDataQuery input)
    {
        var output = new PrintDevDataOutput();
        var entity = await GetInfo(input.id);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);
        var link = await _dbLinkService.GetInfo(entity.DbLinkId);
        var tenantLink = link ?? _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
        var parameter = new List<SugarParameter>()
        {
                new SugarParameter("@formId", input.formId)
        };
        var sqlList = entity.SqlTemplate.ToList<PrintDevSqlModel>();

        #region 处理自定义变量
        var userInfo = await _userManager.GetUserInfo();
        // 获取变量的值
        Func<string, object?> getParameterValue = key =>
        {
            //如果参数是@开头的话，先去掉进行判断
            key = key.TrimStart('@');
            // 1、先从请求的url中判断
            if (App.HttpContext!=null)
            {
                var p = App.HttpContext.Request.Query.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (p.Key!=null)
                {
                    return p.Value.ToString();
                }
               
            }


            // 2、获取userinfo的值
            if (userInfo!=null)
            {
                var p = EntityHelper<UserInfoModel>.InstanceProperties.FirstOrDefault(x => x.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (p!=null)
                {
                    return p.GetValue(userInfo, null);
                }
            }

            return null;
        };
        // 匹配自定义的变量 {}
        string pattern = @"\{([^\{\}]+)\}";
        foreach (var item in sqlList)
        {
            if (item.sql.IsNullOrEmpty())
                throw Oops.Oh(ErrorCode.COM1005);

            // 匹配自定义的变量 @[^\s]+\s?            
            foreach (Match match in Regex.Matches(item.sql, pattern))
            {
                var p = match.Groups[1].Value;
                if (!string.IsNullOrEmpty(p))
                {
                    // 判断是否@开头，统一整理成@开头的变量
                    if (!p.StartsWith("@"))
                    {
                        p = $"@{p}";
                    }

                    if (!parameter.Any(x => x.ParameterName == p))
                    {
                        parameter.Add(new SugarParameter(p, getParameterValue(p)));
                    }

                    item.sql = Regex.Replace(item.sql, match.Groups[0].Value, p);
                }
            }
        } 
        #endregion

        var dataTable = _dataBaseManager.GetInterFaceData(tenantLink, sqlList.FirstOrDefault().sql, parameter.Select(x=>new SugarParameter(x.ParameterName,x.Value)).ToArray());
        //if (dataTable.Rows.Count>1)
        //{

        //}
        var dic = DateConver(DataTableToDicList(dataTable)).FirstOrDefault() ?? new Dictionary<string, object>();
        for (int i = 1; i < sqlList.Count; i++)
        {
            if (sqlList[i].sql.IsNullOrEmpty())
                throw Oops.Oh(ErrorCode.COM1005);
            var childDataTable = _dataBaseManager.GetInterFaceData(tenantLink, sqlList[i].sql, parameter.Select(x => new SugarParameter(x.ParameterName, x.Value)).ToArray());
            if (childDataTable.Rows.Count > 0)
                dic.Add("childrenDataTable" + (i - 1), DateConver(DataTableToDicList(childDataTable)));
        }

        // 添加系统变量
        foreach (var item in _sysParameters)
        {
            dic.TryAdd(item.Key, item.Value());
        }

        output.printData = dic;
        output.printTemplate = entity.PrintTemplate;
        output.operatorRecordList = await _repository.Context.Queryable<FlowTaskOperatorRecordEntity>()
            .Where(a => a.TaskId == input.formId)
            .Select(a => new PrintDevDataModel()
            {
                id = a.Id,
                handleId = a.HandleId,
                handleOpinion = a.HandleOpinion,
                handleStatus = a.HandleStatus,
                nodeCode = a.NodeCode,
                handleTime = a.HandleTime,
                nodeName = a.NodeName,
                signImg = a.SignImg,
                taskId=a.TaskId,
                operatorId = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.OperatorId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                userName = SqlFunc.Subqueryable<UserEntity>().Where(u => u.Id == a.HandleId).Select(u => SqlFunc.MergeString(u.RealName, "/", u.Account)),
                status=a.Status,
                taskNodeId=a.TaskNodeId,
                taskOperatorId=a.TaskOperatorId,
            }).ToListAsync();
        return output;
    }

    /// <summary>
    /// 获取打印模块关联的模块.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}/Actions/Module")]
    public async Task<List<string>> ActionsModule(string id)
    {
        var list = await _repository.Context.Queryable<ModuleRelationEntity>()
            .Where(it => it.ObjectType == ModuleRelationType.PrintDev && it.ObjectId == id)
            .Select(it=>it.ModuleId)
            .ToListAsync();

        return list;
    }

    /// <summary>
    /// 获取所有模块关联的打印模板.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("Actions/GetAllTemplateData")]
    public async Task<Dictionary<string, List<PrintDevInfoSelectorOutput>>> GetAllTemplateData()
    {
        var relationList = await _repository.Context.Queryable<ModuleRelationEntity>()
           .Where(it => it.ObjectType == ModuleRelationType.PrintDev)
           .ToListAsync();

         var list =  await _repository.AsQueryable()
             .Where(it =>  it.EnabledMark == 1 && it.DeleteMark == null)
             .Select(it => new PrintDevInfoSelectorOutput { id = it.Id, fullName = it.FullName, enCode = it.EnCode, propertyJson = it.PropertyJson })
             .ToListAsync();

        //relationList.GroupBy(x=>x.ModuleId)
        Dictionary<string, List<PrintDevInfoSelectorOutput>> result = relationList.GroupBy(x => x.ModuleId)
            .ToDictionary(x => x.Key, x => list.Where(it => x.Any(w => w.ObjectId == it.id)).ToList());

        return result ?? new Dictionary<string, List<PrintDevInfoSelectorOutput>>();
    }
    #endregion

    #region Post

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create_Api([FromBody] PrintDevCrInput input)
    {
        if (await _repository.AnyAsync(x => x.EnCode == input.enCode && x.DeleteMark == null) || await _repository.AnyAsync(x => x.FullName == input.fullName && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<PrintDevEntity>();
        entity.EnabledMark = 1;
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
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
        var isOk = await _repository.Context.Updateable<PrintDevEntity>().SetColumns(it => new PrintDevEntity()
        {
            DeleteMark = 1,
            DeleteUserId = _userManager.UserId,
            DeleteTime = SqlFunc.GetDate()
        }).Where(it => it.Id.Equals(id)).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 修改.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update_Api(string id, [FromBody] PrintDevUpInput input)
    {
        if (await _repository.AnyAsync(x => x.Id != id && x.EnCode == input.enCode && x.DeleteMark == null) || await _repository.AnyAsync(x => x.Id != id && x.FullName == input.fullName && x.DeleteMark == null))
            throw Oops.Oh(ErrorCode.COM1004);
        var entity = input.Adapt<PrintDevEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 修改状态.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task ActionsState_Api(string id)
    {
        var isOk = await _repository.Context.Updateable<BillRuleEntity>().SetColumns(it => new BillRuleEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyUserId = _userManager.UserId,
            LastModifyTime = SqlFunc.GetDate()
        }).Where(it => it.Id.Equals(id)).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1003);
    }

    /// <summary>
    /// 复制.
    /// </summary>
    /// <param name="id">主键值</param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Copy")]
    public async Task ActionsCopy(string id)
    {
        var entity = await GetInfo(id);
        var random = new Random().NextLetterAndNumberString(5).ToLower();
        entity.FullName = entity.FullName + "副本" + random;
        entity.EnabledMark = 0;
        entity.EnCode += random;
        if (entity.FullName.Length >= 50 || entity.EnCode.Length >= 50)
            throw Oops.Oh(ErrorCode.COM1009);
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
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
        if (!fileType.ToLower().Equals(ExportFileType.bp.ToString()))
            throw Oops.Oh(ErrorCode.D3006);
        var josn = _fileManager.Import(file);
        var model = josn.ToObject<PrintDevEntity>();
        if (model == null || model.SqlTemplate.IsNullOrEmpty())
            throw Oops.Oh(ErrorCode.D3006);
        var isOk = await _repository.Context.Storageable(model).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.D3008);
    }

    /// <summary>
    /// 绑定打印模块关联的模块.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Module")]
    //[ServiceFilter(typeof(SqlSugarUnitOfWorkFilter))]
    [SqlSugarUnitOfWork]
    public async Task ActionsModule([FromRoute]string id,[FromBody]PrintDevBindModuleInput input)
    {
        var list = await _repository.Context.Queryable<ModuleRelationEntity>()
            .Where(it => it.ObjectType == ModuleRelationType.PrintDev && it.ObjectId == id)
            .ToListAsync();

        List<ModuleRelationEntity> insertList = new List<ModuleRelationEntity>();
        foreach (var moduleId in input.items)
        {
            var item = list.Find(x => x.ModuleId == moduleId);
            if (item!=null)
            {
                list.Remove(item);
                continue;
            }
            item = new ModuleRelationEntity
            {
                ModuleId = moduleId,
                ObjectId = id,
                ObjectType = ModuleRelationType.PrintDev
            };
            item.Creator();
            insertList.Add(item);
        }

        if (list.Any())
        {
            await _repository.Context.Deleteable(list).ExecuteCommandAsync();
        }

        if (insertList.Any())
        {
            await _repository.Context.Insertable(insertList).ExecuteCommandAsync();
        }
    }
    #endregion

    #region PublicMethod

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<PrintDevEntity> GetInfo(string id)
    {
        return await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 获取字段模型.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    private List<PrintDevFieldModel> GetFieldModels(DataTable dt)
    {
        var models = new List<PrintDevFieldModel>();
        foreach (var item in dt.Columns)
        {
            models.Add(new PrintDevFieldModel()
            {
                field = item.ToString(),
                fieldName = item.ToString(),
            });
        }

        return models;
    }

    /// <summary>
    /// DataTable转DicList.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    private List<Dictionary<string, object>> DataTableToDicList(DataTable dt)
    {
        return dt.AsEnumerable().Select(
                row => dt.Columns.Cast<DataColumn>().ToDictionary(
                column => column.ColumnName,
                column => row[column])).ToList();
    }

    /// <summary>
    /// 动态表单时间格式转换.
    /// </summary>
    /// <param name="diclist"></param>
    /// <returns></returns>
    private List<Dictionary<string, object>> DateConver(List<Dictionary<string, object>> diclist)
    {
        foreach (var item in diclist)
        {
            foreach (var dic in item.Keys)
            {
                if (item[dic] is DateTime)
                {
                    item[dic] = item[dic].ToString() + " ";
                }
            }
        }

        return diclist;
    }
    #endregion
}