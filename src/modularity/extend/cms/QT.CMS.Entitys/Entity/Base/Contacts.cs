using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 联系人
/// </summary>
[SugarTable("cms_contact")]
[Tenant(ClaimConst.TENANTID)]
public class Contacts
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name ="自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true,IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    [Display(Name = "联系电话")]
    [Required]
    [StringLength(128)]
    public string? Phone { get; set; }

    /// <summary>
    /// 主题
    /// </summary>
    [Display(Name = "主题")]
    [Required]
    [StringLength(128)]
    public string? Subject { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    [Display(Name = "IP")]
    [StringLength(512)]
    public string? IP { get; set; }
 

    /// <summary>
    /// 是否已发送短信
    /// </summary>
    [Display(Name = "是否已发送短信")]
    public byte IsSend { get; set; } = 0;


    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(30)]
    public string? AddBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime? AddTime { get; set; } = DateTime.Now;


    /// <summary>
    /// 站点id
    /// </summary>
    [Display(Name = "站点id")]
    public int SiteId { get; set; }


    /// <summary>
    /// 留言
    /// </summary>
    [Display(Name = "留言")]
    [StringLength(512)]
    public string? Remark { get; set; }


    /// <summary>
    /// 扩展信息
    /// </summary>
    [Display(Name = "扩展信息")]
    public string? Extend { get; set; }
}
