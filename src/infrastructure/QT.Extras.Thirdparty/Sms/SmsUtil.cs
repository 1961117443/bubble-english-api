using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using AlibabaCloud.OpenApiClient.Models;
using Newtonsoft.Json;
using NPOI.POIFS.Crypt.Dsig;
using QT.Common.Security;
using QT.DataEncryption;
using QT.RemoteRequest.Extensions;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Sms.V20210111;
using TencentCloud.Sms.V20210111.Models;
using WebSocketSharp.Server;

namespace QT.Extras.Thirdparty.Sms;

public class SmsUtil
{
    #region 阿里云

    /// <summary>
    /// 发送（阿里云短信）.
    /// </summary>
    /// <param name="smsModel"></param>
    /// <returns></returns>
    public static string SendSmsByAli(SmsParameterInfo smsModel)
    {
        try
        {
            var config = new Config
            {
                // 您的AccessKey ID
                AccessKeyId = smsModel.keyId,

                // 您的AccessKey Secret
                AccessKeySecret = smsModel.keySecret,
            };
            config.Endpoint = smsModel.domain;
            AlibabaCloud.SDK.Dysmsapi20170525.Client client = new AlibabaCloud.SDK.Dysmsapi20170525.Client(config);
            AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest sendSmsRequest = new AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest();
            sendSmsRequest.PhoneNumbers = smsModel.mobileAli;
            sendSmsRequest.SignName = smsModel.signName;
            sendSmsRequest.TemplateCode = smsModel.templateId;
            sendSmsRequest.TemplateParam = smsModel.templateParamAli;

            AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsResponse sendSmsResponse = client.SendSms(sendSmsRequest);
            if (sendSmsResponse.Body.Code.Equals("OK") && sendSmsResponse.Body.Message.Equals("OK"))
            {
                return sendSmsResponse.Body.RequestId;
            }
            else
            {
                throw new Exception(sendSmsResponse.Body.Message);
            }

        }
        catch (Exception ex)
        {
            return "短信发送失败";
        }
    }

    /// <summary>
    /// 获取模板配置字段.
    /// </summary>
    /// <param name="smsModel"></param>
    /// <returns></returns>
    public static List<string> GetTemplateByAli(SmsParameterInfo smsModel)
    {
        try
        {
            var config = new Config
            {
                // 您的AccessKey ID
                AccessKeyId = smsModel.keyId,

                // 您的AccessKey Secret
                AccessKeySecret = smsModel.keySecret,
            };
            config.Endpoint = smsModel.domain;
            AlibabaCloud.SDK.Dysmsapi20170525.Client client = new AlibabaCloud.SDK.Dysmsapi20170525.Client(config);
            AlibabaCloud.SDK.Dysmsapi20170525.Models.QuerySmsTemplateRequest querySmsTemplateRequest = new AlibabaCloud.SDK.Dysmsapi20170525.Models.QuerySmsTemplateRequest();
            querySmsTemplateRequest.TemplateCode = smsModel.templateId;
            AlibabaCloud.SDK.Dysmsapi20170525.Models.QuerySmsTemplateResponse querySmsTemplateResponse = client.QuerySmsTemplate(querySmsTemplateRequest);
            if (querySmsTemplateResponse.Body.Code.Equals("OK"))
            {
                return GetFields(querySmsTemplateResponse.Body.TemplateContent);
            }
            else
            {
                throw new Exception("获取模板失败");
            }
        }
        catch (Exception)
        {

            throw new Exception("获取模板失败");
        }
    }

    #endregion

    #region 腾讯云

    /// <summary>
    /// 腾讯云短信.
    /// </summary>
    /// <param name="smsModel"></param>
    /// <returns></returns>
    public static string SendSmsByTencent(SmsParameterInfo smsModel)
    {
        try
        {
            Credential cred = new Credential
            {
                SecretId = smsModel.keyId,
                SecretKey = smsModel.keySecret
            };

            ClientProfile clientProfile = new ClientProfile();
            HttpProfile httpProfile = new HttpProfile();
            httpProfile.Endpoint = smsModel.domain;
            clientProfile.HttpProfile = httpProfile;

            SmsClient client = new SmsClient(cred, smsModel.region, clientProfile);
            SendSmsRequest req = new SendSmsRequest();
            req.PhoneNumberSet = smsModel.mobileTx;
            req.SmsSdkAppId = smsModel.appId;
            req.SignName = smsModel.signName;
            req.TemplateId = smsModel.templateId;
            req.TemplateParamSet = smsModel.templateParamTx;
            SendSmsResponse resp = client.SendSmsSync(req);
            if (!resp.SendStatusSet.FirstOrDefault().Code.Equals("Ok"))
            {
                return "短信发送失败";
            }
            return resp.RequestId;
        }
        catch (Exception ex)
        {
            return "短信发送失败";
        }
    }

