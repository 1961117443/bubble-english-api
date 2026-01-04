using QT.Common.Const;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys;

/// <summary>
/// 导航菜单生成模型
/// </summary>
[SugarTable("cms_manager_menu_model")]
[Tenant(ClaimConst.TENANTID)]
public class ManagerMenuModel
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 父节点
    /// </summary>
    [Display(Name = "父节点")]
    public int ParentId { get; set; } = 0;

    /// <summary>
    /// 导航类型
    /// </summary>
    [Display(Name = "导航类型")]
    [StringLength(128)]
    public string? NavType { get; set; }

    /// <summary>
    /// 导航标识
    /// </summary>
    [Display(Name = "导航标识")]
    [StringLength(128)]
    public string? Name { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [StringLength(128)]
    public string? Title { get; set; }

    /// <summary>
    /// 副标题
    /// </summary>
    [Display(Name = "副标题")]
    [StringLength(128)]
    public string? SubTitle { get; set; }

    /// <summary>
    /// 图标地址
    /// </summary>
    [Display(Name = "图标地址")]
    [StringLength(512)]
    public string? IconUrl { get; set; }

    /// <summary>
    /// 链接地址
    /// </summary>
    [Display(Name = "链接地址")]
    [StringLength(512)]
    public string? LinkUrl { get; set; }

    /// <summary>
    /// 控制器名称
    /// </summary>
    [Display(Name = "控制器名称")]
    [StringLength(128)]
    public string? Controller { get; set; }

    /// <summary>
    /// 资源权限
    /// </summary>
    [Display(Name = "资源权限")]
    [StringLength(512)]
    public string? Resource { get; set; }

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int SortId { get; set; } = 99;
}
