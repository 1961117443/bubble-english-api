using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JZRC.Entitys.Dto.AppService;

public class AppLoginCrInput
{
    /// <summary>
    /// 手机号码
    /// </summary>
    public string phone { get; set; }

    /// <summary>
    /// 账号名称
    /// </summary>
    public string name { get; set; }
}
