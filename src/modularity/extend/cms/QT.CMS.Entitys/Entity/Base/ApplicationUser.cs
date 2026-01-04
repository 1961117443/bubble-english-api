//using Microsoft.AspNetCore.Identity;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Text;

//namespace QTMall.Core.Model.Models
//{
//    /// <summary>
//    /// 1.用户表主健设置为int
//    /// </summary>
//    public class ApplicationUser : IdentityUser<int>
//    {
//        /// <summary>
//        /// 状态(0正常1待验证2待审核3锁定)
//        /// </summary>
//        [Display(Name = "账户状态")]
//        [Range(0, 9)]
//        public byte Status { get; set; } = 0;

//        /// <summary>
//        /// 用于登录刷新的Token
//        /// </summary>
//        [StringLength(512)]
//        public string? RefreshToken { get; set; }

//        //定义导航属性
//        public virtual Members? Member { get; set; }
//        public virtual Manager? Manager { get; set; }
//        public virtual ICollection<ApplicationUserClaim>? Claims { get; set; }
//        public virtual ICollection<ApplicationUserLogin>? Logins { get; set; }
//        public virtual ICollection<ApplicationUserToken>? Tokens { get; set; }
//        public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }
//    }

//    /// <summary>
//    /// 2.角色表主健设置为int
//    /// </summary>
//    public class ApplicationRole : IdentityRole<int>
//    {
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
//        [Required]
//        [StringLength(128)]
//        public string? Title { get; set; }

//        /// <summary>
//        /// 0否1是
//        /// 系统默认不可删除
//        /// </summary>
//        [Display(Name = "系统默认")]
//        [Range(0, 9)]
//        public byte IsSystem { get; set; } = 0;

//        //定义导航属性
//        public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }
//        public virtual ICollection<ApplicationRoleClaim>? RoleClaims { get; set; }
//    }

//    /// <summary>
//    /// 3.用户和角色关联
//    /// </summary>
//    public class ApplicationUserRole : IdentityUserRole<int>
//    {
//        //定义导航属性
//        public virtual ApplicationUser? User { get; set; }
//        public virtual ApplicationRole? Role { get; set; }
//    }

//    /// <summary>
//    /// 4.用户拥有的权限
//    /// </summary>
//    public class ApplicationUserClaim : IdentityUserClaim<int>
//    {
//        //定义导航属性
//        public virtual ApplicationUser? User { get; set; }
//    }

//    /// <summary>
//    /// 5.用户与登录相关联
//    /// </summary>
//    public class ApplicationUserLogin : IdentityUserLogin<int>
//    {
//        //定义导航属性
//        public virtual ApplicationUser? User { get; set; }
//    }

//    /// <summary>
//    /// 6.角色拥有的权限
//    /// </summary>
//    public class ApplicationRoleClaim : IdentityRoleClaim<int>
//    {
//        //定义导航属性
//        public virtual ApplicationRole? Role { get; set; }
//    }

//    /// <summary>
//    /// 7.用户的身份验证令牌
//    /// </summary>
//    public class ApplicationUserToken : IdentityUserToken<int>
//    {
//        //定义导航属性
//        public virtual ApplicationUser? User { get; set; }
//    }
//}
