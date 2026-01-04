//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Text;

//namespace QTMall.Core.Model.ViewModels
//{
//    /// <summary>
//    /// 管理员角色(显示)
//    /// </summary>
//    public class ManagerRolesDto : ManagerRolesEditDto
//    {
//        /// <summary>
//        /// 自增ID
//        /// </summary>
//        [Display(Name = "自增ID")]
//        public int Id { get; set; }
//    }

//    /// <summary>
//    /// 管理员角色(编辑)
//    /// </summary>
//    public class ManagerRolesEditDto
//    {
//        /// <summary>
//        /// 角色标识
//        /// </summary>
//        [Display(Name = "角色标识")]
//        [Required(ErrorMessage = "{0}不可为空")]
//        [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
//        public string? Name { get; set; }

//        /// <summary>
//        /// 0普通会员
//        /// 1普通管理员
//        /// 2超级管理员
//        /// </summary>
//        [Display(Name = "角色类型")]
//        [Range(0, 9)]
//        public byte RoleType { get; set; } = 0;

//        /// <summary>
//        /// 角色备注名
//        /// </summary>
//        [Display(Name = "备注名")]
//        [Required(ErrorMessage = "{0}不可为空")]
//        [MaxLength(128, ErrorMessage = "{0}不可超出{1}字符")]
//        public string? Title { get; set; }

//        /// <summary>
//        /// 0否1是
//        /// 系统默认不可删除
//        /// </summary>
//        [Display(Name = "系统默认")]
//        [Range(0, 9)]
//        public byte IsSystem { get; set; } = 0;

//        /// <summary>
//        /// 子菜单列表
//        /// </summary>
//        public ICollection<ManagerMenuRoleDto> Navigation { get; set; } = new List<ManagerMenuRoleDto>();
//    }
//}
