using QT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Entitys.Dto.LogPCWeb;

public class LogEnterpriseWebListOutput
{
    public string id { get; set; }
    public string name { get; set; }
    public string admin { get; set; }
    public string phone { get; set; }

    public string leader { get; set; }

    public IEnumerable<LogEnterpriseProductWebListOutput> items { get; set; }

    public List<FileControlsModel> imageUrl { get; set; }
}

public class LogEnterpriseProductWebListOutput
{
    public string id { get; set; }
    public string eid { get; set; }
    public string name { get; set; }
    public string producer { get; set; }
    public string retention { get; set; }
    public string storage { get; set; }
    public string remark { get; set; }

    public List<FileControlsModel> imageUrl { get; set; }
}
