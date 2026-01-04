using Microsoft.AspNetCore.Authorization;
using QT.Logistics.Entitys.Dto.LogArticle;
using QT.Logistics.Entitys;
using QT.Systems.Entitys.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QT.Logging.Attributes;
using QT.Common.Const;
using QT.Logistics.Entitys.Dto.LogPCWeb;
using QT.DataEncryption;
using System.Reactive;
using System.Reflection;
using NPOI.SS.Formula.Functions;
using QT.Systems.Entitys.Enum;
using QT.Common.Extension;
using QT.Logistics.Manager;
using Senparc.Weixin.Work.AdvancedAPIs.OaDataOpen;
using QT.Common.Security;
using QT.Logistics.Entitys.Dto.LogEnterprise;
using QT.Common.Models.User;
using NPOI;
using Mapster;
using QT.Common.Models;
using QT.Extras.Thirdparty.Sms;
using QT.Common.Net;
using QT.DataValidation;
using Newtonsoft.Json.Linq;
using QT.Common.Core.Service;
using QT.Logistics.Entitys.Options;

namespace QT.Logistics;

/// <summary>
/// 业务实现：pc端前台.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "PC端前台服务", Name = "pcweb", Order = 200)]
[Route("api/Logistics/[controller]")]
[AllowAnonymous]
[IgnoreLog]
public class LogPCWebService : IDynamicApiController
{
    private ISqlSugarRepository<LogArticleEntity> _repository;
    private readonly ICacheManager _cacheManager;
    private readonly ICoreSysConfigService _coreSysConfigService;

    /// <summary>
    /// 初始化一个<see cref="LogArticleService"/>类型的新实例.
    /// </summary>
    public LogPCWebService(
        ISqlSugarRepository<LogArticleEntity> LogArticleRepository,
        ICacheManager cacheManager,
        ICoreSysConfigService coreSysConfigService)
    {
        _repository = LogArticleRepository;
        _cacheManager = cacheManager;
        _coreSysConfigService = coreSysConfigService;
    }

