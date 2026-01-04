using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Core.Configs;

public class ReadonlyRoleConfigs
{
    /// <summary>
    /// 只读的角色（禁止更新数据库的数据）
    /// </summary>
    public string readonlyRoleId { get; set; }

    /// <summary>
    /// 只读角色的接口白名单
    /// </summary>
    public string readonlyRoleWhiteList { get; set;}
}
