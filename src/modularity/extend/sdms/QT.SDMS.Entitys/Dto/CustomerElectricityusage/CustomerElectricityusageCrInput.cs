using NPOI.Util;
using QT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace QT.SDMS.Entitys.Dto.CustomerElectricityusage;

public class CustomerElectricityusageCrInput
{
    public string meterPointId { get; set; } 

    public DateTime? recordDate { get; set; }

    /// <summary>
    /// 峰用电量
    /// </summary>
    public decimal? peakUsage { get; set; }

    /// <summary>
    /// 谷用电量
    /// </summary>
    public decimal? valleyUsage { get; set; }

    /// <summary>
    /// 平用电量
    /// </summary>
    public decimal? flatUsage { get; set; }

    /// <summary>
    /// 尖用电量
    /// </summary>
    public decimal? spikeUsage { get; set; }

    /// <summary>
    /// 合计用电量
    /// </summary>
    public decimal? totalUsage { get; set; }
}