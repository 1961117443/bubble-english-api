namespace QT.CMS;

/// <summary>
/// 余额支付接口
/// </summary>
[Route("api/cms/balancepay")]
[ApiController]
public class BalancePayController : ControllerBase
{
    private readonly IPaymentCollectionService _paymentCollectionService;
    private readonly ISqlSugarRepository<SitePayment> _sitePaymentService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public BalancePayController( ISqlSugarRepository<SitePayment> sitePaymentService, IPaymentCollectionService paymentCollectionService)
    {
        _paymentCollectionService = paymentCollectionService;
        _sitePaymentService = sitePaymentService;
    }

    /// <summary>
    /// 余额支付统一下单
    /// 示例：/balancepay
    /// </summary>
    [HttpPost]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> Pay(BalancePayDto modelDto)
    {
        if (modelDto.OutTradeNo == null)
        {
            throw Oops.Oh("订单交易号不允许为空");
        }
        //查询订单状态
        var model = await _sitePaymentService.Context.Queryable<PaymentCollection>().Where(x => x.TradeNo == modelDto.OutTradeNo).FirstAsync();
        if (model == null)
        {
            throw Oops.Oh("订单交易号有误，请重试");
        }
        if (model.TradeType == 1)
        {
            throw Oops.Oh("余额支付不允许充值");
        }
        if (model.Status == 2)
        {
            throw Oops.Oh("订单已支付，请勿重复付款");
        }
        if (model.Status == 3)
        {
            throw Oops.Oh("订单已取消，无法再次付款");
        }
        if (model.PaymentAmount <= 0)
        {
            throw Oops.Oh("支付金额必须大于零");
        }

        //判断支付接口类型
        var pay = await _sitePaymentService.Context.Queryable<SitePayment>().FirstAsync(x => x.Id == model.PaymentId && x.Type == "balance");
        if (pay == null)
        {
            throw Oops.Oh("支付方式有误，请重试");
        }
        //修改订单状态
        var result = await _paymentCollectionService.ConfirmAsync(modelDto.OutTradeNo);
        if (!result)
        {
            throw Oops.Oh("确认余额支付失败，请重试");
        }
        return NoContent();
    }

}
