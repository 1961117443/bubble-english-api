using System;
using System.Collections.Generic;
using System.Text;

namespace QT.CMS.Entitys.Dto.Base;

/// <summary>
/// 手机短信内容实体
/// </summary>
public class SmsMessageDto
{
    /// <summary>
    /// 手机号，多个号码以,逗号分隔开
    /// </summary>
    public string? Mobiles { get; set; }

    /// <summary>
    /// 短信内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 短信通道
    /// 1验证码通道
    /// 2通知通道
    /// 3广告通道
    /// </summary>
    public int Pass { get; set; } = 0;
}

/// <summary>
/// 请求结果实体
/// </summary>
public class SmsResultDto
{
    /// <summary>
    /// 状态码
    /// </summary>
    public string? Code { get; set; } = "115";

    /// <summary>
    /// 消息结果
    /// </summary>
    public string? Message { get; set; }
}
