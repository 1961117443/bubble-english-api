using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json.Linq;
using QT.Common.Cache;
using QT.Common.Configuration;
using QT.Common.Core.Manager;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Core.Security;
using QT.Common.Dtos.VisualDev;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Net;
using QT.Common.Security;
using QT.DataEncryption;
using QT.DependencyInjection;
using QT.FriendlyException;
using QT.RemoteRequest.Extensions;
using QT.Systems.Entitys.Model.DataInterFace;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Systems;

/// <summary>
/// 租户接口执行器
/// </summary>
/// <typeparam name="T">租户标识</typeparam>
public class TenantDataInterfaceActuator<T> : IDataInterfaceActuator,ITransient
{
    private readonly SqlSugarClient _sqlSugarClient;

    private readonly IUserManager? _userManager;

    /// <summary>
    /// 当前租户号
    /// </summary>
    private static readonly string _tenantId = TenantCacheFactory.GetTenantId<T>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sqlSugarClient"></param>
    public TenantDataInterfaceActuator(ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = (SqlSugarClient)sqlSugarClient;
        _sqlSugarClient.ChangeDatabase(_tenantId);
    }


    /// <inheritdoc/>
    public async Task<object> GetResponseByType(string id, int type, string tenantId, VisualDevDataFieldDataListInput input = null, Dictionary<string, string> dicParameters = null)
    {
        try
        {
            var sw = new Stopwatch();
            sw.Start();
            if (KeyVariable.MultiTenancy)
            {
                if (!_sqlSugarClient.IsAnyConnection(tenantId))
                {
                    //App.GetService<ITenantManager>()?.Login(tenantId);
                    if (!TenantManager.Login(tenantId, _sqlSugarClient))
                    {
                        throw Oops.Oh("租户数据库登录失败！");
                    }
                }

                _sqlSugarClient.ChangeDatabase(tenantId);
                //_configId = tenantId;
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
                    UserId = _userManager?.UserId ?? _tenantId,
                    InvokIp = httpContext.GetLocalIpAddressToIPv4(),
                    InvokDevice = string.Format("{0}-{1}", UserAgent.GetSystem(), UserAgent.GetBrowser()),
                    InvokWasteTime = (int)sw.ElapsedMilliseconds,
                    InvokType = httpContext.Request.Method
                };
                await _sqlSugarClient.Insertable(log).ExecuteCommandAsync();
            }

            #endregion

            return output;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    /// <summary>
    /// 替换参数默认值.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dic"></param>
    private void ReplaceParameterValue(DataInterfaceEntity entity, Dictionary<string, string> dic)
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

    /// <summary>
    /// 查询.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private async Task<DataTable> GetData(DataInterfaceEntity entity)
    {
        return await connection(entity.DBLinkId, entity.Query, entity.RequestMethod);
    }

    /// <summary>
    /// 通过连接执行sql.
    /// </summary>
    /// <returns></returns>
    private async Task<DataTable> connection(string dbLinkId, string sql, string reqMethod)
    {
        throw new NotImplementedException();
        //var link = await _sqlSugarClient.Queryable<DbLinkEntity>().FirstAsync(x => x.Id == dbLinkId && x.DeleteMark == null);

        //var tenantLink = link ?? await GetTenantDbLink();
        //var parameter = new List<SugarParameter>();
        //if (_userManager.ToKen.IsNotEmptyOrNull())
        //{
        //    sql = sql.Replace("@user", "'" + _userManager.UserId + "'"); // orcale关键字处理
        //    parameter.Add(new SugarParameter("@organize", _userManager.User.OrganizeId));
        //    parameter.Add(new SugarParameter("@department", _userManager.User.OrganizeId));
        //    parameter.Add(new SugarParameter("@postion", _userManager.User.PositionId));
        //}

        //if (reqMethod.Equals("3"))
        //{
        //    return _dataBaseManager.GetInterFaceData(tenantLink, sql, parameter.ToArray());
        //}
        //else
        //{
        //    _dataBaseManager.ExecuteCommand(tenantLink, sql, parameter.ToArray());
        //    return new DataTable();
        //}
    }

    /// <summary>
    /// DataTable 数据分页.
    /// </summary>
    /// <param name="dt">数据源.</param>
    /// <param name="PageIndex">第几页.</param>
    /// <param name="PageSize">每页多少条.</param>
    /// <returns></returns>
    private DataTable GetPageToDataTable(DataTable dt, int PageIndex, int PageSize)
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
        if (_userManager!=null && _userManager.ToKen != null)
            dicHerader.Add("Authorization", _userManager.ToKen);

        if (App.HttpContext != null)
        {
            foreach (var item in App.HttpContext.Request.Headers)
            {
                if (item.Key.StartsWith("x-", StringComparison.OrdinalIgnoreCase))
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
            addr = $"{(App.HttpContext.Request.IsHttps ? "https" : "http")}://{App.HttpContext.Request.Host.Value}/{addr.TrimStart('/')}";
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
        if (_userManager != null && _userManager.ToKen != null)
            dicHerader.Add("Authorization", _userManager.ToKen);
        foreach (var key in parameters)
        {
            dic.Add(key.field, key.defaultValue);
        }

        foreach (var key in parametersHerader)
        {
            dicHerader[key.field] = key.defaultValue;
        }

        switch (entity.RequestMethod)
        {
            case "6":
                result = await entity.Path.SetHeaders(dicHerader).SetQueries(dic).GetAsStringAsync();
                break;
            case "7":
                result = await entity.Path.SetHeaders(dicHerader).SetBody(dic).PostAsStringAsync();
                break;
        }

        return result;
    }
}
