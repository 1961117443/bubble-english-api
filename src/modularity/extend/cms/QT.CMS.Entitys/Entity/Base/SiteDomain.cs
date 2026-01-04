using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 站点域名
/// </summary>
[SugarTable("cms_site_domain")]
[Tenant(ClaimConst.TENANTID)]
public class SiteDomain
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsIdentity =true,IsPrimaryKey = true)]
    public int Id { get; set; }

    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    [ForeignKey("Site")]
    public int SiteId { get; set; }

    /// <summary>
    /// 站点域名
    /// </summary>
    [Display(Name = "站点域名")]
    [Required]
    [StringLength(128)]
    public string? Domain { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 站点信息
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(SiteId))]
    public Sites? Site { get; set; }
}
