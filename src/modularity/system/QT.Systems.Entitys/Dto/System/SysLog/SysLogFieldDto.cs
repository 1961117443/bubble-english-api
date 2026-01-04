using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Systems.Entitys.Dto.System.SysLog;

public class SysLogFieldDto
{
    public string id { get; set; }
    public string tableName { get; set; }
    public string fieldName { get; set; }
    public string description { get; set; }

    public string oldValue { get; set; }
    public string newValue { get; set; }

    public string userName { get; set; }

    public DateTime? createTime { get; set; }
}
