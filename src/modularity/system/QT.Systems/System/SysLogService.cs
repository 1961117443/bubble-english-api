using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.LinqBuilder;
using QT.Logging.Attributes;
using QT.Systems.Entitys.Dto.SysLog;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.Reflection;
using QT.Common.Core.Entity;
using QT.Systems.Entitys.Permission;
using QT.JsonSerialization;
using Newtonsoft.Json.Linq;

namespace QT.Systems;

/// <summary>
/// 系统日志



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "Log", Order = 211)]
[Route("api/system/[controller]")]
public class SysLogService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<SysLogEntity> _repository;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="SysLogService"/>类型的新实例.
    /// </summary>
    public SysLogService(
        ISqlSugarRepository<SysLogEntity> sysLogRepository,
        ISqlSugarClient context)
    {
        _repository = sysLogRepository;
        _db = context.AsTenant();
    }

    #region GET

    /// <summary>
    /// 获取系统日志列表-登录日志（带分页）.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <param name="Type">分类.</param>
    /// <returns></returns>
    [HttpGet("{Type}")]
    public async Task<dynamic> GetList([FromQuery] LogListQuery input, int Type)
    {
        var whereLambda = LinqExpression.And<SysLogEntity>();
        whereLambda = whereLambda.And(x => x.Category == Type);
        var start = new DateTime();
        var end = new DateTime();
        if (input.endTime != null && input.startTime != null)
        {
            start = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 00:00:00}", input.startTime?.TimeStampToDateTime()));
            end = Convert.ToDateTime(string.Format("{0:yyyy-MM-dd 23:59:59}", input.endTime?.TimeStampToDateTime()));
            whereLambda = whereLambda.And(x => SqlFunc.Between(x.CreatorTime, start, end));
        }


        // 关键字（用户、IP地址、功能名称）
        if (!string.IsNullOrEmpty(input.keyword))
            whereLambda = whereLambda.And(m => m.UserName.Contains(input.keyword) || m.IPAddress.Contains(input.keyword) || m.ModuleName.Contains(input.keyword));
        if (input.ipaddress.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(m => m.IPAddress.Contains(input.ipaddress));
        if (input.userName.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(m => m.UserName.Contains(input.userName));
        if (input.moduleName.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(m => m.ModuleName == input.moduleName);
        if (input.requestMethod.IsNotEmptyOrNull())
            whereLambda = whereLambda.And(m => m.RequestMethod == input.requestMethod);
        var list = Type == 6 ? null : (await _repository.Context.UseLogDatabase().Queryable<SysLogEntity>().SplitTable().Where(whereLambda).OrderBy(x => x.CreatorTime, OrderByType.Desc).ToPagedListAsync(input.currentPage, input.pageSize));
        object output = null;
        switch (Type)
        {
            case 1:
                {
                    var pageList = new SqlSugarPagedList<LogLoginOutput>()
                    {
                        list = list.list.Adapt<List<LogLoginOutput>>(),
                        pagination = list.pagination
                    };
                    return PageResult<LogLoginOutput>.SqlSugarPageResult(pageList);
                }

            case 3:
                {
                    var pageList = new SqlSugarPagedList<LogOperationOutput>()
                    {
                        list = list.list.Adapt<List<LogOperationOutput>>(),
                        pagination = list.pagination
                    };
                    return PageResult<LogOperationOutput>.SqlSugarPageResult(pageList);
                }

            case 4:
                {
                    var pageList = new SqlSugarPagedList<LogExceptionOutput>()
                    {
                        list = list.list.Adapt<List<LogExceptionOutput>>(),
                        pagination = list.pagination
                    };
                    return PageResult<LogExceptionOutput>.SqlSugarPageResult(pageList);
                }

            case 5:
                {
                    var pageList = new SqlSugarPagedList<LogRequestOutput>()
                    {
                        list = list.list.Adapt<List<LogRequestOutput>>(),
                        pagination = list.pagination
                    };
                    return PageResult<LogRequestOutput>.SqlSugarPageResult(pageList);
                }
            case 6:
                {
                    var diffList = await _repository.Context.UseLogDatabase().Queryable<SysLogDiff>()
                        .WhereIF(input.endTime != null && input.startTime != null, x => SqlFunc.Between(x.CreateTime, start, end))
                        .WhereIF(!string.IsNullOrEmpty(input.keyword),x=> x.Sql.Contains(input.keyword))
                        .WhereIF(input.logKey.IsNotEmptyOrNull(), x=>x.BusinessData == input.logKey)
                        //.Select(x=> new SysLogDiffOutput
                        //{
                        //    createUserIdName = SqlFunc.Subqueryable<UserEntity>().Where(ddd => ddd.Id == x.CreateUserId).Select(ddd => ddd.RealName)
                        //}, true)
                        .SplitTable()
                        .OrderBy(x => x.CreateTime, OrderByType.Desc)
                        .ToPagedListAsync(input.currentPage, input.pageSize);

                    //
                    var pageResult = PageResult<SysLogDiffOutput>.SqlSugarPagedList(diffList);
                    if (diffList.list.IsAny())
                    {
                        var userIds = diffList.list.Select(x => x.CreateUserId).ToList();
                        var users= await _repository.Context.Queryable<UserEntity>().Where(x => userIds.Contains(x.Id)).Select(x => new UserEntity { Id = x.Id, RealName = x.RealName }).ToListAsync();

                        foreach (var item in pageResult.list)
                        {
                            //var beforeObj = item.beforeData.IsNotEmptyOrNull() ? JObject.Parse(item.beforeData) : JObject.Parse("{}");
                            //var afterObj = item.afterData.IsNotEmptyOrNull() ? JObject.Parse(item.afterData) : JObject.Parse("{}");
                            item.createUserIdName = users.FirstOrDefault(x => x.Id == item.createUserId)?.RealName ?? "";
                        }
                    }

                    return pageResult;
                }
        }
        return output;
    }

    /// <summary>
    /// 操作模块.
    /// </summary>
    /// <returns></returns>
    [HttpGet("ModuleName")]
    public async Task<dynamic> ModuleNameSelector()
    {
        return App.EffectiveTypes
                .Where(u => u.IsClass && !u.IsInterface && !u.IsAbstract && typeof(IDynamicApiController).IsAssignableFrom(u))
                .SelectMany(u => u.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                .Where(x => x.IsDefined(typeof(OperateLogAttribute), false))
                .Select(x => new { moduleName = x.GetCustomAttribute<OperateLogAttribute>().ModuleName }).Distinct();
    }

    #endregion

    #region POST

    /// <summary>
    /// 批量删除.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpDelete]
    [SqlSugarUnitOfWork]
    public async Task Delete([FromBody] LogDelInput input)
    {
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        await _repository.Context.Deleteable<SysLogEntity>().In(it => it.Id, input.ids).SplitTable().ExecuteCommandAsync();

        //    _db.CommitTran();
        //}
        //catch (Exception)
        //{
        //    _db.RollbackTran();
        //    throw Oops.Oh(ErrorCode.COM1001);
        //}
    }

    /// <summary>
    /// 批量删除.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpDelete("deleteBy/{type}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(int type, [FromBody] LogDelInput input)
    {
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();

        if (type == 6)
        {
            await _repository.Context.Deleteable<SysLogDiff>().In(it => it.Id, input.ids).ExecuteCommandAsync();
        }
        else
        {
            await _repository.Context.Deleteable<SysLogEntity>().In(it => it.Id, input.ids).SplitTable().ExecuteCommandAsync();
        }

        //    _db.CommitTran();
        //}
        //catch (Exception)
        //{
        //    _db.RollbackTran();
        //    throw Oops.Oh(ErrorCode.COM1001);
        //}
    }

    /// <summary>
    /// 一键删除.
    /// </summary>
    /// <param name="type">请求参数.</param>
    /// <returns></returns>
    [HttpDelete("{type}")]
    [SqlSugarUnitOfWork]
    public async Task Delete(int type)
    {
        //try
        //{
        //    // 开启事务
        //    _db.BeginTran();
        if (type == 6)
        {
            await _repository.Context.Deleteable<SysLogDiff>().ExecuteCommandAsync();
        }
        else
        {
            await _repository.Context.Deleteable<SysLogEntity>().Where(x => x.Category == type).SplitTable().ExecuteCommandAsync();
        }

        //    _db.CommitTran();
        //}
        //catch (Exception)
        //{
        //    _db.RollbackTran();
        //    throw Oops.Oh(ErrorCode.COM1001);
        //}
    }

    #endregion
}