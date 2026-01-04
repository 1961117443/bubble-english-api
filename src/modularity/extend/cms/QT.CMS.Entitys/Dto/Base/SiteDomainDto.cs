using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Base;

public class SiteDomainDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public int id { get; set; }

    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int siteId { get; set; }

    /// <summary>
    /// 站点域名
    /// </summary>
    [Display(Name = "站点域名")]
    public string? domain { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    public string? remark { get; set; }
}
