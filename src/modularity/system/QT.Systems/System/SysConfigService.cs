using QT.Common.Core.Security;
using QT.Common.Dtos;
using QT.Common.Enum;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Extras.Thirdparty.DingDing;
using QT.Extras.Thirdparty.Email;
using QT.Extras.Thirdparty.WeChat;
using QT.FriendlyException;
using QT.Systems.Entitys.Dto.SysConfig;
using QT.Systems.Entitys.Enum;
using QT.Systems.Entitys.Permission;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using QT.Common.Const;
using QT.Common.Extension;
using Microsoft.AspNetCore.Authorization;
using QT.Logging.Attributes;
using Microsoft.Extensions.Caching.Memory;
using QT.Common.Cache;
using Microsoft.Extensions.Options;
using QT.Common.Options;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Core.Service;
using QT.Common;

namespace QT.Systems.System;

/// <summary>
/// 系统配置



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "System", Name = "SysConfig", Order = 211)]
[Route("api/system/[controller]")]
public class SysConfigService : ISysConfigService, IDynamicApiController, IScoped, ICoreSysConfigService
{
    /// <summary>
    /// 系统配置仓储.
    /// </summary>
    private readonly ISqlSugarRepository<SysConfigEntity> _sysConfigRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ITenantManager _tenantManager;
    private readonly TenantOptions _tenantOptions;

    private SysConfigOutput _sysConfigOutput;
    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant db;

    /// <summary>
    /// 初始化一个<see cref="SysConfigService"/>类型的新实例.
    /// </summary>
    public SysConfigService(
        ISqlSugarRepository<SysConfigEntity> sysConfigRepository,
        ISqlSugarClient context,
        IMemoryCache memoryCache,
        IOptions<TenantOptions> tenantOptions,
        ITenantManager tenantManager)
    {
        _sysConfigRepository = sysConfigRepository;
        _memoryCache = memoryCache;
        _tenantManager = tenantManager;
        _tenantOptions = tenantOptions.Value;
        db = context.AsTenant();
    }

    #region GET

    /// <summary>
    /// 获取系统配置.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<SysConfigOutput> GetInfo()
    {
        var array = new Dictionary<string, string>();
        var data = await _sysConfigRepository.Where(x => x.Category.Equals("SysConfig")).ToListAsync();
        foreach (var item in data)
        {
            array.Add(item.Key, item.Value);
        }

        if (!array.TryGetValue(nameof(SysConfigOutput.loginMode),out string mode) || mode.IsNullOrEmpty())
        {
            array[nameof(SysConfigOutput.loginMode)] = "LoginMode1";
        }

        _sysConfigOutput = array.ToObject<SysConfigOutput>();

        return _sysConfigOutput;
    }


    /// <summary>
    /// 获取系统配置信息
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [NonAction]
    public async Task<SysConfigOutput> GetSysConfig()
    {
        if (_sysConfigOutput == null)
        {
            return await GetInfo();
        }
        else
        {
            return _sysConfigOutput;
        }
    }

    /// <summary>
    /// 获取所有超级管理员.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getAdminList")]
    public async Task<dynamic> GetAdminList()
    {
        return await _sysConfigRepository.Context.Queryable<UserEntity>()
            .Where(x => x.IsAdministrator == 1 && x.DeleteMark == null)
            .Select(x => new AdminUserOutput()
            {
                id = x.Id,
                account = x.Account,
                realName = x.RealName
            }).ToListAsync();
    }

