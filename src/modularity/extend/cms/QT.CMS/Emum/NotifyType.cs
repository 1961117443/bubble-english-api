using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QT.CMS.Emum;

/// <summary>
/// 通知类型
/// </summary>
public enum NotifyType
{
    /// <summary>
    /// 邮件
    /// </summary>
    [Display(Name = "邮件")]
    Email = 1,
    /// <summary>
    /// 短信
    /// </summary>
    [Display(Name = "短信")]
    Sms = 2,
    /// <summary>
    /// 微信
    /// </summary>
    [Display(Name = "微信")]
    Weixin = 3
}
