using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Core.Configs;

public class IotConfigs
{
    /// <summary>
    /// 自动创建的账号，默认的角色
    /// </summary>
    public string defaultRoleId { get; set; }


    ///// <summary>
    ///// 只读的角色（禁止更新数据库的数据）
    ///// </summary>
    //public string readonlyRoleId { get; set; }

    /// <summary>
    /// 启用体验角色,自动创建的账号
    /// </summary>
    public bool isUseDemoRole { get; set; }

    /// <summary>
    /// 默认体验时间（多少天）
    /// </summary>
    public int defaultExperienceDays { get; set; }
}
