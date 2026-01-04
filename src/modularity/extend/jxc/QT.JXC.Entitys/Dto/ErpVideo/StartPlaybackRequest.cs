namespace QT.JXC.Entitys.Dto.ErpVideo;

/// <summary>
/// 表示启动回放请求的数据结构。
/// </summary>
public class StartPlaybackRequest
{
    /// <summary>
    /// 终端SIM卡号。
    /// </summary>
    public string Sim { get; set; }

    /// <summary>
    /// 逻辑通道号。
    /// </summary>
    public byte ChannelId { get; set; }

    /// <summary>
    /// 回放起始时间。
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 回放结束时间。
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 音视频类型(媒体类型)
    /// 0：audio and video
    /// 1：audio
    /// 2：video
    /// 3：audio or video
    /// </summary>
    public byte MediaType { get; set; }
    /// <summary>
    /// 码流类型
    /// 0：主或子码流
    /// 1：主
    /// 2：子
    /// 如果此通道只传输音频，置为0
    /// </summary>
    public byte StreamType { get; set; }
    /// <summary>
    /// 存储器类型
    /// 0：主或灾备存储器
    /// 1：主存储器
    /// 2：灾备存储器
    /// </summary>
    public byte MemoryType { get; set; }
    /// <summary>
    /// 回放方式
    /// 0：正常
    /// 1：快进
    /// 2：关键帧快退回放
    /// 3：关键帧播放
    /// 4：单帧上传
    /// </summary>
    public byte PlaybackWay { get; set; }
    /// <summary>
    /// 快进或快退倍数，当<see cref="PlaybackWay"/>为1和2时，此字段有效，否则置0
    /// 0：无效
    /// 1：1倍
    /// 2：2倍
    /// 3：4倍
    /// 4：8倍
    /// 5：16倍
    /// </summary>
    public byte PlaySpeed { get; set; }
 
}