    /// <summary>
    /// 获取登录配置
    /// </summary>
    /// <returns></returns>
    [HttpGet("loginConfig")]
    public dynamic GetLoginConfig()
    {
        return new
        {
            adminAddress = App.Configuration["Logistics:pcweb:adminAddress"]
        };
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("LogArticle/{id}")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        int.TryParse(id, out var idInt);
        var entity = await _repository.Context
            .Queryable<LogArticleEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.CreatorUserId == b.Id))
            .Where(a => (a.Id == id || a.UniqueKey == id || (idInt < 2000 && a.Type == idInt)) && a.DeleteMark == null /*&& a.EnabledMark == 1*/)
            .Select((a, b) => new LogArticleInfoOutput
            {
                id = a.Id,
                lastModifyTime = a.LastModifyTime,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                title = a.Title,
                bodyText = a.BodyText,
                files = a.Files,
                toUserIds = a.ToUserIds,
                readCount = a.IsRead ?? 0
            }).FirstAsync();

        if (entity == null)
        {
            return null;
        }

        entity.readCount++;
        // 更新阅读次数
        await _repository.Context.Updateable<LogArticleEntity>()
            .SetColumns(it => it.IsRead, entity.readCount)
            .Where(it => it.Id == entity.id).ExecuteCommandAsync();

        return entity;
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("LogArticle/List")]
    public async Task<dynamic> GetLogArticleList([FromQuery] LogArticleNoticeInput input)
    {
        var list = await _repository.Context.Queryable<LogArticleEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.CreatorUserId))
            .WhereIF(input.type.HasValue, a => a.Type == input.type && a.DeleteMark == null)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.Title.Contains(input.keyword))
            .Where(a => a.EnabledMark == 1)
            .OrderBy(a => a.EnabledMark).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b) => new LogArticleNoticeOutput
            {
                id = a.Id,
                lastModifyTime = a.EnabledMark == 1 ? a.LastModifyTime : null,
                enabledMark = a.EnabledMark,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                title = a.Title,
                type = a.Type,
                creatorTime = a.CreatorTime,
            }).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogArticleNoticeOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 获取商家
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/LogEnterprise/v1")]
    public async Task<dynamic> GetLogEnterpriseV1([FromQuery] PageInputBase input)
    {
        var list = await _repository.Context.Queryable<LogEnterpriseEntity>()
            .Where(it => it.Status == 1)
            //.Select(it => new LogEnterpriseWebListOutput
            //{
            //    id = it.Id,
            //    admin = SqlFunc.Subqueryable<UserEntity>().Where(x => x.Id == it.AdminId).Select(x => x.RealName),
            //    phone = it.Phone,
            //    name = it.Name
            //})
            .ToPagedListAsync(input.currentPage, input.pageSize);

        var result = list.Adapt<SqlSugarPagedList<LogEnterpriseWebListOutput>>();

        return PageResult<LogEnterpriseWebListOutput>.SqlSugarPageResult(result);
    }

    /// <summary>
    /// 获取商家以及推荐商品
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/LogEnterprise")]
    public async Task<dynamic> GetLogEnterprise([FromQuery] PageInputBase input)
    {
        var list = await _repository.Context.Queryable<LogEnterpriseEntity>()
            .Where(it => it.Status == 1)
            .Where(it => SqlFunc.Subqueryable<LogEnterpriseSupplyProductEntity>().Where(x => x.EId == it.Id && x.State == 1).Any())
            .Select(it => new LogEnterpriseWebListOutput
            {
                id = it.Id,
                admin = SqlFunc.Subqueryable<UserEntity>().Where(x => x.Id == it.AdminId).Select(x => x.RealName),
                phone = it.Phone,
                name = it.Name
            })
            .ToPagedListAsync(input.currentPage, input.pageSize);
        if (list.list.IsAny())
        {
            var idList = list.list.Select(it => it.id).ToArray();

            var productList = await _repository.Context
                .Queryable<LogEnterpriseSupplyProductEntity>().ClearFilter<ILogEnterpriseEntity>()
                .Where(it => idList.Contains(it.EId) && it.State == 1)
                .Select(it => new
                {
                    index2 = SqlFunc.RowNumber(it.Id, it.EId),
                    it.Id,
                    it.EId,
                    it.Name,
                    it.Producer,
                    it.Storage,
                    it.Retention,
                    it.Remark,
                    it.ImageUrl
                })
            .MergeTable()//将结果合并成一个表
            .Where(it => it.index2 <= 10) //相同的name只取一条记录
            //.Select(it => new LogEnterpriseProductWebListOutput
            //{
            //    id = it.Id,
            //    name = it.Name,
            //    producer = it.Producer,
            //    remark = it.Remark,
            //    retention = it.Retention,
            //    storage = it.Storage,
            //    eid = it.EId,
            //})
            .Select(it => new LogEnterpriseSupplyProductEntity { }, true)
           .ToListAsync();

            foreach (var item in list.list)
            {
                item.items = productList.Where(x => x.EId == item.id).ToList().Adapt<List<LogEnterpriseProductWebListOutput>>() ?? new List<LogEnterpriseProductWebListOutput>();
            }
        }

        return PageResult<LogEnterpriseWebListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 获取商家详细信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/LogEnterprise/{id}")]
    public async Task<LogEnterpriseWebInfoOutput> GetLogEnterprise([FromRoute] string id)
    {
        var entity = await _repository.Context.Queryable<LogEnterpriseEntity>()
            .Where(it => it.Status == 1)
            .InSingleAsync(id) ?? throw Oops.Oh("商家不存在！");

        var data = entity.Adapt<LogEnterpriseWebInfoOutput>();
        if (!string.IsNullOrEmpty(entity.PropertyJson))
        {
            data.properties = entity.PropertyJson.ToObject<List<LogEnterpriseProperty>>()
                .Where(x => x.enable)
                .Select(x => new LogEnterprisePropertyWebInfoOutput
                {
                    label = x.title ?? x.prop,
                    value = x.value
                }).ToList();
        }
        else
        {
            data.properties = new List<LogEnterprisePropertyWebInfoOutput>();
        }

        if (!string.IsNullOrEmpty(entity.AdminId))
        {
            data.admin = await _repository.Context.Queryable<UserEntity>().Where(x => x.Id == entity.AdminId).Select(it => it.RealName).FirstAsync();
        }
        return data;
    }

    /// <summary>
    /// 获取商家商品列表
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/LogEnterprise/{id}/Product")]
    public async Task<dynamic> GetLogEnterpriseProduct([FromRoute] string id, [FromQuery] PageInputBase input)
    {
        var list = await _repository.Context
                .Queryable<LogEnterpriseSupplyProductEntity>().ClearFilter<ILogEnterpriseEntity>()
                .Where(it => it.EId == id && it.State == 1)
                //.Select(it => new LogEnterpriseProductWebListOutput
                //{
                //    id = it.Id,
                //    name = it.Name,
                //    producer = it.Producer,
                //    remark = it.Remark,
                //    eid = it.EId,
                //    retention = it.Retention,
                //    storage = it.Storage
                //})
                .ToPagedListAsync(input.currentPage, input.pageSize);

        var result = list.Adapt<SqlSugarPagedList<LogEnterpriseProductWebListOutput>>();

        return PageResult<LogEnterpriseProductWebListOutput>.SqlSugarPageResult(result);
    }

    /// <summary>
    /// 获取商家商品详情
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/LogEnterprise/Product/{id}")]
    public async Task<dynamic> GetLogEnterpriseProduct([FromRoute] string id)
    {
        var entity = await _repository.Context
                .Queryable<LogEnterpriseSupplyProductEntity>().ClearFilter<ILogEnterpriseEntity>()
                .Where(it => it.Id == id && it.State == 1)
                //.Select(it => new LogEnterpriseProductWebListOutput
                //{
                //    id = it.Id,
                //    name = it.Name,
                //    producer = it.Producer,
                //    remark = it.Remark,
                //    eid = it.EId,
                //    retention = it.Retention,
                //    storage = it.Storage
                //}, true)
                .FirstAsync();

        return new
        {
            info = entity.Adapt<LogEnterpriseProductWebListOutput>(),
            emp = await GetLogEnterprise(entity.EId)
        };
    }

    /// <summary>
    /// 获取短信验证码
    /// 5分钟内有效，1分钟后可以重新获取
    /// </summary>
    /// <param name="mobile"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    [HttpGet("VerificationCode/{mobile}")]
    public async Task GetSmsCode(string mobile)
    {
        // 验证是否有效的手机号码
        if (mobile.IsNullOrEmpty())
        {
            throw Oops.Oh("手机号码不能为空！");
        }
        if (!mobile.TryValidate(validationTypes: ValidationTypes.PhoneNumber).IsValid)
        {
            throw Oops.Oh("请输入正确的手机号码！");
        }

        // 判断手机号码是否注册
        if (await _repository.Context.Queryable<LogMemberEntity>().Where(it => it.PhoneNumber == mobile).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.D1003);
        }

        // 同一个ip,一分钟内最多发5个号码
        var key = $"{nameof(LogPCWebService)}:VerificationCode:{NetHelper.Ip}";
        var now = DateTimeOffset.UtcNow;
        var list = await RedisHelper.ZRangeByScoreAsync(key, now.AddMinutes(-1).ToUnixTimeSeconds(), now.ToUnixTimeSeconds());
        if (list.Count() > 5 || list.Contains(mobile))
        {
            throw Oops.Oh("请求太频繁，请稍后再试！");
        }

        var options = await _coreSysConfigService.GetConfig<LogisticConfigs>();

        if (options.thirdAppKey.IsNullOrEmpty() || options.thirdAppSecret.IsNullOrEmpty() || options.thirdServer.IsNullOrEmpty())
        {
            throw Oops.Oh("三网短信缺少配置！");
        }

        // 删除一分钟之前的数据
        //await RedisHelper.ZRemRangeByScoreAsync(key, 0, now.AddMinutes(-1).ToUnixTimeSeconds());

        //NetHelper.Ip;

        string cacheKey = string.Format("pcweb:VerificationCode:{0}", mobile);
        var code = Random.Shared.Next(1000, 9999);
        Console.WriteLine("验证码-{0}:{1}", cacheKey, code);
        //if (mobile == "13536688943")
        //{
        //    throw Oops.Oh("短信发送失败！");
        //}

        //        【云上智慧】尊敬的用户，您的{ 0}
        //    操作验证码为: { 1}，有效期{ 2}
        //        分钟，请勿泄露他人。
        //【云上智慧】尊敬的用户，您的验证码: { 0}，有效期{ 1}
        //        分钟，工作人员不会索取，请勿转发或泄漏。
        //【云上智慧】验证码：{ 0}，您正在使用{ 1}
        //        功能，有效期{ 2}
        //        分钟，请勿转发或泄漏。

        int cache = 5;


        var jsonStr = await SmsUtil.SendSmsByThird(new SmsParameterInfo
        {
            keyId = options.thirdAppKey,
            keySecret = options.thirdAppSecret,
            domain = options.thirdServer,
            mobileAli = mobile,
            content = $"【云上智慧】尊敬的用户，您的验证码: {code}，有效期{cache}分钟，工作人员不会索取，请勿转发或泄漏。"
        });

        if (jsonStr.IsNotEmptyOrNull())
        {
            var jo = JObject.Parse(jsonStr);
            if (jo["status"]!=null && jo.Value<int>("status") == 1)
            {
                await _cacheManager.SetAsync(cacheKey, code.ToString(), TimeSpan.FromMinutes(cache));


                // 写入缓存
                await RedisHelper.ZAddAsync(key, (DateTimeOffset.UtcNow.ToUnixTimeSeconds(), mobile));
                // 设置过期时间
                await RedisHelper.ExpireAsync(key, TimeSpan.FromMinutes(cache));

                return;
            }
        }

        throw Oops.Oh("短信发送失败！");
    }

    
    #region Post

    /// <summary>
    /// 会员注册
    /// </summary>
    /// <returns></returns>
    [HttpPost("register")]
    public async Task Register([FromBody] RegisterCrInput input)
    {
        // 判断验证码是否正确
        string cacheKey = string.Format("pcweb:VerificationCode:{0}", input.mobile);
        var code = await _cacheManager.GetAsync<string>(cacheKey);
        if (input.code != code)
        {
            throw Oops.Oh("验证码不正确");
        }
        // 删掉缓存的验证码
        await _cacheManager.DelAsync(cacheKey);

        // 判断手机号码是否注册
        if (await _repository.Context.Queryable<LogMemberEntity>().Where(it => it.PhoneNumber == input.mobile).AnyAsync())
        {
            throw Oops.Oh(ErrorCode.D1003);
        }


        LogMemberEntity logMemberEntity = new LogMemberEntity
        {
            Id = SnowflakeIdHelper.NextId(),
            Name = input.mobile,
            PhoneNumber = input.mobile,
            Password = MD5Encryption.Encrypt(input.password),
        };
        await _repository.Context.Insertable<LogMemberEntity>(logMemberEntity).IgnoreColumnsNull().ExecuteCommandAsync();
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<dynamic> Login([FromBody] UserLoginInput input)
    {
        var payload = new Dictionary<string, object>();
        switch (input.userType)
        {
            case "1":
                {
                    var user = await _repository.Context.Queryable<LogMemberEntity>().Where(it => it.PhoneNumber == input.userName && it.DeleteMark == null).FirstAsync() ?? throw Oops.Oh(ErrorCode.D5002); ;
                    if (user.Password != input.passWord)
                    {
                        throw Oops.Oh(ErrorCode.D1000);
                    }
                    // 生成Token令牌
                    payload.Add(ClaimConst.CLAINMUSERID, user.Id);
                    payload.Add(ClaimConst.CLAINMACCOUNT, user.PhoneNumber);
                    payload.Add(ClaimConst.CLAINMREALNAME, user.Name);
                    payload.Add("Role", LoginUserRoleType.Member);
                }
                break;
            case "2":
                break;
            case "3":
                break;
            default:
                throw Oops.Oh($"未知的用户类型[{input.userType}]");
        }

        if (payload.IsAny())
        {
            return new
            {
                token = JWTEncryption.Encrypt(payload)
            };
        }


        throw Oops.Oh("登录失败！");
    } 
    #endregion
}
