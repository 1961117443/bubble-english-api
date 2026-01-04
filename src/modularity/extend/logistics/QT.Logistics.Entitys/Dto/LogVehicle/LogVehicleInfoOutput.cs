
using QT.Common.Models;
using QT.Logistics.Entitys.Dto.LogEnterpriseAttachment;

namespace QT.Logistics.Entitys.Dto.LogVehicle;

/// <summary>
/// 车辆信息输出参数.
/// </summary>
public class LogVehicleInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 车牌号码.
    /// </summary>
    public string licensePlateNumber { get; set; }

    /// <summary>
    /// 尺寸.
    /// </summary>
    public string size { get; set; }

    /// <summary>
    /// 运送类型.
    /// </summary>
    public string transportType { get; set; }

    /// <summary>
    /// 吨位.
    /// </summary>
    public string tone { get; set; }

    /// <summary>
    /// 驾驶员.
    /// </summary>
    public string driver { get; set; }

    /// <summary>
    /// 驾驶员手机.
    /// </summary>
    public string driverPhone { get; set; }

    /// <summary>
    /// 运送状态.
    /// </summary>
    public string transportStatus { get; set; }


    /// <summary>
    /// 车辆附件.
    /// </summary>
    public List<LogVehicleAttachmentInfoOutput> logVehicleAttachmentList { get; set; }


    /// <summary>
    /// 简图.
    /// </summary>
    public List<FileControlsModel> imageUrl { get; set; }

    /// <summary>
    /// 关联班次
    /// </summary>
    public List<string> cIdList { get; set; }
}