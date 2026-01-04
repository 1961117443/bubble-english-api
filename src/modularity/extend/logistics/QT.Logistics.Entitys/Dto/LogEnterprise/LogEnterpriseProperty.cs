using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Entitys.Dto.LogEnterprise;

public class LogEnterpriseProperty
{
    /// <summary>
    /// 属性
    /// </summary>
    public string prop { get; set; }

    /// <summary>
    /// 属性值
    /// </summary>
    public string value { get; set; }

    /// <summary>
    /// 属性描述
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool enable { get; set; }
}
