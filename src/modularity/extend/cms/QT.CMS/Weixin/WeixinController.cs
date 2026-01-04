using Microsoft.AspNetCore.Mvc;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Weixin;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 微信验证
/// </summary>
[Route("[controller]")]
[ApiController]
public class WeixinController : ControllerBase
{
    private readonly ISqlSugarRepository<WxAccount> _accountService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public WeixinController(ISqlSugarRepository<WxAccount> accountService)
    {
        _accountService = accountService;
    }

    #region 前台调用接口============================
    /// <summary>
    /// 验证微信服务器消息
    /// 示例：/weixin/1
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] int id, [FromQuery] WxCheckResult param)
    {
        if (id <= 0)
        {
            return BadRequest("公众号账号不正确，请检查参数重试。");
        }
        var accountModel = await _accountService.SingleAsync(x => x.Id == id);
        if (accountModel == null)
        {
            return BadRequest("公众号不存在，请检查参数重试。");
        }
        if (CheckSignature.Check(param.Signature, param.Timestamp, param.Nonce, accountModel.Token))
        {
            return Ok(param.Echostr); //返回随机字符串则表示验证通过
        }
        else
        {
            return Ok($"failed:{param.Signature},{CheckSignature.GetSignature(param.Timestamp, param.Nonce, accountModel.Token)}。" +
                "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
        }
    }
    #endregion
}
