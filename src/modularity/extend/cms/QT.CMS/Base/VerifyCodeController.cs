using QT.Common.Const;
using QT.Common.Core.Captcha.General;
using QT.Logging.Attributes;

namespace QT.CMS;

/// <summary>
/// 验证码接口
/// </summary>
[Route("api/cms/[controller]")]
[ApiDescriptionSettings(ModuleConst.CMS, Tag = "CMS", Name = "VerifyCode", Order = 200)]
[ApiController]
public class VerifyCodeController : ControllerBase
{
    //private readonly IHttpContextAccessor _httpContextAccessor;
    //private readonly ISmsService _smsService;
    //private readonly ISysConfigService _sysConfigService;
    //private readonly INotifyTemplateService _notifyTemplateService;
    //private readonly IUserService _userService;

    ///// <summary>
    ///// 依赖注入
    ///// </summary>
    //public VerifyCodeController(IHttpContextAccessor httpContextAccessor, ISmsService smsService,
    //    ISysConfigService sysConfigService, INotifyTemplateService notifyTemplateService, IUserService userService)
    //{
    //    _httpContextAccessor = httpContextAccessor;
    //    _smsService = smsService;
    //    _sysConfigService = sysConfigService;
    //    _notifyTemplateService = notifyTemplateService;
    //    _userService = userService;
    //}

    private readonly IGeneralCaptcha _generalCaptcha;

    public VerifyCodeController(IGeneralCaptcha generalCaptcha)
    {
        _generalCaptcha = generalCaptcha;
    }

    #region 前台调用接口============================

    /// <summary>
    /// 获取图形验证码
    /// 示例：/verifycode
    /// </summary>
    [HttpGet("")]
    [AllowAnonymous]
    [IgnoreLog]
    [NonUnify]
    public async Task<dynamic> GetCode()
    {
        var key = Guid.NewGuid().ToString();
        var stream = await _generalCaptcha.CreateCaptchaImage(key, 120, 30, 4);
        var value = Convert.ToBase64String(stream.ToArray());
        //return new FileContentResult(stream, "image/jpeg");

        return new
        {
            key = key,
            data = $"data:image/png;base64,{value}"
        };

        //string code = VerifyCodeHelper.RandomCode(4);
        //using (var stream = VerifyCodeHelper.Create(code, 80, 30))
        //{
        //    var key = Guid.NewGuid().ToString();
        //    var value = Convert.ToBase64String(stream.ToArray());
        //    MemoryCacheHelper.Set(key, code, TimeSpan.FromSeconds(120));
        //    return Ok(new
        //    {
        //        Key = key,
        //        Data = $"data:image/png;base64,{value}"
        //    });
        //}
    }

    ///// <summary>
    ///// 发送手机验证码
    ///// 示例：/verifycode/mobile/13521062888
    ///// </summary>
    //[HttpGet("mobile/{mobile}")]
    //public async Task<IActionResult> GetMobileCode([FromRoute] string mobile, [FromQuery] byte check = 1)
    //{
    //    //检查手机号码
    //    Regex r = new Regex(@"^1\d{10}$", RegexOptions.IgnoreCase); //正则表达式实例，不区分大小写
    //    Match m = r.Match(mobile); //搜索匹配项
    //    if (m == null)
    //    {
    //        throw new ResponseException("手机号码有误");
    //    }
    //    //获取客户端IP
    //    var client = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    //    if (!client.IsNotNullOrEmpty())
    //    {
    //        throw new ResponseException("无法获取客户端IP");
    //    }
    //    //检查是否过于频繁请求
    //    var cacheKey = MD5Helper.MD5Encrypt32($"code:{client}");
    //    var isCache = MemoryCacheHelper.Get(cacheKey);
    //    if (isCache != null)
    //    {
    //        throw new ResponseException("请求频繁，请稍候重试");
    //    }
    //    //检查账户是否存在
    //    if (check > 0 && !await _userService.ExistsAsync<ApplicationUser>(x => x.PhoneNumber == mobile))
    //    {
    //        throw new ResponseException("手机账户不存在，请重试");
    //    }

