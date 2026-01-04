using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Emum;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Base;
using QT.CMS.Entitys.Dto.Config;
using QT.Common.Const;
using QT.Common.Core.Filter;
using QT.Common.Extension;
using QT.FriendlyException;
using QT.JsonSerialization;
using QT.Systems.Entitys.System;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 系统参数配置
/// </summary>
[Route("api/cms/admin/setting")]
[ApiController]
[ApiDescriptionSettings(ModuleConst.CMS)]
public class SettingController : ControllerBase
{
    private readonly ISqlSugarRepository<SysConfig> _sysConfigService;
    /// <summary>
    /// 构造函数依赖注入
    /// </summary>
    public SettingController(ISqlSugarRepository<SysConfig> sysConfigService)
    {
        _sysConfigService = sysConfigService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 获取系统配置
    /// 示例：/admin/setting/sysconfig
    /// </summary>
    [HttpGet("{type}")]
    [Authorize]
    //[AuthorizeFilter("Setting", ActionType.View, "type")]
    public async Task<IActionResult> Get([FromRoute] ConfigType type)
    {
        var model = await _sysConfigService.SingleAsync(x => x.Type != null && x.Type.ToLower() == type.ToString());
        if (model == null)
        {
            //如果找不到，则返回统一错误消息
            throw Oops.Oh($"系统参数{type}无法找到。");
        }
        if (model.Type == ConfigType.SysConfig.ToString())
        {
            return Ok(JSON.Deserialize<SysConfigDto>(model.JsonData));
        }
        else if (model.Type == ConfigType.MemberConfig.ToString())
        {
            return Ok(JSON.Deserialize<MemberConfigDto>(model.JsonData));
        }
        else if (model.Type == ConfigType.OrderConfig.ToString())
        {
            return Ok(JSON.Deserialize<OrderConfigDto>(model.JsonData));
        }
        throw Oops.Oh($"找不到{type}配置返回类型");
    }

    /// <summary>
    /// 修改系统配置
    /// 示例：/admin/setting/sysconfig
    /// </summary>
    [HttpPut("sysconfig")]
    [Authorize]
    //[AuthorizeFilter("Setting", ActionType.Edit, "SysConfig")]
    public async Task<IActionResult> Update([FromBody] SysConfigDto modelDto)
    {
        var model = await _sysConfigService.SingleAsync(x => x.Type == ConfigType.SysConfig.ToString());
        if (model == null)
        {
            //如果找不到，则返回统一错误消息
            throw Oops.Oh($"系统配置不存在或已删除。");
        }
        model.JsonData = JSON.Serialize(modelDto);
        var result = await _sysConfigService.UpdateAsync(model) > 0;
        if (!result)
        {
            //抛出自定义异常错误
            throw Oops.Oh("保存过程中发生错误。");
        }
        return NoContent();
    }

    /// <summary>
    /// 修改会员配置
    /// 示例：/admin/setting/memberconfig
    /// </summary>
    [HttpPut("memberconfig")]
    [Authorize]
    //[AuthorizeFilter("Setting", ActionType.Edit, "MemberConfig")]
    public async Task<IActionResult> Update([FromBody] MemberConfigDto modelDto)
    {
        var model = await _sysConfigService.SingleAsync(x=>x.Type ==ConfigType.MemberConfig.ToString());
        if (model == null)
        {
            //如果找不到，则返回统一错误消息
            throw Oops.Oh($"会员配置不存在或已删除。");
        }
        model.JsonData = JSON.Serialize(modelDto);
        var result = await _sysConfigService.UpdateAsync(model) > 0;
        if (!result)
        {
            //抛出自定义异常错误
            throw Oops.Oh("保存过程中发生错误。");
        }
        return NoContent();
    }

    /// <summary>
    /// 修改订单配置
    /// 示例：/admin/setting/orderconfig
    /// </summary>
    [HttpPut("orderconfig")]
    [Authorize]
    //[AuthorizeFilter("Setting", ActionType.Edit, "OrderConfig")]
    public async Task<IActionResult> Update([FromBody] OrderConfigDto modelDto)
    {
        var model = await _sysConfigService.SingleAsync(x => x.Type == ConfigType.OrderConfig.ToString());
        if (model == null)
        {
            //如果找不到，则返回统一错误消息
            throw Oops.Oh($"订单配置不存在或已删除。");
        }
        model.JsonData = JSON.Serialize(modelDto);
        var result = await _sysConfigService.UpdateAsync(model) > 0;
        if (!result)
        {
            //抛出自定义异常错误
            throw Oops.Oh("保存过程中发生错误。");
        }
        return NoContent();
    }

    /// <summary>
    /// 获取短信账户数量
    /// send:已发送数量
    /// quantity:余额
    /// 示例：/admin/setting/sms/account/quantity
    /// </summary>
    [HttpGet("sms/account/{type}")]
    [Authorize]
    //[AuthorizeFilter("Setting", ActionType.Edit, "SysConfig")]
    public async Task<IActionResult> GetSmsQuantity([FromRoute] string type)
    {
        if (type.IsNullOrEmpty())
        {
            throw Oops.Oh("查询参数有误");
        }
        var result = new SmsResultDto();
        throw new NotImplementedException();
        //switch (type)
        //{
        //    case "quantity":
        //        result = await _smsService.GetAccountQuantity();
        //        break;
        //    default:
        //        result = await _smsService.GetSendQuantity();
        //        break;
        //}
        //if (result.Code == null || !result.Code.Equals("100"))
        //{
        //    throw new ResponseException("查询短信账户失败");
        //}
        //return Ok(result);
    }

    /// <summary>
    /// 获取官方动态
    /// 示例：/admin/setting/official/notice
    /// </summary>
    [HttpGet("official/notice")]
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetNotice([FromRoute] ConfigType type)
    {
        throw new NotImplementedException();
        //var result = await RequestHelper.GetAsync("http://www.95033.cn/notice.ashx");
        //return Ok(result);
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取系统基本配置
    /// 示例：/client/setting/sysconfig
    /// </summary>
    [HttpGet("/client/setting/{type}")]
    [CachingFilter]
    [AllowAnonymous,NonUnify]
    public async Task<IActionResult> ClientGetSysConfig([FromRoute] ConfigType type)
    {
        var configType = type;
        if (type == ConfigType.UploadConfig)
        {
            configType = ConfigType.SysConfig;
        }
        var model = await _sysConfigService.SingleAsync(x=>x.Type == configType.ToString());
        if (model == null)
        {
            throw Oops.Oh($"找不到{type}配置返回类型");
        }
        switch (type)
        {
            case ConfigType.SysConfig:
                return Ok(JSON.Deserialize<SysConfigClientDto>(model.JsonData));
            case ConfigType.UploadConfig:
                var sysConfig = JSON.Deserialize<SysConfigDto>(model.JsonData);
                var uploadConfig = sysConfig.Adapt<UploadConfigDto>();
                return Ok(uploadConfig);
            default:
                throw Oops.Oh($"找不到{type}配置返回类型");
        }
    }

    /// <summary>
    /// 获取系统基本配置
    /// 示例：/client/setting/sysconfig
    /// </summary>
    [HttpGet("/client/Consultation/Question")]
    [CachingFilter]
    [AllowAnonymous, NonUnify]
    public async Task<IActionResult> ClientGetConsultationQuestion()
    {
        var list = await _sysConfigService.Context.Queryable<DictionaryDataEntity>()
            .Where(x => SqlFunc.Subqueryable<DictionaryTypeEntity>().Where(ddd => ddd.Id == x.DictionaryTypeId && ddd.EnCode == "ConsultationQuestion").Any())
            .OrderBy(x => x.SortCode)
            .Select(x=>x.FullName)
            .ToListAsync();

        return Ok(list ?? new List<string>());
    }
    #endregion
}
