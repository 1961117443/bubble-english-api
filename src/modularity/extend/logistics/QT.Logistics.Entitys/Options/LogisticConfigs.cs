using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Entitys.Options;

public class LogisticConfigs
{
    /// <summary>
    /// 三网短信应用凭证
    /// </summary>
    public string thirdAppKey { get; set; }

    /// <summary>
    /// 三网短信凭证密钥
    /// </summary>
    public string thirdAppSecret { get; set; }

    /// <summary>
    /// 三网短信服务地址
    /// </summary>
    public string thirdServer { get; set; }
}