    //    //取出短信模板
    //    var template = await _notifyTemplateService.QueryAsync<NotifyTemplate>(x => x.Type == 2 && x.CallIndex != null && x.CallIndex.ToLower().Equals("usercode"));
    //    if (template == null || template.Content == null)
    //    {
    //        throw new ResponseException("短信模板有误，请联系管理员");
    //    }
    //    //取出站点和会员设置
    //    var jsonData1 = await _sysConfigService.QueryByTypeAsync(ConfigType.SysConfig);
    //    if (jsonData1 == null)
    //    {
    //        throw new ResponseException("系统配置有误，请联系管理员");
    //    }
    //    var sysConfig = JsonHelper.ToJson<SysConfigDto>(jsonData1.JsonData);
    //    if (sysConfig == null)
    //    {
    //        throw new ResponseException("系统配置格式有误，请联系管理员");
    //    }
    //    var jsonData2 = await _sysConfigService.QueryByTypeAsync(ConfigType.MemberConfig);
    //    if (jsonData2 == null)
    //    {
    //        throw new ResponseException("会员设置有误，请联系管理员");
    //    }
    //    var memberConfig = JsonHelper.ToJson<MemberConfigDto>(jsonData2.JsonData);
    //    if (memberConfig == null)
    //    {
    //        throw new ResponseException("会员设置格式有误，请联系管理员");
    //    }
    //    //生成验证码
    //    var code = UtilConvert.Number(memberConfig.RegCodeLength);
    //    //替换模板变量
    //    var smsContent = template.Content;
    //    smsContent = smsContent.Replace("{webname}", sysConfig.WebName);
    //    smsContent = smsContent.Replace("{weburl}", sysConfig.WebUrl);
    //    smsContent = smsContent.Replace("{webtel}", sysConfig.WebTel);
    //    smsContent = smsContent.Replace("{code}", code);
    //    smsContent = smsContent.Replace("{valid}", memberConfig.RegSmsExpired.ToString());

    //    //开始发送短信
    //    var result = await _smsService.Send(new SmsMessageDto
    //    {
    //        Pass = 1,
    //        Mobiles = mobile,
    //        Content = smsContent
    //    });
    //    if (result.Code != "100")
    //    {
    //        throw new ResponseException(result.Message);
    //    }
    //    //写入缓存
    //    var key = Guid.NewGuid().ToString(); //获取缓存Key
    //    var secret = MD5Helper.MD5Encrypt32(mobile + code); //将key和手机号码加密放入缓存中，保证一致性
    //    MemoryCacheHelper.Set(cacheKey, mobile, TimeSpan.FromMinutes(memberConfig.RegCodeCtrl)); //设置请求间隔限制
    //    MemoryCacheHelper.Set(key, secret, TimeSpan.FromMinutes(memberConfig.RegSmsExpired));
    //    return Ok(new
    //    {
    //        CodeKey = key
    //    });
    //}