    /// <summary>
    /// 获取模板配置字段.
    /// </summary>
    /// <param name="smsModel"></param>
    /// <returns></returns>
    public static List<string> GetTemplateByTencent(SmsParameterInfo smsModel)
    {
        try
        {
            Credential cred = new Credential
            {
                SecretId = smsModel.keyId,
                SecretKey = smsModel.keySecret
            };

            ClientProfile clientProfile = new ClientProfile();
            HttpProfile httpProfile = new HttpProfile();
            httpProfile.Endpoint = smsModel.domain;
            clientProfile.HttpProfile = httpProfile;

            SmsClient client = new SmsClient(cred, smsModel.region, clientProfile);
            DescribeSmsTemplateListRequest req = new DescribeSmsTemplateListRequest();
            req.International = 0;
            req.TemplateIdSet = new ulong?[] { ulong.Parse(smsModel.templateId) };
            DescribeSmsTemplateListResponse resp = client.DescribeSmsTemplateListSync(req);
            if (resp.DescribeTemplateStatusSet.Count() > 0 && !string.IsNullOrEmpty(resp.RequestId))
            {
                var data = resp.DescribeTemplateStatusSet.FirstOrDefault().ToObject<Dictionary<string, object>>();
                var templateContent = data["TemplateContent"].ToString();
                return GetFields(templateContent);
            }
            else
            {
                throw new Exception("获取模板失败");
            }

        }
        catch (Exception ex)
        {

            throw new Exception("获取模板失败");
        }
    }

    #endregion

    /// <summary>
    /// 截取{}中的字符串.
    /// </summary>
    /// <param name="templateContent"></param>
    /// <returns></returns>
    private static List<string> GetFields(string templateContent)
    {
        MatchCollection mc = Regex.Matches(templateContent, "(?i){.*?}");
        return mc.Cast<Match>().Select(m => m.Value.TrimStart('{').TrimEnd('}')).ToList();
    }

    #region 通过三网短信发送短信
    /// <summary>
    /// 通过三网短信发送
    /// </summary>
    /// <returns></returns>
    public static async Task<string> SendSmsByThird(SmsParameterInfo smsModel)
    {
        string SMS_SEND_URL = smsModel.domain + "/api/sms/air/send";
        //var result =  sendUrl(SMS_SEND_URL, SendBuildSendEntity(smsModel));
        var result = await SMS_SEND_URL.SetBody(SendBuildSendEntity(smsModel), "application/json")
            .PostAsStringAsync();
        return result;
    }

    private static SendSmsEntity SendBuildSendEntity(SmsParameterInfo smsModel)
    {
        SendSmsEntity ss = new SendSmsEntity();
        ss.appKey = smsModel.keyId;
        ss.timestamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000 + "";
        ss.mobile = smsModel.mobileAli;
        ss.content = smsModel.content;
        ss.sendTime = "";
        ss.spNumber = "";
        ss.reportUrl = "";
        ss.moUrl = "";
        ss.attach = "";
        ////appKey,timestamp,mobile,content,spNumber,sendTime,appSecret
        ss.sign = MD5Encryption.Encrypt(ss.appKey + ss.timestamp + ss.mobile + ss.content + ss.spNumber + ss.sendTime + smsModel.keySecret);
        //return JsonConvert.SerializeObject(ss);

        return ss;
    }
    #endregion
}

internal class SendSmsEntity
{
    public string appKey {get;set;}
    public string timestamp {get;set;}
    public string mobile {get;set;}
    public string content {get;set;}
    public string sendTime {get;set;}
    public string spNumber {get;set;}
    public string sign {get;set;}
    public string reportUrl {get;set;}
    public string moUrl {get;set;}
    public string attach {get;set;}
}