using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JZRC.Entitys;

public enum JzrcAmountLogEnum
{
    None = 0,
    /// <summary>
    /// 保证金
    /// </summary>
    Margin,
    /// <summary>
    /// 充值
    /// </summary>
    Recharge,
}
