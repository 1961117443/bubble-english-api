using QT.Systems.Entitys.Permission;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QT.CMS.Entitys;

/// <summary>
/// 开放平台授权
/// </summary>
[SugarTable("cms_site_oauth_login")]
public class SiteOAuthLogin
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 所属开放平台
    /// </summary>
    [Display(Name = "所属平台")]
    [ForeignKey("OAuth")]
    public int OAuthId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    [Display(Name = "所属用户")]
    [ForeignKey("User")]
    public string UserId { get; set; }

    /// <summary>
    /// 平台标识
    /// qq|wechat
    /// </summary>
    [Display(Name = "平台标识")]
    [Required]
    [StringLength(128)]
    public string? Provider { get; set; }

    /// <summary>
    /// 开放平台用户OpenId
    /// </summary>
    [Display(Name = "OpenId")]
    [Required]
    [StringLength(512)]
    public string? OpenId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 开放平台信息
    /// </summary>
    public SiteOAuths? OAuth { get; set; }

    /// <summary>
    /// 用户信息
    /// </summary>
    public UserEntity? User { get; set; }
}
