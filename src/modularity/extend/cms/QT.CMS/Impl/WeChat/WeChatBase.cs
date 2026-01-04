using QT.DependencyInjection;

namespace QT.CMS;

/// <summary>
/// 微信支付基类
/// </summary>
public class WeChatBase:IScoped
{
    private readonly ISqlSugarRepository<SitePayment> _sitePaymentService;
    public WeChatBase(ISqlSugarRepository<SitePayment> sitePaymentService)
    {
        _sitePaymentService = sitePaymentService;
    }

    /// <summary>
    /// 获取微信支付账户
    /// </summary>
    public async Task<WeChatPayAccountDto> GetAccountAsync(int id)
    {
        //取得微信支付账户
        var payModel = await _sitePaymentService.AsQueryable()
            .Includes(x=>x.Site)
            .Includes(x=>x.Payment).SingleAsync(x => x.Id == id);
        if (payModel == null)
        {
            throw Oops.Oh("支付方式有误，请联系客服");
        }
        var model = new WeChatPayAccountDto
        {
            SiteId = payModel.SiteId,
            AppId = payModel.Key1,
            AppSecret = payModel.Key2,
            MchId = payModel.Key3,
            Apiv3Key = payModel.Key4,
            CertPath = payModel.Key5,
            NotifyUrl = payModel.Payment?.NotifyUrl
        };
        if (string.IsNullOrEmpty(model.AppId)
            || string.IsNullOrEmpty(model.AppSecret)
            || string.IsNullOrEmpty(model.MchId)
            || string.IsNullOrEmpty(model.Apiv3Key)
            || string.IsNullOrEmpty(model.CertPath)
            || string.IsNullOrEmpty(model.NotifyUrl))
        {
            throw Oops.Oh("支付方式设置有误，请联系客服");
        }
        model.CertPath = FileHelper.GetRootPath(model.CertPath);
        if (!FileHelper.FileExists(model.CertPath))
        {
            throw Oops.Oh("证书文件不存在，请联系客服");
        }
        return model;
    }
}