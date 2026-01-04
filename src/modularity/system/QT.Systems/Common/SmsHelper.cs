using Microsoft.Extensions.DependencyInjection;
using QT.Common.Configuration;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Extension;
using QT.Common.Security;
using QT.Extras.Thirdparty.Sms;
using QT.FriendlyException;
using QT.Systems.Entitys.System;
using QT.Systems.Interfaces.System;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Systems.Common;

public class SmsHelper
{
    /// <summary>
    /// 根据短信模板发送短信
    /// </summary>
    /// <param name="enCode"></param>
    /// <param name="phone"></param>
    /// <param name="param">短信参数，如果是阿里云的参数，param={ [参数名称]=参数值}</param>
    /// <returns></returns>
    public static async Task<string> SendByCode(string enCode, string phone, object param)
    {
        var msg = string.Empty;
        await TenantScoped.Create(TenantScoped.TenantId, async (factory, scope) =>
        {
            var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
            var entity = await db.Queryable<SmsTemplateEntity>().SingleAsync(x => x.EnCode == enCode) ?? throw Oops.Oh($"缺少短信模板[{enCode}]");

            var _sysConfigService = scope.ServiceProvider.GetRequiredService<ISysConfigService>();

            // 发送短信
            var sysconfig = await _sysConfigService.GetInfo();
            var smsModel = new SmsParameterInfo()
            {
                keyId = entity.Company == 1 ? sysconfig.aliAccessKey : sysconfig.tencentSecretId,
                keySecret = entity.Company == 1 ? sysconfig.aliSecret : sysconfig.tencentSecretKey,
                region = entity.Region,
                domain = entity.Endpoint,
                templateId = entity.TemplateId,
                signName = entity.SignContent
            };

            if (entity.Company == 1)
            {
                smsModel.mobileAli = phone;
                smsModel.templateParamAli = param?.ToJsonString();
                msg = SmsUtil.SendSmsByAli(smsModel);
            }
            else
            {
                smsModel.mobileTx = new string[] { phone };
                List<string> mList = new List<string>();
                //foreach (string data in input.parameters.Values)
                //{
                //    mList.Add(data);
                //}
                smsModel.appId = sysconfig.tencentAppId;
                smsModel.templateParamTx = mList.ToArray();
                msg = SmsUtil.SendSmsByTencent(smsModel);
            }

            if (msg.Equals("短信发送失败"))
                throw Oops.Oh(msg);

        });
        return msg;
    }
}
