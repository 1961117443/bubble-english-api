using Mapster.Utils;
using Microsoft.AspNetCore.Http;
using Minio;
using QT.Common.Const;
using QT.Logistics.Entitys.Dto.LogPCWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Manager;

public class WebUserManager : IWebUserManager ,IScoped
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClaimsPrincipal _user;

    public WebUserManager(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _user = _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
    }
    public string UserId => _user.FindFirst(ClaimConst.CLAINMUSERID)?.Value ?? throw Oops.Oh(ErrorCode.D1022);

    public LoginUserRoleType Role
    {
        get
        {
            var role = _user.FindFirst("Role")?.Value ?? throw Oops.Oh(ErrorCode.D1022);
            return Enum<LoginUserRoleType>.Parse(role);
        }
    }

    public string Account => _user.FindFirst(ClaimConst.CLAINMACCOUNT)?.Value ?? throw Oops.Oh(ErrorCode.D1022);

    public string RealName => _user.FindFirst(ClaimConst.CLAINMREALNAME)?.Value ?? throw Oops.Oh(ErrorCode.D1022);
}
