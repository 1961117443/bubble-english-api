using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Entitys.Dto.LogOrderClasses;

public class LogVehicleStatusCrInput
{
    /// <summary>
    /// 车辆id
    /// </summary>
    public string vId { get; set; }


    /// <summary>
    /// 经度.
    /// </summary>
    public string longitude { get; set; }

    /// <summary>
    /// 纬度.
    /// </summary>
    public string latitude { get; set; }

    /// <summary>
    /// 数据来源.
    /// </summary>
    public string dataSource { get; set; }

    /// <summary>
    /// 采集设备.
    /// </summary>
    public string collectionDevice { get; set; }

    /// <summary>
    /// 配送点id.
    /// </summary>
    public string pointId { get; set; }
}
