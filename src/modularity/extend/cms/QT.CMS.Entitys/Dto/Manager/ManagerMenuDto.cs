//using System.ComponentModel.DataAnnotations;

//namespace QTMall.Core.Model.ViewModels
//{
//    /// <summary>
//    /// 管理菜单(显示)
//    /// </summary>
//    public class ManagerMenuDto : ManagerMenuEditDto
//    {
//        /// <summary>
//        /// 自增ID
//        /// </summary>
//        [Display(Name = "自增ID")]
//        public int Id { get; set; }

//        /// <summary>
//        /// 子菜单列表
//        /// </summary>
//        public List<ManagerMenuDto> Children { get; set; } = new List<ManagerMenuDto>();
//    }

//    /// <summary>
//    /// 导航菜单(编辑)
//    /// </summary>
//    public class ManagerMenuEditDto
//    {
//        /// <summary>
//        /// 父导航ID
//        /// </summary>
//        [Display(Name = "父导航")]
//        public int? ParentId { get; set; } = 0;

//        /// <summary>
//        /// 所属频道ID
//        /// </summary>
//        [Display(Name = "所属频道")]
//        public int? ChannelId { get; set; } = 0;

//        /// <summary>
//        /// 导航标识
//        /// </summary>
//        [Display(Name = "导航标识")]
//        [Required(ErrorMessage = "{0}不可为空")]
//        [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
//        public string? Name { get; set; }

//        /// <summary>
//        /// 标题
//        /// </summary>
//        [Display(Name = "标题")]
//        [Required(ErrorMessage = "{0}不可为空")]
//        [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
//        public string? Title { get; set; }

//        /// <summary>
//        /// 副标题
//        /// </summary>
//        [Display(Name = "副标题")]
//        [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
//        public string? SubTitle { get; set; }

//        /// <summary>
//        /// 图标地址
//        /// </summary>
//        [Display(Name = "图标地址")]
//        [StringLength(512, ErrorMessage = "{0}不可超出{1}字符")]
//        public string? IconUrl { get; set; }

//        /// <summary>
//        /// 链接地址
//        /// </summary>
//        [Display(Name = "链接地址")]
//        [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
//        public string? LinkUrl { get; set; }

//        /// <summary>
//        /// 排序数字
//        /// </summary>
//        [Display(Name = "排序数字")]
//        public int SortId { get; set; } = 99;

//        /// <summary>
//        /// 状态0显示1隐藏
//        /// </summary>
//        [Display(Name = "状态")]
//        public byte Status { get; set; } = 0;

//        /// <summary>
//        /// 系统默认0否1是
//        /// </summary>
//        [Display(Name = "系统默认")]
//        public byte IsSystem { get; set; } = 0;

//        /// <summary>
//        /// 备注说明
//        /// </summary>
//        [Display(Name = "备注说明")]
//        [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
//        public string? Remark { get; set; }

//        /// <summary>
//        /// 控制器名称
//        /// </summary>
//        [Display(Name = "控制器名称")]
//        [Required(ErrorMessage = "{0}不可为空")]
//        [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
//        public string? Controller { get; set; }

//        /// <summary>
//        /// 权限资源
//        /// </summary>
//        [Display(Name = "权限资源")]
//        [Required(ErrorMessage = "{0}不可为空")]
//        [MaxLength(512, ErrorMessage = "{0}不可超出{1}字符")]
//        public string? Resource { get; set; }
//    }

//    /// <summary>
//    /// 导航菜单(前台)
//    /// </summary>
//    public class ManagerMenuClientDto
//    {
//        /// <summary>
//        /// 自增ID
//        /// </summary>
//        public int Id { get; set; }

//        /// <summary>
//        /// 父节点ID
//        /// </summary>
//        public int ParentId { get; set; }

//        /// <summary>
//        /// 频道ID
//        /// </summary>
//        public int ChannelId { get; set; }

//        /// <summary>
//        /// 菜单名称
//        /// </summary>
//        public string? Name { get; set; }

//        /// <summary>
//        /// 菜单标题
//        /// </summary>
//        public string? Text { get; set; }

//        /// <summary>
//        /// 图标
//        /// </summary>
//        public string? Icon { get; set; }

//        /// <summary>
//        /// 链接
//        /// </summary>
//        public string? Href { get; set; }

//        /// <summary>
//        /// 是否展开
//        /// </summary>
//        public bool IsExpand { get; set; } = false;

//        /// <summary>
//        /// 是否选中
//        /// </summary>
//        public bool IsSelected { get; set; } = false;

//        /// <summary>
//        /// 子菜单列表
//        /// </summary>
//        public List<ManagerMenuClientDto> Children { get; set; } = new List<ManagerMenuClientDto>();
//    }

//    /// <summary>
//    /// 导航菜单(角色)
//    /// </summary>
//    public class ManagerMenuRoleDto
//    {
//        /// <summary>
//        /// 自增ID
//        /// </summary>
//        public int Id { get; set; }

//        /// <summary>
//        /// 名称
//        /// </summary>
//        public string? Name { get; set; }

//        /// <summary>
//        /// 标题
//        /// </summary>
//        public string? Title { get; set; }

//        /// <summary>
//        /// 控制器名称
//        /// </summary>
//        public string? Controller { get; set; }

//        /// <summary>
//        /// 权限列表
//        /// </summary>
//        public List<ManagerMenuResourceDto> Resource { get; set; } = new List<ManagerMenuResourceDto>();

//        /// <summary>
//        /// 子菜单列表
//        /// </summary>
//        public List<ManagerMenuRoleDto> Children { get; set; } = new List<ManagerMenuRoleDto>();
//    }

//    /// <summary>
//    /// 菜单权限资源
//    /// </summary>
//    public class ManagerMenuResourceDto
//    {
//        /// <summary>
//        /// 名称
//        /// </summary>
//        public string? Name { get; set; }

//        /// <summary>
//        /// 标题
//        /// </summary>
//        public string? Title { get; set; }

//        /// <summary>
//        /// 是否选中
//        /// </summary>
//        public bool IsSelected { get; set; } = false;
//    }
//}
