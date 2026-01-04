using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Extras.DatabaseAccessor.SqlSugar;

public interface ICompanyEntity
{
    /// <summary>
    /// 公司id
    /// </summary>
    public string Oid { get; set; }
}
