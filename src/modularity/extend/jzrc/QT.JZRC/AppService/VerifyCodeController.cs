using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QT.Common.Const;
using QT.Common.Core.Captcha.General;
using QT.DynamicApiController;
using QT.Logging.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace QT.JZRC;


/// <summary>
/// 业务实现：验证服务.
/// </summary>
[ApiDescriptionSettings(ModuleConst.JZRC, Tag = "JZRC", Name = "VerifyCode", Order = 200)]
[Route("api/JZRC/[controller]")]
public class VerifyCodeController :IDynamicApiController
{
    private readonly IGeneralCaptcha _generalCaptcha;

    public VerifyCodeController(IGeneralCaptcha generalCaptcha)
    {
        _generalCaptcha = generalCaptcha;
    }
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
}
