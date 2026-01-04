using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.CMS;

/// <summary>
/// 支付宝支付基类
/// </summary>
public class AlipayBase
{
    private readonly ISqlSugarRepository<SitePayment> _sitePaymentService;
    public AlipayBase(ISqlSugarRepository<SitePayment> sitePaymentService)
    {
        _sitePaymentService = sitePaymentService;
    }

    /// <summary>
    /// 获取支付宝账户信息
    /// </summary>
    public async Task<AlipayAccountDto> GetAccountAsync(int id)
    {
        //取得微信支付账户
        var payModel = await _sitePaymentService.AsQueryable()
            .Includes(x => x.Site)
            .Includes(x => x.Payment)
            .SingleAsync(x => x.Id == id);
        if (payModel == null)
        {
            throw Oops.Oh("支付方式有误，请联系客服");
        }
        var model = new AlipayAccountDto
        {
            SiteId = payModel.SiteId,
            AppId = payModel.Key1,
            AlipayPublicKey = payModel.Key2,
            AppPrivateKey = payModel.Key3,
            NotifyUrl = payModel.Payment?.NotifyUrl,
            NotifyType = payModel.Type
        };
        if (string.IsNullOrEmpty(model.AppId)
            || string.IsNullOrEmpty(model.AlipayPublicKey)
            || string.IsNullOrEmpty(model.AppPrivateKey)
            || string.IsNullOrEmpty(model.NotifyUrl)
            || string.IsNullOrEmpty(model.NotifyType))
        {
            throw Oops.Oh("支付宝设置有误，请联系客服");
        }
        return model;
    }
}