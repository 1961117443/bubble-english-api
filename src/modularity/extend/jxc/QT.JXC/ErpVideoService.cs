using Newtonsoft.Json.Linq;
using QT.ClayObject.Extensions;
using QT.Common.Core.Service;
using QT.Common.Extension;
using QT.JXC.Entitys.Dto.ErpVideo;
using QT.JsonSerialization;
using QT.RemoteRequest;
using QT.RemoteRequest.Extensions;
using QT.UnifyResult;

namespace QT.JXC;

/// <summary>
/// QT.Extend 模块的 ErpVideoService 类
/// 调用物联网的视频服务接口
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "Erp", Name = "ErpVideo", Order = 200)]
public class ErpVideoService : IDynamicApiController
{
    private readonly ICacheManager _cacheManager;

    private readonly ICoreSysConfigService _coreSysConfigService;

    public ErpVideoService(ICacheManager cacheManager, ICoreSysConfigService coreSysConfigService)
    {
        _cacheManager = cacheManager;
        _coreSysConfigService = coreSysConfigService;
    }

    // 1.0获取监控设备列表
    [HttpGet("api/ErpVideo/GetDeviceList")]
    [NonUnify]
    public async Task<RESTfulResult<List<IotNodeInfoListOutput>>> GetDeviceList()
    {
        var req =(await CreateApiClient("/api/iot/node/list"))
            .SetQueries(new Dictionary<string, object>
            {
                { "iotNodeType",8 }
            });
        var result = await req.GetAsAsync<RESTfulResult<List<IotNodeInfoListOutput>>>();
        return result;
    }

    // 1.1获取监控设备定位记录
    [HttpGet("api/ErpVideo/GetDeviceLocation")]
    [NonUnify]
    public async Task<RESTfulResult<List<IotNodeGpsDto>>> GetDeviceLocation([FromQuery] IotNodeGpsQueryInput query)
    {
        if (!query.startTime.HasValue)
        {
            query.startTime = DateTime.Now.AddDays(-1);
        }
        if (!query.endTime.HasValue)
        {
            query.endTime = DateTime.Now;
        }
        var req =(await CreateApiClient("/api/iot/node-gps/list"))
            .SetQueries(new Dictionary<string, object>
            {
                { "iotNodeType",8 },
                {"nodeId",query.deviceId},
                {"startTime", query.startTime.Value.ToString("yyyy-MM-dd HH:mm:ss") },
                {"endTime", query.endTime.Value.ToString("yyyy-MM-dd HH:mm:ss") },
                {"maxParkingMinutes",query.maxParkingMinutes ?? 3}
            });
        var result = await req.GetAsAsync<RESTfulResult<List<IotNodeGpsDto>>>();
        return result;
    }


