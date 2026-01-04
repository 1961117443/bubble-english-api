using QT.Systems.Dto.Crm;
using QT.Systems.Entitys.Dto.User;
using QT.Systems.Entitys.Dto.UsersCurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Systems.Entitys.Dto.Permission.ExperienceUsers;

public class ExperienceUserInfoOutput: UserInfoOutput
{
    /// <summary>
    /// 到期时间
    /// </summary>
    public DateTime? expireTime { get; set; }

    /// <summary>
    /// 沟通记录
    /// </summary>
    public List<CrmUserCommunicationInfoOutput> userCommunicationList { get; set; }

    /// <summary>
    /// 账户登录记录
    /// </summary>
    public List<UsersCurrentSystemLogOutput> loginLogData { get; set; }

    /// <summary>
    /// 业务经理
    /// </summary>
    public string sid { get; set; }

    /// <summary>
    /// 推荐码
    /// </summary>
    public string miniProgramQRCode { get; set; }
}

public class ExperienceUserUpInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }



    ///// <summary>
    ///// 账户.
    ///// </summary>
    //public string account { get; set; }

    /// <summary>
    /// 用户姓名.
    /// </summary>
    public string realName { get; set; }

    ///// <summary>
    ///// 机构ID.
    ///// </summary>
    //public string organizeId { get; set; }

    ///// <summary>
    ///// 我的主管.
    ///// </summary>
    //public string managerId { get; set; }

    ///// <summary>
    ///// 岗位主键.
    ///// </summary>
    //public string positionId { get; set; }

    ///// <summary>
    ///// 角色主键.
    ///// </summary>
    //public string roleId { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 性别.
    /// </summary>
    public int? gender { get; set; }

    ///// <summary>
    ///// 有效标志.
    ///// </summary>
    //public int? enabledMark { get; set; }

    ///// <summary>
    ///// 民族.
    ///// </summary>
    //public string nation { get; set; }

    ///// <summary>
    ///// 籍贯.
    ///// </summary>
    //public string nativePlace { get; set; }

    ///// <summary>
    ///// 证件类型.
    ///// </summary>
    //public string certificatesType { get; set; }

    ///// <summary>
    ///// 证件号码.
    ///// </summary>
    //public string certificatesNumber { get; set; }

    ///// <summary>
    ///// 文化程度.
    ///// </summary>
    //public string education { get; set; }

    ///// <summary>
    ///// 生日.
    ///// </summary>
    //public DateTime? birthday { get; set; }

    ///// <summary>
    ///// 入职日期.
    ///// </summary>
    //public DateTime? entryDate { get; set; }

    ///// <summary>
    ///// 电话.
    ///// </summary>
    //public string telePhone { get; set; }

    ///// <summary>
    ///// 固定电话.
    ///// </summary>
    //public string landline { get; set; }

    ///// <summary>
    ///// 手机.
    ///// </summary>
    //public string mobilePhone { get; set; }

    /// <summary>
    /// 邮箱.
    /// </summary>
    public string email { get; set; }

    ///// <summary>
    ///// 紧急联系人.
    ///// </summary>
    //public string urgentContacts { get; set; }

    ///// <summary>
    ///// 紧急电话.
    ///// </summary>
    //public string urgentTelePhone { get; set; }

    ///// <summary>
    ///// 通讯地址.
    ///// </summary>
    //public string postalAddress { get; set; }

    ///// <summary>
    ///// 头像.
    ///// </summary>
    //public string headIcon { get; set; }

    ///// <summary>
    ///// 排序.
    ///// </summary>
    //public long? sortCode { get; set; }

    ///// <summary>
    ///// 分组ID.
    ///// </summary>
    //public string groupId { get; set; }



    /// <summary>
    /// 到期时间
    /// </summary>
    public DateTime? expireTime { get; set; }
}