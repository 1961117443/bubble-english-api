namespace QT.JXC.Entitys.Dto.ErpVideo;

/// <summary>
/// 表示回放控制的请求参数。
/// </summary>
public class ControlPlaybackRequest
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
    /// 回放控制。
    ///０：开始回放；
    ///１：暂停回放；
    ///２：结束回放；
    ///３：快进回放；
    ///４：关键帧快退回放；
    ///５：拖动回放；
    ///６：关键帧播放
    /// </summary>
    public byte ControlType { get; set; }

    /// <summary>
    /// 快进或快退倍数。回放控制为 ３ 和 ４ 时，此字段内容有效，否则置 ０。
    /// ０：无效；
    /// １：１ 倍；
    /// ２：２ 倍；
    /// ３：４ 倍；
    /// ４：８ 倍；
    /// ５：１６ 倍
    /// </summary>
    public byte Speed { get; set; }

    /// <summary>
    /// 拖动回放时的时间位置。ＹＹ⁃ＭＭ⁃ＤＤ⁃ＨＨ⁃ＭＭ⁃ＳＳ，回放控制为 ５ 时，此字段有效。
    /// </summary>
    public DateTime DragPosition { get; internal set; }

    /// <summary>
    /// 初始化 <see cref="ControlPlaybackRequest"/> 类的新实例。
    /// </summary>
    public ControlPlaybackRequest() { }

    /// <summary>
    /// 使用指定参数初始化 <see cref="ControlPlaybackRequest"/> 类的新实例。
    /// </summary>
    /// <param name="sim">终端SIM卡号。</param>
    /// <param name="controlType">控制类型。</param>
    /// <param name="speed">回放速度。</param>
    public ControlPlaybackRequest(string sim, byte controlType, byte speed)
    {
        Sim = sim;
        ControlType = controlType;
        Speed = speed;
    }
}
