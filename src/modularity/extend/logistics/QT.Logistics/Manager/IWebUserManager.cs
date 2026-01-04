using QT.Logistics.Entitys.Dto.LogPCWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Manager;

public interface IWebUserManager
{
    /// <summary>
    /// 用户编号.
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// 用户角色.
    /// </summary>
    LoginUserRoleType Role { get; }

    /// <summary>
    /// 用户账号.
    /// </summary>
    string Account { get; }

    /// <summary>
    /// 用户昵称.
    /// </summary>
    string RealName { get; }
}
