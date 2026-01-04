using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Contracts;

public interface IBillRule
{
    /// <summary>
    /// 获取流水号.
    /// </summary>
    /// <param name="enCode">流水编码.</param>
    /// <param name="isCache">是否缓存：每个用户会自动占用一个流水号，这个刷新页面也不会跳号.</param>
    /// <returns></returns>
    Task<string> GetBillNumber(string enCode, bool isCache = false);
}
