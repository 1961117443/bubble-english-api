namespace QT.CMS;

/// <summary>
/// 微信支付下单
/// </summary>
[Route("api/cms/wechatpay")]
[ApiController]
public class WeChatPayController : ControllerBase
{
    private readonly IWeChatExecuteService _weChatExecuteService;
    private readonly ISqlSugarRepository<PaymentCollection> _paymentCollectionService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public WeChatPayController(IWeChatExecuteService weChatExecuteService, ISqlSugarRepository<PaymentCollection> paymentCollectionService)
    {
        _weChatExecuteService = weChatExecuteService;
        _paymentCollectionService = paymentCollectionService;
    }

    /// <summary>
    /// 微信统一下单
    /// 示例：/wechatpay
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Pay(WeChatPayDto modelDto)
    {
        if (modelDto.OutTradeNo == null)
        {
            throw Oops.Oh("无法获取订单号有误，请重试");
        }
        //查询订单状态
        var model = await _paymentCollectionService.Context.Queryable<PaymentCollection>().FirstAsync(x => x.TradeNo == modelDto.OutTradeNo);
        if (model == null)
        {
            throw Oops.Oh("订单交易号有误，请重试");
        }
        if (model.Status == 2)
        {
            throw Oops.Oh("订单已支付，请勿重复付款");
        }
        if (model.Status == 3)
        {
            throw Oops.Oh("订单已取消，无法再次支付");
        }
        if (model.PaymentAmount <= 0)
        {
            throw Oops.Oh("支付金额必须大于零");
        }
        //赋值支付方式及总金额
        modelDto.PaymentId = model.PaymentId;
        modelDto.Total = model.PaymentAmount;

        //判断支付接口类型
        var pay = await _paymentCollectionService.Context.Queryable<SitePayment>().FirstAsync(x => x.Id == modelDto.PaymentId);
        if (pay == null)
        {
            throw Oops.Oh("支付方式有误，请重试");
        }
        if (pay.Type == "jsapi")
        {
            return Ok(await _weChatExecuteService.JsApiPayAsync(modelDto, false));
        }
        else if (pay.Type == "mp")
        {
            return Ok(await _weChatExecuteService.JsApiPayAsync(modelDto));
        }
        else if (pay.Type == "app")
        {
            return Ok(await _weChatExecuteService.AppPayAsync(modelDto));
        }
        else if (pay.Type == "h5")
        {
            return Ok(await _weChatExecuteService.H5PayAsync(modelDto));
        }
        else if (pay.Type == "native")
        {
            return Ok(await _weChatExecuteService.NativePayAsync(modelDto));
        }
        throw Oops.Oh("支付方式有误，请重试");
    }

    /// <summary>
    /// 返回授权拼接好地址
    /// 示例：/wechatpay/oauth
    /// </summary>
    [HttpPost("oauth")]
    public async Task<IActionResult> OAuth(WeChatPayOAuthDto modelDto)
    {
        if (modelDto.OutTradeNo == null)
        {
            throw Oops.Oh("无法获取订单号有误，请重试");
        }
        //查询订单状态
        var model = await _paymentCollectionService.Context.Queryable<PaymentCollection>().FirstAsync(x => x.TradeNo == modelDto.OutTradeNo);
        if (model == null)
        {
            throw Oops.Oh("订单交易号有误，请重试");
        }
        modelDto.PaymentId = model.PaymentId;

        //调用授权返回URL
        var result = await _weChatExecuteService.OAuthAsync(modelDto);
        return Ok(new { url = result });
    }
}