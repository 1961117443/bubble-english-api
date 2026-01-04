using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Systems.Entitys.Dto.System.SysConfig;

public class SysConfigGlobalOutput
{
    /// <summary>
    /// 是否多租户版本
    /// </summary>
    public bool multiTenancy { get; set; }


    /// <summary>
    /// 是否自动登录，根据域名去判断
    /// </summary>
    public bool autoLogin { get; set; }
}