    // 1.2获取监控设备录像列表
    [HttpGet("api/ErpVideo/GetDeviceRecordList")]
    [NonUnify]
    public async Task<RESTfulResult<PageResult<IotNodeAVResourceDto>>> GetDeviceRecordList([FromQuery] IotNodeAVResourceQueryInput query)
    {
        if (query.startTime.IsNullOrEmpty())
        {
            query.startTime = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 00:00:00");
        }
        if (query.endTime.IsNullOrEmpty())
        {
            query.endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        var dict = query.ToDictionary();
        var req = (await CreateApiClient("/api/iot/node-avresource"))
            .SetQueries(query);
        var result = await req.GetAsAsync<RESTfulResult<PageResult<IotNodeAVResourceDto>>>();
        return result;
    }

    // 1.3根据id获取监控设备信息
    [HttpGet("api/ErpVideo/Info/{id}")]
    [NonUnify]
    public async Task<RESTfulResult<IotNodeInfoListOutput>> GetDeviceInfo(string id)
    {
        var req = await CreateApiClient($"/api/iot/node/{id}")
            //.SetQueries(new Dictionary<string, object>
            //{
            //    { "iotNodeType",8 }
            //})
            ;
        var result = await req.GetAsAsync<RESTfulResult<IotNodeInfoListOutput>>();
        return result;
    }

    // 1.4开始直播
    [HttpGet("api/ErpVideo/live/start")]
    [NonUnify]
    public async Task<RESTfulResult<string>> StartLive(string sim, byte channel)
    {
        if (string.IsNullOrEmpty(sim))
        {
            throw Oops.Oh("请选择设备");
        }
        if (channel == 0)
        {
            throw Oops.Oh("请选择通道");
        }
        var req = (await CreateApiClient($"/api/iot/admin/video/live/start"))
            .SetQueries(new Dictionary<string, object>
            {
                { "sim",sim },
                 { "channel",channel }
            })
            ;
        var result = await req.GetAsAsync<RESTfulResult<string>>();
        return result;
    }

    // 1.4停止直播
    [HttpGet("api/ErpVideo/live/stop")]
    [NonUnify]
    public async Task<RESTfulResult<object>> StopLive(string sim, byte channel)
    {
        if (string.IsNullOrEmpty(sim))
        {
            throw Oops.Oh("请选择设备");
        }
        if (channel == 0)
        {
            throw Oops.Oh("请选择通道");
        }
        var req = (await CreateApiClient($"/api/iot/admin/video/live/stop"))
            .SetQueries(new Dictionary<string, object>
            {
                { "sim",sim },
                 { "channel",channel }
            })
            ;
        var result = await req.GetAsAsync<RESTfulResult<object>>();
        return result;
    }

    // 开始历史播放
    [HttpGet("api/ErpVideo/playback/start")]
    [NonUnify]
    public async Task<RESTfulResult<string>> StartPlayback([FromQuery] StartPlaybackRequest input)
    {
        if (string.IsNullOrEmpty(input.Sim))
        {
            throw Oops.Oh("请选择设备");
        }
        if (input.ChannelId == 0)
        {
            throw Oops.Oh("请选择通道");
        }
        var req = (await CreateApiClient($"/api/iot/admin/video/playback/start"))
            .SetBody(input)
            ;
        var result = await req.PostAsAsync<RESTfulResult<string>>();
        return result;
    }

    // 历史播放控制
    [HttpPost("api/ErpVideo/playback/control")]
    [NonUnify]
    public async Task<RESTfulResult<object>> PlaybackControl([FromBody] ControlPlaybackRequest input)
    {
        if (string.IsNullOrEmpty(input.Sim))
        {
            throw Oops.Oh("请选择设备");
        }
        if (input.ChannelId == 0)
        {
            throw Oops.Oh("请选择通道");
        }
        //input.ControlType = 2; // 停止播放
        var req = (await CreateApiClient($"/api/iot/admin/video/playback/control"))
            .SetBody(input)
            ;
        var result = await req.PostAsAsync<RESTfulResult<object>>();
        return result;
    }

    [HttpPost("api/ErpVideo/gps51/{control}")]
    public async Task<dynamic> Gps51Action(string control, [FromBody] JObject request)
    {
        var str = request.ToJsonString();
        var req = (await CreateApiClient($"/api/iot/admin/video/gps51/{control}"))
           .SetBody(request.ToString());

        var result = await req.PostAsAsync<string>();

        if (string.IsNullOrEmpty(result))
        {
            throw Oops.Oh("GPS51接口请求失败");
        }
        return JObject.Parse(result);
    }


    /// <summary>
    /// 创建远程请求客户端
    /// </summary>
    /// <param name="uri"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private async Task<HttpRequestPart> CreateApiClient(string uri)
    {
        if (string.IsNullOrEmpty(uri))
            throw new ArgumentNullException(nameof(uri));

        var config = await _cacheManager.GetOrCreateAsync("qt_iot_api_scret", async entry =>
        {
            var config = await _coreSysConfigService.GetConfig<IotApiConfig>();

            if (config == null || config.iot_api_appid.IsNullOrEmpty() || config.iot_api_scret.IsNullOrEmpty())
            {
                throw Oops.Oh("请先配置物联网API密钥");
            }

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30); // 设置缓存过期时间
            return config; // 这里可以替换为实际的密钥获取逻辑
        });
        var scret = config.iot_api_scret; // "0a51dfce-1db4-4968-8aee-86f1c9b498c2";

        var appid = config.iot_api_appid; //"TWpBd05FQnplWE5oWkcxcGJnPT0=";
        // 创建远程请求客户端
        var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

        var signature = MD5Encryption.Encrypt($"{appid}{timestamp}{scret}"); // 这里需要根据实际情况生成签名
        return uri.SetClient("qt-iot")
            .SetHeaders(new Dictionary<string, object>
            {
                { "X-Api-AppId",appid},
                { "X-Api-Timestamp", timestamp },
                { "X-Api-Signature", signature }
            })
           .SetJsonSerialization<NewtonsoftJsonSerializerProvider>()
            ;
    }

}


public class IotApiConfig
{
    public string iot_api_appid { get; set; }

    public string iot_api_scret { get; set; }
}