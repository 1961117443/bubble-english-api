using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Common.Core.Manager.Parameter;

public interface IParameterManager
{
    /// <summary>
    /// 获取参数的值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<object> GetParameterValue(string key);
}
