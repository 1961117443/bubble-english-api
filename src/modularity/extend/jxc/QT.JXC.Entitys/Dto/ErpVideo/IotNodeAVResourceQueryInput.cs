using System.Threading.Channels;

namespace QT.JXC.Entitys.Dto.ErpVideo;

public class IotNodeAVResourceQueryInput : PageInputBase
{
    /// <summary>
    /// 设备id
    /// </summary>
    public string nodeId { get; set; }

    ///// <summary>
    ///// 日期
    ///// </summary>
    //public DateTime? date { get; set; }


    /// <summary>
    /// 开始时间
    /// </summary>
    public string startTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public string endTime { get; set; }

    /// <summary>
    /// ０：音视频，１：音频，２：视频
    /// </summary>
    public int dataType { get; set; }

    /// <summary>
    /// ０：主存储器或灾备存储器，１：主存储器，２：灾备存储器
    /// </summary>
    public int memoryType { get; set; }

    /// <summary>
    /// ０：主码流或子码流，１：主码流，２：子码流；  如果此通道只传输音频，此字段置 ０
    /// </summary>
    public int streamType { get; set; }

    /// <summary>
    /// 通道号
    /// </summary>
    public int channel { get; set; }
}

public class IotNodeAVResourceDto
{
    public string id { get; set; }

    public string nodeId { get; set; }

    /// <summary>
    /// 状态标志  0:正常，1:上传中，2:已上传，3:上传失败
    /// </summary>
    public byte statusFlag { get; set; }


    /// <summary>
    /// 逻辑通道号
    /// </summary>
    public byte logicChannelNo { get; set; }
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime beginTime { get; set; }
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime endTime { get; set; }

    /// <summary>
    /// 报警标志
    /// </summary>
    public ulong alarmFlag { get; set; }
    /// <summary>
    /// 音视频资源类型
    /// </summary>
    public byte avResourceType { get; set; }
    /// <summary>
    /// 码流类型
    /// </summary>
    public byte streamType { get; set; }
    /// <summary>
    /// 存储器类型
    /// </summary>
    public byte memoryType { get; set; }
    /// <summary>
    /// 文件大小
    /// </summary>
    public uint fileSize { get; set; }

    /// <summary>
    /// 扩展属性
    /// </summary>
    public string extend { get; set; }
}