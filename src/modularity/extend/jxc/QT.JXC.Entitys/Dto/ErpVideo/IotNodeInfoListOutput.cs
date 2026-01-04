using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Dto.ErpVideo;

public class IotNodeInfoListOutput
{
    public string id { get; set; }

    public string name { get; set; }

    public string lonLat { get; set; }

    public string iot_node_status { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string device_code { get; set; }

    public List<IotNodeSubOutput> iotNodeSubList { get; set; }
}



public class IotNodeSubOutput
{
    public string id { get; set; }

    /// <summary>
    /// 网关id
    /// </summary>
    public string nodeId { get; set; }

    /// <summary>
    /// 从机地址号
    /// </summary>
    public string sensorDeviceId { get; set; }

    /// <summary>
    /// 从机名称
    /// </summary>
    public string name { get; set; }
}