    /// <summary>
    /// 获取登录配置.
    /// </summary>
    /// <returns></returns>
    [HttpGet("login"),AllowAnonymous, IgnoreLog]
    public async Task<dynamic> GetLoginConfig()
    {
        string tenantId = string.Empty;
        if (_tenantOptions.MultiTenancy)
        {
            if (_tenantManager.IsLoggedIn)
            {
                tenantId = _tenantManager.TenantId;
            }
        }
        var cacheKey = $"GetLoginConfig:{tenantId}";
        return await _memoryCache.GetOrCreateAsync(cacheKey, async (entry) =>
        {
            var array = new Dictionary<string, object>();
            if (_tenantOptions.MultiTenancy)
            {
                if (_tenantManager.IsLoggedIn)
                {
                    _sysConfigRepository.Context.ChangeDatabase(tenantId);
                }
                else
                {
                    array.Add("multiTenancy", true);
                    return array;
                }
            }
            //var keys = new string[] { nameof(SysConfigOutput.loginMode), nameof(SysConfigOutput.sysName), "enableverificationcode", "verificationcodenumber" };
           
            try
            {
                //var data = await _sysConfigRepository
                //   .Where(x => x.Category.Equals("SysConfig"))
                //   .Where(x => keys.Contains(x.Key))
                //   .ToListAsync();
                //    foreach (var item in data)
                //    {
                //        array.Add(item.Key, item.Value);
                //    }

                // 系统配置信息
                var sysInfo = await GetInfo();
                array.Add(nameof(SysConfigOutput.loginMode), sysInfo.loginMode);
                array.Add(nameof(SysConfigOutput.sysName), sysInfo.sysName);
                array.Add(nameof(SysConfigOutput.enableVerificationCode), sysInfo.enableVerificationCode);
                array.Add(nameof(SysConfigOutput.verificationCodeNumber), sysInfo.verificationCodeNumber > 0 ? sysInfo.verificationCodeNumber : 4);

                var t = App.EffectiveTypes.FirstOrDefault(x => x.Name == "SysConfigInfo");
                if (t != null)
                {
                    array["sysInfo"] = sysInfo.Adapt(typeof(SysConfigOutput), t);
                }
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            }
            catch
            {
                // 1秒钟后过期
                entry.AbsoluteExpiration = DateTime.Now.AddSeconds(1);
            }

            if (!array.TryGetValue(nameof(SysConfigOutput.loginMode), out object mode) || mode.IsNullOrEmpty())
            {
                array[nameof(SysConfigOutput.loginMode)] = "LoginMode1";
            }
            return array;
        });       
    }

    #endregion

    #region Post

    /// <summary>
    /// 邮箱链接测试.
    /// </summary>
    /// <param name="input"></param>
    [HttpPost("Email/Test")]
    public void EmailTest([FromBody] MailParameterInfo input)
    {
        var result = MailUtil.CheckConnected(input);
        if (!result)
            throw Oops.Oh(ErrorCode.D9003);
    }

    /// <summary>
    /// 钉钉链接测试.
    /// </summary>
    /// <param name="input"></param>
    [HttpPost("testDingTalkConnect")]
    public void testDingTalkConnect([FromBody] DingParameterInfo input)
    {
        var dingUtil = new DingUtil(input.dingSynAppKey, input.dingSynAppSecret);
        if (string.IsNullOrEmpty(dingUtil.token))
            throw Oops.Oh(ErrorCode.D9003);
    }

    /// <summary>
    /// 企业微信链接测试.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="input"></param>
    [HttpPost("{type}/testQyWebChatConnect")]
    public void testQyWebChatConnect(int type, [FromBody] WeChatParameterInfo input)
    {
        var weChatUtil = new WeChatUtil(input.qyhCorpId, input.qyhCorpSecret);
        if (string.IsNullOrEmpty(weChatUtil.accessToken))
            throw Oops.Oh(ErrorCode.D9003);
    }

