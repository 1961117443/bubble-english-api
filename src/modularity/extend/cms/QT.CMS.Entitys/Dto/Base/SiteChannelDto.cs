using QT.Common.Security;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QT.CMS.Entitys.Dto.Base;

/// <summary>
/// 站点频道(显示)
/// </summary>
public class SiteChannelDto : SiteChannelEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public int id { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    public string? addBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime addTime { get; set; } = DateTime.Now;
}

/// <summary>
/// 站点频道(编辑)
/// </summary>
public class SiteChannelEditDto
{
    /// <summary>
    /// 所属站点ID
    /// </summary>
    [Display(Name = "所属站点")]
    public int siteId { get; set; }

    /// <summary>
    /// 频道名称
    /// </summary>
    [Display(Name = "频道名称")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
    public string? name { get; set; }

    /// <summary>
    /// 频道标题
    /// </summary>
    [Display(Name = "频道标题")]
    [Required(ErrorMessage = "{0}不可为空")]
    [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
    public string? title { get; set; }

    /// <summary>
    /// 是否开启评论
    /// </summary>
    [Display(Name = "是否开启评论")]
    public byte isComment { get; set; } = 0;

    /// <summary>
    /// 是否开启相册
    /// </summary>
    [Display(Name = "是否开启相册")]
    public byte isAlbum { get; set; } = 0;

    /// <summary>
    /// 是否开启附件
    /// </summary>
    [Display(Name = "是否开启附件")]
    public byte isAttach { get; set; } = 0;

    /// <summary>
    /// 是否允许投稿
    /// </summary>
    [Display(Name = "是否允许投稿")]
    public byte isContribute { get; set; } = 0;

    /// <summary>
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int sortId { get; set; } = 99;

    /// <summary>
    /// 状态0正常1禁用
    /// </summary>
    [Display(Name = "状态")]
    public byte status { get; set; } = 0;

    /// <summary>
    /// 字段列表
    /// </summary>
    public ICollection<SiteChannelFieldDto> fields { get; set; } = new List<SiteChannelFieldDto>();
}

public class SiteChannelMenuTreeModel: TreeModel
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public long NumId { get; set; }

    /// <summary>
    /// 父导航ID
    /// </summary>
    [Display(Name = "父导航")]
    public long NumParentId { get; set; } = 0;

    /// <summary>
    /// 所属频道
    /// </summary>
    [Display(Name = "所属频道")]
    public int ChannelId { get; set; } = 0;

    /// <summary>
    /// 导航标识
    /// </summary>
    [Display(Name = "导航标识")]
    [Required]
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
    /// 排序数字
    /// </summary>
    [Display(Name = "排序数字")]
    public int SortId { get; set; } = 99;

    /// <summary>
    /// 状态0显示1隐藏
    /// </summary>
    [Display(Name = "状态")]
    public byte Status { get; set; } = 0;

    /// <summary>
    /// 系统默认0否1是
    /// </summary>
    [Display(Name = "系统默认")]
    public byte IsSystem { get; set; } = 0;

    /// <summary>
    /// 备注说明
    /// </summary>
    [Display(Name = "备注说明")]
    [StringLength(512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 控制器名称
    /// </summary>
    [Display(Name = "控制器名称")]
    [StringLength(128)]
    public string? Controller { get; set; }

    /// <summary>
    /// 权限资源
    /// </summary>
    [Display(Name = "权限资源")]
    [StringLength(512)]
    public string? Resource { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [Display(Name = "创建人")]
    [StringLength(128)]
    public string? AddBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime AddTime { get; set; } = DateTime.Now;
}