using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 会员积分记录
/// </summary>
[SugarTable("cms_member_point_log")]
public class MemberPointLog
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 所属用户
    /// </summary>
    [Display(Name = "所属用户")]
    public string? UserId { get; set; }

    /// <summary>
    /// 增减积分
    /// </summary>
    [Display(Name = "增减积分")]
    public int Value { get; set; } = 0;

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;
}
