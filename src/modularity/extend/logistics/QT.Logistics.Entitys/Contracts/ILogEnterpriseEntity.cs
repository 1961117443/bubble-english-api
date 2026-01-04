using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics;

/// <summary>
/// 商家实体接口
/// </summary>
public interface ILogEnterpriseEntity
{
    /// <summary>
    /// 商家ID.
    /// </summary>
    public string EId { get; set; }
}
