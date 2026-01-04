using System.Web;

namespace QT.CMS;

/// <summary>
/// 支付宝支付下单
/// </summary>
[Route("api/cms/alipay")]
[ApiController]
public class AlipayController : ControllerBase
{
    private readonly IAlipayExecuteService _alipayExecuteService;
    private readonly ISqlSugarRepository<PaymentCollection> _paymentCollectionService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public AlipayController(IAlipayExecuteService alipayExecuteService, ISqlSugarRepository<PaymentCollection> paymentCollectionService)
    {
        _alipayExecuteService = alipayExecuteService;
        _paymentCollectionService = paymentCollectionService;
    }

    /// <summary>
    /// 支付宝统一下单
    /// 示例：/alipay
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Pay(AlipayTradeDto modelDto)
    {
        if (modelDto.OutTradeNo == null)
        {
            throw Oops.Oh("无法获取订单号有误，请重试");
        }
        //查询订单状态
        var model = await _paymentCollectionService.Where(x => x.TradeNo == modelDto.OutTradeNo).FirstAsync();
        if (model == null)
        {
            throw Oops.Oh("订单号有误，请重试");
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

        //防止用户URL编码，URL解码
        modelDto.ReturnUri = HttpUtility.UrlDecode(modelDto.ReturnUri);
        //判断支付接口类型
        var pay = await _paymentCollectionService.Context.Queryable<SitePayment>().FirstAsync(x => x.Id == modelDto.PaymentId);
        if (pay == null)
        {
            throw Oops.Oh("支付方式有误，请重试");
        }
        if (pay.Type == "app")
        {
            return Ok(await _alipayExecuteService.AppPayAsync(modelDto));
        }
        else if (pay.Type == "pc")
        {
            return Ok(await _alipayExecuteService.PcPayAsync(modelDto));
        }
        else if (pay.Type == "h5")
        {
            return Ok(await _alipayExecuteService.H5PayAsync(modelDto));
        }
        throw Oops.Oh("支付方式有误，请重试");
    }
}
