//using QT.Common.Const;
//using SqlSugar;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Text;

//namespace QT.CMS.Entitys;

///// <summary>
///// 导航菜单
///// </summary>
//[SugarTable("cms_manager_menu")]
//[Tenant(ClaimConst.TENANTID)]
//public class ManagerMenu
//{
//    /// <summary>
//    /// 自增ID
//    /// </summary>
//    [Display(Name = "自增ID")]
//    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//    public int Id { get; set; }

//    /// <summary>
//    /// 父导航ID
//    /// </summary>
//    [Display(Name = "父导航")]
//    public int ParentId { get; set; } = 0;

//    /// <summary>
//    /// 所属频道
//    /// </summary>
//    [Display(Name = "所属频道")]
//    public int ChannelId { get; set; } = 0;

//    /// <summary>
//    /// 导航标识
//    /// </summary>
//    [Display(Name = "导航标识")]
//    [Required]
//    [StringLength(128)]
//    public string? Name { get; set; }

//    /// <summary>
//    /// 标题
//    /// </summary>
//    [Display(Name = "标题")]
//    [StringLength(128)]
//    public string? Title { get; set; }

//    /// <summary>
//    /// 副标题
//    /// </summary>
//    [Display(Name = "副标题")]
//    [StringLength(128)]
//    public string? SubTitle { get; set; }

//    [Display(Name = "图标地址")]
//    [StringLength(512)]
//    public string? IconUrl { get; set; }

//    /// <summary>
//    /// 链接地址
//    /// </summary>
//    [Display(Name = "链接地址")]
//    [StringLength(512)]
//    public string? LinkUrl { get; set; }

//    /// <summary>
//    /// 排序数字
//    /// </summary>
//    [Display(Name = "排序数字")]
//    public int SortId { get; set; } = 99;

//    /// <summary>
//    /// 状态0显示1隐藏
//    /// </summary>
//    [Display(Name = "状态")]
//    public byte Status { get; set; } = 0;

//    /// <summary>
//    /// 系统默认0否1是
//    /// </summary>
//    [Display(Name = "系统默认")]
//    public byte IsSystem { get; set; } = 0;

//    /// <summary>
//    /// 备注说明
//    /// </summary>
//    [Display(Name = "备注说明")]
//    [StringLength(512)]
//    public string? Remark { get; set; }

//    /// <summary>
//    /// 控制器名称
//    /// </summary>
//    [Display(Name = "控制器名称")]
//    [StringLength(128)]
//    public string? Controller { get; set; }

//    /// <summary>
//    /// 权限资源
//    /// </summary>
//    [Display(Name = "权限资源")]
//    [StringLength(512)]
//    public string? Resource { get; set; }

//    /// <summary>
//    /// 创建人
//    /// </summary>
//    [Display(Name = "创建人")]
//    [StringLength(128)]
//    public string? AddBy { get; set; }

//    /// <summary>
//    /// 创建时间
//    /// </summary>
//    [Display(Name = "创建时间")]
//    public DateTime AddTime { get; set; } = DateTime.Now;
//}
