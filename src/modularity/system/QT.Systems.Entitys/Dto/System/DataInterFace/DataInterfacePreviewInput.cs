using QT.DependencyInjection;
using QT.Systems.Entitys.Model.DataInterFace;

namespace QT.Systems.Entitys.Dto.System.DataInterFace;

/// <summary>
/// 数据接口预览输入.
/// </summary>
[SuppressSniffer]
public class DataInterfacePreviewInput
{
    /// <summary>
    /// 租户id.
    /// </summary>
    public string tenantId { get; set; }

    /// <summary>
    /// 预览参数.
    /// </summary>
    public List<DataInterfaceReqParameter> paramList { get; set; }
}