    ///// <summary>
    ///// 发送邮箱验证码
    ///// 示例：/verifycode/email/test@qq.com
    ///// </summary>
    //[HttpGet("email/{email}")]
    //public async Task<IActionResult> GetEmailCode([FromRoute] string email, [FromQuery] byte check = 1)
    //{
    //    //检查邮箱账户
    //    Regex r = new Regex(@"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,})$",
    //        RegexOptions.IgnoreCase); //正则表达式实例，不区分大小写
    //    Match m = r.Match(email); //搜索匹配项
    //    if (m == null)
    //    {
    //        throw new ResponseException("邮箱格式有误");
    //    }
    //    //检查账户是否存在
    //    if (check > 0 && !await _userService.ExistsAsync<ApplicationUser>(x => x.Email.ToLower() == email.ToLower()))
    //    {
    //        throw new ResponseException("邮箱账户不存在，请重试");
    //    }
    //    //取出邮箱模板
    //    var template = await _notifyTemplateService.QueryAsync<NotifyTemplate>(x => x.Type == 1 && x.CallIndex != null && x.CallIndex.ToLower().Equals("usercode"));
    //    if (template == null || template.Content == null)
    //    {
    //        throw new ResponseException("邮箱模板有误，请联系管理员");
    //    }
    //    //取出站点和会员设置
    //    var jsonData1 = await _sysConfigService.QueryByTypeAsync(ConfigType.SysConfig);
    //    if (jsonData1 == null)
    //    {
    //        throw new ResponseException("系统配置有误，请联系管理员");
    //    }
    //    var sysConfig = JsonHelper.ToJson<SysConfigDto>(jsonData1.JsonData);
    //    if (sysConfig == null || !sysConfig.EmailSmtp.IsNotNullOrEmpty() || !sysConfig.EmailFrom.IsNotNullOrEmpty() || !sysConfig.EmailPassword.IsNotNullOrEmpty())
    //    {
    //        throw new ResponseException("邮箱账户未设置，请联系管理员");
    //    }
    //    var jsonData2 = await _sysConfigService.QueryByTypeAsync(ConfigType.MemberConfig);
    //    if (jsonData2 == null)
    //    {
    //        throw new ResponseException("会员设置有误，请联系管理员");
    //    }
    //    var memberConfig = JsonHelper.ToJson<MemberConfigDto>(jsonData2.JsonData);
    //    if (memberConfig == null)
    //    {
    //        throw new ResponseException("会员设置格式有误，请联系管理员");
    //    }
    //    //生成验证码
    //    var code = UtilConvert.GetCheckCode(memberConfig.RegCodeLength);
    //    //替换模板变量
    //    string content = template.Content;
    //    content = content.Replace("{webname}", sysConfig.WebName);
    //    content = content.Replace("{weburl}", sysConfig.WebUrl);
    //    content = content.Replace("{webtel}", sysConfig.WebTel);
    //    content = content.Replace("{code}", code);
    //    content = content.Replace("{valid}", memberConfig.RegSmsExpired.ToString());
    //    //发送邮件
    //    try
    //    {
    //        MailHelper.Send(sysConfig.EmailSmtp, sysConfig.EmailPort.GetValueOrDefault(), sysConfig.EmailSSL > 0,
    //        sysConfig.EmailUserName, sysConfig.EmailPassword, sysConfig.EmailNickname,
    //        sysConfig.EmailFrom, email, template.Title, content);
    //    }
    //    catch (Exception e)
    //    {
    //        throw new ResponseException($"邮件发送失败：{e.Message}");
    //    }
    //    //写入缓存
    //    var key = Guid.NewGuid().ToString(); //获取缓存Key
    //    var secret = MD5Helper.MD5Encrypt32(email + code); //将邮箱+验证码加密放入缓存中，保证一致性
    //    MemoryCacheHelper.Set(key, secret, TimeSpan.FromMinutes(memberConfig.RegSmsExpired));
    //    return Ok(new
    //    {
    //        CodeKey = key
    //    });
    //}

    ///// <summary>
    ///// 验证普通验证码
    ///// 示例：/verifycode
    ///// </summary>
    //[HttpPost]
    //public IActionResult ValidateCode([FromBody] VerifyCode verifyCode)
    //{
    //    var code = MemoryCacheHelper.Get(verifyCode.CodeKey);
    //    if (code == null)
    //    {
    //        return BadRequest(ResponseMessage.Error("验证码已过期，请重试"));
    //    }
    //    if (code.ToString()?.ToLower() != verifyCode.CodeValue?.ToLower())
    //    {
    //        return BadRequest(ResponseMessage.Error("验证码有误，请重试"));
    //    }
    //    return NoContent();
    //}
    #endregion
}