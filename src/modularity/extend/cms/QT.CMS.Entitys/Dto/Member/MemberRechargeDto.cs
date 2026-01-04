using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Member;

/// <summary>
/// 会员充值(显示)
/// </summary>
public class MemberRechargeDto : MemberRechargeEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public string id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名")]
    [StringLength(128)]
    public string? userName { get; set; }

    /// <summary>
    /// 交易单号
    /// </summary>
    [Display(Name = "交易单号")]
    [StringLength(128)]
    public string? tradeNo { get; set; }

    /// <summary>
    /// 支付方式名称
    /// </summary>
    [Display(Name = "支付方式名称")]
    [StringLength(128)]
    public string? paymentTitle { get; set; }

    /// <summary>
    /// 收款状态
    /// 1.待支付
    /// 2.已支付
    /// 3.已取消
    /// </summary>
    [Display(Name = "收款状态")]
    public byte status { get; set; } = 1;

    /// <summary>
    /// 完成时间
    /// </summary>
    [Display(Name = "完成时间")]
    public DateTime? completeTime { get; set; }
}

/// <summary>
/// 会员充值(增改)
/// </summary>
public class MemberRechargeEditDto
{
    /// <summary>
    /// 充值用户ID
    /// </summary>
    [Display(Name = "充值用户")]
    public string? userId { get; set; }

    /// <summary>
    /// 支付方式ID
    /// </summary>
    [Display(Name = "支付方式")]
    [Required(ErrorMessage = "{0}不可为空")]
    public int paymentId { get; set; }

    /// <summary>
    /// 充值金额
    /// </summary>
    [Display(Name = "充值金额")]
    [Required(ErrorMessage = "{0}不可为空")]
    public decimal amount { get; set; } = 0;
}