    /// <summary>
    /// 更新系统配置.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut]
    public async Task Update([FromBody] SysConfigUpInput input)
    {
        var configDic = input.ToObject<Dictionary<string, object>>();
        var entitys = new List<SysConfigEntity>();
        foreach (var item in configDic.Keys)
        {
            if (configDic[item] != null)
            {
                if (item == "tokentimeout")
                {
                    long time = 0;
                    if (long.TryParse(configDic[item].ToString(), out time))
                    {
                        if (time > 8000000)
                        {
                            throw Oops.Oh(ErrorCode.D9008);
                        }
                    }
                }

                if (item == "verificationCodeNumber")
                {
                    int codeNum = 3;
                    if (int.TryParse(configDic[item].ToString(), out codeNum))
                    {
                        if (codeNum > 6 || codeNum < 3) throw Oops.Oh(ErrorCode.D9009);
                    }
                }

                SysConfigEntity sysConfigEntity = new SysConfigEntity();
                sysConfigEntity.Id = SnowflakeIdHelper.NextId();
                sysConfigEntity.Key = item;
                sysConfigEntity.Value = configDic[item].ToString();
                sysConfigEntity.Category = "SysConfig";
                entitys.Add(sysConfigEntity);
            }
        }

        await Save(entitys, "SysConfig");

        //清空登录缓存
        string tenantId = string.Empty;
        if (_tenantOptions.MultiTenancy)
        {
            if (_tenantManager.IsLoggedIn)
            {
                tenantId = _tenantManager.TenantId;
            }
        }
        _memoryCache.Remove($"GetLoginConfig:{tenantId}");
    }

    /// <summary>
    /// 更新赋予超级管理员.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPut("setAdminList")]
    public async Task SetAdminList([FromBody] SetAdminInput input)
    {
        await _sysConfigRepository.Context.Updateable<UserEntity>().SetColumns(x => x.IsAdministrator == 0).Where(x => x.IsAdministrator == 1 && !x.Id.Equals(CommonConst.SUPPER_ADMIN_ACCOUNT)).ExecuteCommandAsync();
        await _sysConfigRepository.Context.Updateable<UserEntity>().SetColumns(x => x.IsAdministrator == 1).Where(x => input.adminIds.Contains(x.Id)).ExecuteCommandAsync();
    }

    #endregion

    #region PublicMethod

    /// <summary>
    /// 系统配置信息.
    /// </summary>
    /// <param name="category">分类.</param>
    /// <param name="key">键.</param>
    /// <returns></returns>
    [NonAction]
    public async Task<SysConfigEntity> GetInfo(string category, string key)
    {
        return await _sysConfigRepository.FirstOrDefaultAsync(s => s.Category == category && s.Key == key);
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 保存.
    /// </summary>
    /// <param name="entitys"></param>
    /// <param name="category"></param>
    /// <returns></returns>
    private async Task Save(List<SysConfigEntity> entitys, string category)
    {
        try
        {
            db.BeginTran();

            await _sysConfigRepository.DeleteAsync(x => x.Category.Equals(category));
            await _sysConfigRepository.InsertAsync(entitys);

            db.CommitTran();
        }
        catch (Exception)
        {
            db.RollbackTran();
        }
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TConfig"></typeparam>
    /// <returns></returns>
    [NonAction]
    public async Task<TConfig> GetConfig<TConfig>() where TConfig : class, new()
    {
        var propertyList = EntityHelper<TConfig>.InstanceProperties.Select(p => p.Name).ToArray();
        var array = new Dictionary<string, string>();
        var data = await _sysConfigRepository.Where(x => propertyList.Contains(x.Key)).ToListAsync();
        foreach (var item in data)
        {
            array.Add(item.Key, item.Value);
        }
        return array.ToObject<TConfig>();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TConfig"></typeparam>
    /// <returns></returns>
    [NonAction]
    public async Task<TConfig> GetConfigWithCategory<TConfig>(string category) where TConfig : class, new()
    {
        var propertyList = EntityHelper<TConfig>.InstanceProperties.Select(p => p.Name).ToArray();
        var array = new Dictionary<string, string>();
        var data = await _sysConfigRepository.Where(x => x.Category == category && propertyList.Contains(x.Key)).ToListAsync();
        foreach (var item in data)
        {
            array.Add(item.Key, item.Value);
        }
        return array.ToObject<TConfig>();
    }
}