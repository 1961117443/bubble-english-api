using QT.DependencyInjection;

namespace QT.Systems.Entitys.Dto.DataInterFace;

/// <summary>
/// 数据接口修改输入.
/// </summary>
[SuppressSniffer]
public class DataInterfaceUpInput : DataInterfaceCrInput
{
    /// <summary>
    /// id.
    /// </summary>
    public string id { get; set; }
}