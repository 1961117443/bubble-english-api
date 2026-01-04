using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using QT.Common.Configuration;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Files;
using QT.Common.Core.Security;
using QT.Common.Dtos.OAuth;
using QT.Common.Dtos.VisualDev;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Net;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extras.Thirdparty.JSEngine;
using QT.FriendlyException;
using QT.LinqBuilder;
using QT.Logging.Attributes;
using QT.RemoteRequest.Extensions;
using QT.SensitiveDetection;
using QT.Systems.Entitys.Dto.DataInterFace;
using QT.Systems.Entitys.Dto.System.DataInterFace;
using QT.Systems.Entitys.Model.DataInterFace;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using QT.UnifyResult;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json.Linq;
using SqlSugar;
using QT.Common.Core.Manager.Tenant;
using Serilog;
using QT.Common.Cache;
using QT.Extras.DatabaseAccessor.SqlSugar;
using Microsoft.Extensions.Caching.Memory;

namespace QT.Systems;

/// <summary>
/// 数据接口



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "DataInterface", Order = 204)]
[Route("api/system/[controller]")]
public class DataInterfaceService : IDataInterfaceService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<DataInterfaceEntity> _repository;

    /// <summary>
    /// 数据字典服务.
    /// </summary>
    private readonly IDictionaryDataService _dictionaryDataService;

    /// <summary>
    /// 脱敏词汇提供器.
    /// </summary>
    private readonly ISensitiveDetectionProvider _sensitiveDetectionProvider;

    /// <summary>
    /// 数据库管理.
    /// </summary>
    private readonly IDataBaseManager _dataBaseManager;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化 SqlSugar 客户端.
    /// </summary>
    private readonly SqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 数据库上下文ID.
    /// </summary>
    private string _configId = App.Configuration["ConnectionStrings:ConfigId"];

    /// <summary>
    /// 数据库名称.
    /// </summary>
    private string _dbName = App.Configuration["ConnectionStrings:DBName"];

    /// <summary>
    /// 初始化一个<see cref="DataInterfaceService"/>类型的新实例.
    /// </summary>
    public DataInterfaceService(
        ISqlSugarRepository<DataInterfaceEntity> dataInterfaceRepository,
        IDictionaryDataService dictionaryDataService,
        IDataBaseManager dataBaseManager,
        IUserManager userManager,
        IFileManager fileManager,
        ISensitiveDetectionProvider sensitiveDetectionProvider,
        ISqlSugarClient context)
    {
        _sensitiveDetectionProvider = sensitiveDetectionProvider;
        _repository = dataInterfaceRepository;
        _dictionaryDataService = dictionaryDataService;
        _fileManager = fileManager;
        _dataBaseManager = dataBaseManager;
        _userManager = userManager;
        _sqlSugarClient = (SqlSugarClient)context;
        _db = context.AsTenant();
    }

    #region Get

    /// <summary>
    /// 获取接口列表(分页).
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] DataInterfaceListQuery input)
    {
        var list = await _repository.Context.Queryable<DataInterfaceEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.CreatorUserId))
            .Where(a => a.DeleteMark == null)
            .WhereIF(!string.IsNullOrEmpty(input.categoryId), a => a.CategoryId == input.categoryId)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.FullName.Contains(input.keyword) || a.EnCode.Contains(input.keyword))
            .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select((a, b) => new DataInterfaceListOutput
            {
                id = a.Id,
                categoryId = a.CategoryId,
                creatorTime = a.CreatorTime,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                dataType = a.DataType,
                dbLinkId = a.DBLinkId,
                description = a.Description,
                enCode = a.EnCode,
                fullName = a.FullName,
                enabledMark = a.EnabledMark,
                path = a.Path,
                query = a.Query,
                requestMethod = SqlFunc.IF(a.RequestMethod.Equals("1")).Return("新增").ElseIF(a.RequestMethod.Equals("2")).Return("修改")
                .ElseIF(a.RequestMethod.Equals("3")).Return("查询").ElseIF(a.RequestMethod.Equals("4")).Return("删除")
                .ElseIF(a.RequestMethod.Equals("5")).Return("存储过程").ElseIF(a.RequestMethod.Equals("6")).Return("Get")
                .End("Post"),
                requestParameters = a.RequestParameters,
                responseType = a.ResponseType,
                sortCode = a.SortCode,
                checkType = a.CheckType,
                tenantId = _userManager.TenantId
            }).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<DataInterfaceListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 获取接口列表(分页).
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpGet("getList")]
    public async Task<dynamic> getList([FromQuery] DataInterfaceListQuery input)
    {
        var list = await _repository.Context.Queryable<DataInterfaceEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.CreatorUserId))
            .Where(a => a.DeleteMark == null)
            .WhereIF(!string.IsNullOrEmpty(input.categoryId), a => a.CategoryId == input.categoryId)
            .WhereIF(!string.IsNullOrEmpty(input.dataType), a => a.DataType.ToString() == input.dataType)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.FullName.Contains(input.keyword) || a.EnCode.Contains(input.keyword))
            .OrderBy(a => a.SortCode).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .Select((a, b) => new DateInterfaceGetListOutput
            {
                id = a.Id,
                categoryId = a.CategoryId,
                creatorTime = a.CreatorTime,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                _dataType = a.DataType,
                dbLinkId = a.DBLinkId,
                description = a.Description,
                enCode = a.EnCode,
                fullName = a.FullName,
                enabledMark = a.EnabledMark,
                path = a.Path,
                query = a.Query,
                requestMethod = SqlFunc.IF(a.RequestMethod.Equals("1")).Return("新增").ElseIF(a.RequestMethod.Equals("2")).Return("修改")
                .ElseIF(a.RequestMethod.Equals("3")).Return("查询").ElseIF(a.RequestMethod.Equals("4")).Return("删除")
                .ElseIF(a.RequestMethod.Equals("5")).Return("存储过程").ElseIF(a.RequestMethod.Equals("6")).Return("Get")
                .End("Post"),
                requestParameters = a.RequestParameters,
                responseType = a.ResponseType,
                sortCode = a.SortCode,
                checkType = a.CheckType,
                tenantId = _userManager.TenantId
            }).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<DateInterfaceGetListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 获取接口列表下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("Selector")]
    public async Task<dynamic> GetSelector()
    {
        List<DataInterfaceSelectorOutput> tree = new List<DataInterfaceSelectorOutput>();
        foreach (var entity in (await _repository.ToListAsync(x => x.DeleteMark == null && x.EnabledMark == 1)).OrderBy(x => x.SortCode).ToList())
        {
            var dictionaryDataEntity = await _dictionaryDataService.GetInfo(entity.CategoryId);
            if (dictionaryDataEntity != null && tree.Where(t => t.id == entity.CategoryId).Count() == 0)
            {
                DataInterfaceSelectorOutput firstModel = dictionaryDataEntity.Adapt<DataInterfaceSelectorOutput>();
                firstModel.categoryId = "0";
                DataInterfaceSelectorOutput treeModel = entity.Adapt<DataInterfaceSelectorOutput>();
                treeModel.categoryId = "1";
                treeModel.parentId = dictionaryDataEntity.Id;
                firstModel.children.Add(treeModel);
                tree.Add(firstModel);
            }
            else
            {
                DataInterfaceSelectorOutput treeModel = entity.Adapt<DataInterfaceSelectorOutput>();
                treeModel.categoryId = "1";
                treeModel.parentId = entity.CategoryId;
                var parent = tree.Where(t => t.id == entity.CategoryId).FirstOrDefault();
                if (parent != null)
                {
                    parent.children.Add(treeModel);
                }
            }
        }

        return tree.OrderBy(x => x.sortCode).ToList();
    }

    /// <summary>
    /// 获取接口数据.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        return (await GetInfo(id)).Adapt<DataInterfaceInfoOutput>();
    }

    /// <summary>
    /// 预览接口.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/Actions/Preview")]
    public async Task<dynamic> Preview(string id, [FromBody] DataInterfacePreviewInput input)
    {
        _configId = _userManager.TenantId;
        _dbName = _userManager.TenantDbName;
        object output = null;
        var info = await GetInfo(id);
        info.RequestParameters = input.paramList.ToJsonString();
        ReplaceParameterValue(info, new Dictionary<string, string>());
        if (info.DataType == 1)
        {
            output = await GetData(info);
        }
        else if (info.DataType == 2)
        {
            output = info.Query.ToObject<object>();
        }
        else
        {
            output = await GetApiDataByTypePreview(info);
        }
        if (info.DataProcessing.IsNullOrEmpty())
        {
            return output;
        }
        else
        {
            string sheetData = Regex.Match(info.DataProcessing, @"\{(.*)\}", RegexOptions.Singleline).Groups[1].Value;
            var scriptStr = "var result = function(data){data = JSON.parse(data);" + sheetData + "}";
            var obj = JsEngineUtil.CallFunction(scriptStr, output.ToJsonString());
            return obj;
        }
    }

    /// <summary>
    /// 获取预览参数.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("GetParam/{id}")]
    public async Task<dynamic> GetParam(string id)
    {
        var info = await GetInfo(id);
        if (info.IsNotEmptyOrNull() && info.RequestParameters.IsNotEmptyOrNull())
        {
            return info.RequestParameters.ToList<DataInterfaceReqParameter>();
        }
        else
        {
            return new List<DataInterfaceReqParameter>();
        }
    }

    /// <summary>
    /// 访问接口.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="tenantId">有值则为地址请求，没有则是内部请求.</param>
    /// <returns></returns>
    [AllowAnonymous]
    [IgnoreLog]
    [HttpGet("{id}/Actions/Response")]
    public async Task<dynamic> ActionsResponse(string id, [FromQuery] string tenantId)
    {
        return await GetResponseByType(id, 2, tenantId);
    }

    /// <summary>
    /// 访问接口 分页.
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [IgnoreLog]
    [HttpGet("{id}/Action/List")]
    public async Task<dynamic> ActionsResponseList(string id, [FromQuery] string tenantId, [FromQuery] VisualDevDataFieldDataListInput input)
    {
        return await GetResponseByType(id, 0, tenantId, input);
    }

    /// <summary>
    /// 访问接口 选中 回写.
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [IgnoreLog]
    [HttpGet("{id}/Action/Info")]
    public async Task<dynamic> ActionsResponseInfo(string id, [FromQuery] string tenantId, [FromQuery] VisualDevDataFieldDataListInput input)
    {
        return await GetResponseByType(id, 1, tenantId, input);
    }

    /// <summary>
    /// 导出.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/Action/Export")]
    public async Task<dynamic> ActionsExport(string id)
    {
        var data = await GetInfo(id);
        var jsonStr = data.ToJsonString();
        return await _fileManager.Export(jsonStr, data.FullName, ExportFileType.bd);
    }

    #endregion

    #region Post

    /// <summary>
    /// 添加接口.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] DataInterfaceCrInput input)
    {
        var entity = input.Adapt<DataInterfaceEntity>();
        if (entity.DataType == 1 && await _sensitiveDetectionProvider.VaildedAsync(entity.Query.ToUpper()))
                throw Oops.Oh(ErrorCode.xg1005);
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 修改接口.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] DataInterfaceUpInput input)
    {
        var entity = input.Adapt<DataInterfaceEntity>();
        if (entity.DataType == 1 && await _sensitiveDetectionProvider.VaildedAsync(entity.Query.ToUpper()))
            throw Oops.Oh(ErrorCode.xg1005);
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除接口.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete_Api(string id)
    {
        var isOk = await _repository.Context.Updateable<DataInterfaceEntity>().SetColumns(it => new DataInterfaceEntity()
        {
            DeleteMark = 1,
            DeleteTime = DateTime.Now,
            DeleteUserId = _userManager.UserId
        }).Where(it => it.Id == id).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <summary>
    /// 更新接口状态.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/State")]
    public async Task UpdateState(string id)
    {
        var isOk = await _repository.Context.Updateable<DataInterfaceEntity>().SetColumns(it => new DataInterfaceEntity()
        {
            EnabledMark = SqlFunc.IIF(it.EnabledMark == 1, 0, 1),
            LastModifyTime = DateTime.Now,
            LastModifyUserId = _userManager.UserId
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
        if (!fileType.ToLower().Equals(ExportFileType.bd.ToString()))
            throw Oops.Oh(ErrorCode.D3006);
        var josn = _fileManager.Import(file);
        var data = josn.ToObject<DataInterfaceEntity>();
        if (data == null)
            throw Oops.Oh(ErrorCode.D3006);
        var isOk = await _repository.Context.Storageable(data).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.D3008);
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<DataInterfaceEntity> GetInfo(string id)
    {
        return await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
    }

    /// <summary>
    /// 查询.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<DataTable> GetData(DataInterfaceEntity entity)
    {
        return await connection(entity.DBLinkId, entity.Query, entity.RequestMethod);
    }

    /// <summary>
    /// 根据不同类型请求接口.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type">0 ： 分页 1 ：详情 ，其他 原始.</param>
    /// <param name="tenantId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<object> GetResponseByType(string id, int type, string tenantId, VisualDevDataFieldDataListInput input = null, Dictionary<string, string> dicParameters = null)
    {
        try
        {
            var sw = new Stopwatch();
            sw.Start();
            if (KeyVariable.MultiTenancy)
            {
                tenantId = tenantId.IsNullOrEmpty() ? _userManager.TenantId : tenantId;

                if (!_sqlSugarClient.IsAnyConnection(tenantId))
                {
                    Log.Information($"GetResponseByType，租户不存在【{tenantId}】,当前【{_sqlSugarClient.CurrentConnectionConfig.ConfigId}】");
                    //App.GetService<ITenantManager>()?.Login(tenantId);
                    if (!TenantManager.Login(tenantId, _sqlSugarClient))
                    {
                        throw Oops.Oh("租户数据库登录失败！");
                    }
                }

                /*
                var interFace = App.Configuration["Tenant:MultiTenancyDBInterFace"] + tenantId;
                var response = await interFace.GetAsStringAsync();
                var result = response.ToObject<RESTfulResult<TenantInterFaceOutput>>();
                if (result.code != 200)
                    throw Oops.Oh(result.msg);
                else if (result.data.dotnet == null)
                    throw Oops.Oh(ErrorCode.D1025);
                if (!_sqlSugarClient.IsAnyConnection(tenantId))
                {
                    _sqlSugarClient.AddConnection(new ConnectionConfig()
                    {
                        DbType = (SqlSugar.DbType)Enum.Parse(typeof(SqlSugar.DbType), App.Configuration["ConnectionStrings:DBType"]),
                        ConfigId = tenantId, // 设置库的唯一标识
                        IsAutoCloseConnection = true,
                        ConnectionString = string.Format($"{App.Configuration["ConnectionStrings:DefaultConnection"]}", result.data.dotnet)
                    });
                }
                */

                _sqlSugarClient.ChangeDatabase(tenantId);
                _configId = tenantId;
                //_dbName = result.data.dotnet;
            }

            var data = await _sqlSugarClient.Queryable<DataInterfaceEntity>().FirstAsync(x => x.Id == id && x.DeleteMark == null);
            if (input.IsNotEmptyOrNull())
            {
                if (!string.IsNullOrWhiteSpace(input.relationField) && !string.IsNullOrWhiteSpace(input.keyword))
                    data.Query = string.Format("select * from ({0}) t where {1}='{2}' ", data.Query.TrimEnd(';'), input.relationField, input.keyword);
                if (!string.IsNullOrWhiteSpace(input.propsValue) && !string.IsNullOrWhiteSpace(input.id))
                    data.Query = string.Format("select * from ({0}) t where {1}='{2}' ", data.Query.TrimEnd(';'), input.propsValue, input.id);
            }

            if (dicParameters.IsNullOrEmpty())
                dicParameters = new Dictionary<string, string>();
            ReplaceParameterValue(data, dicParameters);
            object output = null;

            #region 授权判断

            if (data == null)
            {
                throw Oops.Oh(ErrorCode.COM1005);
            }
            else if (data.CheckType == 1)
            {
                var tokenStr = App.HttpContext.Request.Headers["Authorization"].ToString();
                if (tokenStr.IsNullOrEmpty())
                    throw Oops.Oh(ErrorCode.D9007);
                var token = new JsonWebToken(tokenStr.Replace("Bearer ", string.Empty));
                var flag = JWTEncryption.ValidateJwtBearerToken((DefaultHttpContext)App.HttpContext, out token);
                if (!flag)
                    throw Oops.Oh(ErrorCode.D9007);
            }
            else if (data.CheckType == 2)
            {
                var ipList = data.IpAddress.Split(",").ToList();
                if (!ipList.Contains(App.HttpContext.GetLocalIpAddressToIPv4()))
                    throw Oops.Oh(ErrorCode.D9002);
            }

            #endregion

            #region 调用接口

            if (1.Equals(data.DataType))
            {
                var resTable = await GetData(data);
                if (type == 0)
                {
                    // 分页
                    var dt = GetPageToDataTable(resTable, input.currentPage, input.pageSize);
                    var res = new
                    {
                        pagination = new PageResult()
                        {
                            pageIndex = input.currentPage,
                            pageSize = input.pageSize,
                            total = resTable.Rows.Count
                        },
                        list = dt.ToObject<List<Dictionary<string, object>>>(),
                        dataProcessing = data.DataProcessing
                    };

                    output = res;
                }
                else if (type == 1)
                {
                    output = resTable.ToObject<List<Dictionary<string, object>>>().FirstOrDefault();
                }
                else
                {
                    output = new { data = resTable, dataProcessing = data.DataProcessing };
                }

            }
            else if (2.Equals(data.DataType))
            {

                output = new { data = data.Query.ToObject<object>(), dataProcessing = data.DataProcessing };
            }
            else
            {
                if (type == 0)
                {
                    output = await GetApiDataPagination(data);
                }
                else if (type == 1)
                {
                    output = await GetApiDataByType(data);
                    var resObj = output.ToObject<JObject>();
                    if (resObj.ContainsKey("list"))
                    {
                        var resList = resObj["list"].ToObject<List<Dictionary<string, object>>>();
                        return resList.Find(x => x.ContainsKey(input.propsValue) && x.ContainsValue(input.id));
                    }
                }
                else
                {
                    output = new { data = await GetApiDataByType(data), dataProcessing = data.DataProcessing };
                }
            }
            #endregion
            sw.Stop();

            #region 插入日志

            if (App.HttpContext.IsNotEmptyOrNull())
            {
                var httpContext = App.HttpContext;
                var headers = httpContext.Request.Headers;
                var log = new DataInterfaceLogEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    InvokId = id,
                    InvokTime = DateTime.Now,
                    UserId = _userManager.UserId,
                    InvokIp = httpContext.GetLocalIpAddressToIPv4(),
                    InvokDevice = string.Format("{0}-{1}", UserAgent.GetSystem(), UserAgent.GetBrowser()),
                    InvokWasteTime = (int)sw.ElapsedMilliseconds,
                    InvokType = httpContext.Request.Method
                };
                await _sqlSugarClient.Insertable(log).ExecuteCommandAsync();
            }

            #endregion
            Log.Information($"GetResponseByType成功【{id}】");
            return output;
        }
        catch (Exception e)
        {
            Log.Error(e, $"GetResponseByType失败【{id}】");
            return null;
        }
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 通过连接执行sql.
    /// </summary>
    /// <returns></returns>
    private async Task<DataTable> connection(string dbLinkId, string sql, string reqMethod)
    {
        var link = new DbLinkEntity();

        if (!_repository.Context.IsAnyConnection(_configId))
        {
            Log.Information($"connection：当前ConfigId=【{_repository.Context.CurrentConnectionConfig.ConfigId}】,【{_configId}】找不到");
            var cache = App.GetService<IMemoryCache>();
            var tenantList = cache?.Get<List<ITenantDbInfo>>("tenant:dbcontext:list");
            if (tenantList.IsAny() && tenantList.Any(x=>x.enCode==_configId))
            {
                var tenant = tenantList.Find(x => x.enCode == _configId);
                if (tenant is TenantInterFaceOutput tenantInterFace)
                {
                    _sqlSugarClient.AddConnection(SqlSugarHelper.CreateConnectionConfig(_configId, tenantInterFace));
                }
            }
        }

        if (!_repository.Context.IsAnyConnection(_configId))
        {
            link = await _sqlSugarClient.Queryable<DbLinkEntity>().FirstAsync(x => x.Id == dbLinkId && x.DeleteMark == null);
        }
        else
        {
            link = await _repository.Context.Queryable<DbLinkEntity>().FirstAsync(x => x.Id == dbLinkId && x.DeleteMark == null);
        }

        var tenantLink = link ?? await GetTenantDbLink();
        var parameter = new List<SugarParameter>();
        if (_userManager.ToKen.IsNotEmptyOrNull())
        {
            sql = sql.Replace("@user", "'" + _userManager.UserId + "'"); // orcale关键字处理
            parameter.Add(new SugarParameter("@organize", _userManager.User.OrganizeId));
            parameter.Add(new SugarParameter("@department", _userManager.User.OrganizeId));
            parameter.Add(new SugarParameter("@postion", _userManager.User.PositionId));
        }

        if (reqMethod.Equals("3"))
        {
            return _dataBaseManager.GetInterFaceData(tenantLink, sql, parameter.ToArray());
        }
        else
        {
            _dataBaseManager.ExecuteCommand(tenantLink, sql, parameter.ToArray());
            return new DataTable();
        }
    }

    /// <summary>
    /// 根据不同规则请求接口.
    /// </summary>
    /// <param name="entity">实体对象.</param>
    /// <returns></returns>
    private async Task<object> GetApiDataByType(DataInterfaceEntity entity)
    {
        var result = string.Empty;
        var parameters = entity.RequestParameters.ToObject<List<DataInterfaceReqParameter>>();
        var parametersHerader = entity.RequestHeaders.ToObject<List<DataInterfaceReqParameter>>();
        var dic = new Dictionary<string, object>();
        var dicHerader = new Dictionary<string, object>();
        dicHerader.Add("QT_API", true);
        if (_userManager.ToKen != null)
            dicHerader.Add("Authorization", _userManager.ToKen);
        foreach (var key in parameters)
        {
            dic.Add(key.field, key.defaultValue);
        }

        foreach (var key in parametersHerader)
        {
            dicHerader[key.field] = key.defaultValue;
        }
        var addr = entity.Path;
        if (!addr.TrimStart().StartsWith("http"))
        {
            addr = $"{(App.HttpContext.Request.IsHttps ? "https" : "http")}://{App.HttpContext.Request.Host.Value}/{addr.TrimStart('/')}";
        }
        switch (entity.RequestMethod)
        {
            case "6":
                result = await addr.SetHeaders(dicHerader).SetQueries(dic).GetAsStringAsync();
                break;
            case "7":
                result = await addr.SetHeaders(dicHerader).SetBody(dic).PostAsStringAsync();
                break;
        }

        return result;
    }

    /// <summary>
    /// 根据不同规则请求接口(预览).
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private async Task<object> GetApiDataByTypePreview(DataInterfaceEntity entity)
    {
        var result = new object();
        var parameters = entity.RequestParameters.ToObject<List<DataInterfaceReqParameter>>();
        var parametersHerader = entity.RequestHeaders.ToObject<List<DataInterfaceReqParameter>>();
        var dic = new Dictionary<string, object>();
        var dicHerader = new Dictionary<string, object>();
        dicHerader.Add("QT_API", true);
        if (_userManager.ToKen != null)
            dicHerader.Add("Authorization", _userManager.ToKen);

        if (App.HttpContext!=null)
        {
            foreach (var item in App.HttpContext.Request.Headers)
            {
                if (item.Key.StartsWith("x-",StringComparison.OrdinalIgnoreCase))
                {
                    dicHerader[item.Key] = item.Value;
                }
            }
        }
        
        foreach (var key in parameters)
        {
            dic.Add(key.field, key.defaultValue);
        }

        foreach (var key in parametersHerader)
        {
            dicHerader[key.field] = key.defaultValue;
        }

        var addr = entity.Path;
        if (!addr.TrimStart().StartsWith("http"))
        {
            addr = $"{(App.HttpContext.Request.IsHttps? "https":"http")}://{App.HttpContext.Request.Host.Value}/{addr.TrimStart('/')}";
        }
        JObject jobject = new JObject();
        switch (entity.RequestMethod)
        {
            case "6":
                jobject = (await addr.SetHeaders(dicHerader).SetQueries(dic).GetAsStringAsync()).ToObject<JObject>();
                break;
            case "7":
                jobject = (await addr.SetHeaders(dicHerader).SetBody(dic).PostAsStringAsync()).ToObject<JObject>();
                break;
        }

        if (jobject.ContainsKey("code") && jobject.ContainsKey("msg") && jobject.ContainsKey("data"))
        {
            result = jobject["data"];
        }
        else
        {
            result = jobject;
        }

        return result;
    }

    /// <summary>
    /// 根据不同规则请求接口(分页).
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private async Task<object> GetApiDataPagination(DataInterfaceEntity entity)
    {
        var result = await GetApiDataByTypePreview(entity);
        if (result == null) return result;
        var jobj = result.ToObject<JObject>();
        var value = jobj.ContainsKey("list") ? jobj["list"] : jobj;
        return new { list = value, dataProcessing = entity.DataProcessing };
    }

    /// <summary>
    /// DataTable 数据分页.
    /// </summary>
    /// <param name="dt">数据源.</param>
    /// <param name="PageIndex">第几页.</param>
    /// <param name="PageSize">每页多少条.</param>
    /// <returns></returns>
    public static DataTable GetPageToDataTable(DataTable dt, int PageIndex, int PageSize)
    {
        if (PageIndex == 0)
            return dt; // 0页代表每页数据，直接返回

        if (dt == null)
        {
            return new DataTable();
        }

        DataTable newdt = dt.Copy();
        newdt.Clear(); // copy dt的框架

        int rowbegin = (PageIndex - 1) * PageSize;
        int rowend = PageIndex * PageSize; // 要展示的数据条数

        if (rowbegin >= dt.Rows.Count)
            return dt; // 源数据记录数小于等于要显示的记录，直接返回dt

        if (rowend > dt.Rows.Count)
            rowend = dt.Rows.Count;
        for (int i = rowbegin; i <= rowend - 1; i++)
        {
            DataRow newdr = newdt.NewRow();
            DataRow dr = dt.Rows[i];
            foreach (DataColumn column in dt.Columns)
            {
                newdr[column.ColumnName] = dr[column.ColumnName];
            }

            newdt.Rows.Add(newdr);
        }

        return newdt;
    }

    /// <summary>
    /// 获取多租户Link.
    /// </summary>
    /// <returns></returns>
    public async Task<DbLinkEntity> GetTenantDbLink()
    {
        return new DbLinkEntity
        {
            Id = _configId,
            ServiceName = _dbName,
            DbType = App.Configuration["ConnectionStrings:DBType"],
            Host = App.Configuration["ConnectionStrings:Host"],
            Port = App.Configuration["ConnectionStrings:Port"].ParseToInt(),
            UserName = App.Configuration["ConnectionStrings:UserName"],
            Password = App.Configuration["ConnectionStrings:Password"]
        };
    }

    /// <summary>
    /// 替换参数默认值.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dic"></param>
    public void ReplaceParameterValue(DataInterfaceEntity entity, Dictionary<string, string> dic)
    {
        if (dic.IsNotEmptyOrNull() && entity.IsNotEmptyOrNull() && entity.RequestParameters.IsNotEmptyOrNull())
        {
            var parameterList = entity.RequestParameters.ToList<DataInterfaceReqParameter>();
            foreach (var item in parameterList)
            {
                if (dic.Keys.Contains(item.field))
                    item.defaultValue = dic[item.field].ToString();
                if (entity.DataType == 1)
                    entity.Query = entity.Query?.Replace("{" + item.field + "}", "'" + item.defaultValue + "'");
                else
                    entity.Query = entity.Query?.Replace("{" + item.field + "}", item.defaultValue);
            }

            entity.RequestParameters = parameterList.ToJsonString();
        }
    }

    #endregion
}