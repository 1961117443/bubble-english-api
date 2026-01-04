using System;
using System.Collections.Generic;
using System.Text;

namespace QT.CMS.Entitys.Dto.Config;

/// <summary>
/// 订单设置
/// </summary>
public class OrderConfigDto
{
    /// <summary>
    /// 订单支付有效时间(分钟)
    /// </summary>
    public int orderExpired { get; set; } = 60;

    /// <summary>
    /// 订单自动确认时间(天)
    /// </summary>
    public int ordereComplete { get; set; } = 10;

    /// <summary>
    /// 订单自动结算时间(天)
    /// </summary>
    public int ordereSettle { get; set; } = 15;

    /// <summary>
    /// 税费类型1百分比2固定金额
    /// </summary>
    public int taxType { get; set; }

    /// <summary>
    /// 百分比取值范围：0-100，固定金额单位为“元”
    /// </summary>
    public decimal taxaMount { get; set; }

    /// <summary>
    /// 订单确认通知0关闭1短信2邮件
    /// </summary>
    public int confirmMsg { get; set; }

    /// <summary>
    /// 通知模板别名
    /// </summary>
    public string? confirmCallindex { get; set; }

    /// <summary>
    /// 订单发货通知0关闭1短信2邮件
    /// </summary>
    public int expressMsg { get; set; }

    /// <summary>
    /// 通知模板别名
    /// </summary>
    public string? expressCallindex { get; set; }

    /// <summary>
    /// 订单完成通知0关闭1短信2邮件
    /// </summary>
    public int completeMsg { get; set; }

    /// <summary>
    /// 通知模板别名
    /// </summary>
    public string? completeCallindex { get; set; }

    /// <summary>
    /// 快递100API地址
    /// </summary>
    public string? kuaidiApi { get; set; }

    /// <summary>
    /// 快递100Key
    /// </summary>
    public string? kuaidiKey { get; set; }

    /// <summary>
    /// 快递100授权码
    /// </summary>
    public string? kuaidiCust { get; set; }

    /// <summary>
    /// 物流跟踪返回0json字符串1xml对象2html表格3文本
    /// </summary>
    public int kuaidiShow { get; set; }

    /// <summary>
    /// 跟踪信息排序asc：按时间由旧到新,desc：按时间由新到旧
    /// </summary>
    public string? kuaidiOrder { get; set; }
}
