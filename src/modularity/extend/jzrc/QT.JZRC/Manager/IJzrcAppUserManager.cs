using QT.Common.Extension;
using QT.JZRC.Entitys.Dto.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JZRC;

public interface IJzrcAppUserManager
{
    //public AppLoginUser AppLoginUser { get; }

    public string TenantId { get; }
    public string UserId { get; }
    public string NickName { get; }

    public AppLoginUserRole Role { get; }

    Task<UserInfo> GetUserInfo();
}
