using Newtonsoft.Json;
using QT.Common.Core.Manager.Tenant;
using QT.Common.Extension;
using QT.JXC.Entitys.Dto.Erp.gz_cycs;
using QT.RemoteRequest;
using QT.Systems.Entitys.System;

namespace QT.JXC.gz_cycs;

public interface IHttpProxy : IHttpDispatchProxy
{
    /// <summary>
    /// 上报进货登记
    /// </summary>
    /// <param name="input"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Post("https://spzs.amr.guizhou.gov.cn/gz_cycs/out-api/tCycsInAdd")]
    Task<CycsApiResponseBase> tCycsInAdd([Body]List<CycsInAddInfo> input, [Interceptor(InterceptorTypes.Request)] Action<HttpClient, HttpRequestMessage> action = default);

    /// <summary>
    /// 产品信息上报
    /// </summary>
    /// <param name="input"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Post("https://spzs.amr.guizhou.gov.cn/gz_cycs/out-api/tCycsGoodsAdd")]
    Task<CycsGoodsAddResponse> tCycsGoodsAdd([Body] List<CycsGoodsAddInfo> input, [Interceptor(InterceptorTypes.Request)] Action<HttpClient, HttpRequestMessage> action = default);


    /// <summary>
    /// 供应商信息数据上报
    /// </summary>
    /// <param name="input"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Post("https://spzs.amr.guizhou.gov.cn/gz_cycs/out-api/tCycsSupplierAdd")]
    Task<CycsGoodsAddResponse> tCycsSupplierAdd([Body] List<CycsSupplierAddInfo> input, [Interceptor(InterceptorTypes.Request)] Action<HttpClient, HttpRequestMessage> action = default);

    /// <summary>
    /// 进货检测上报数据
    /// </summary>
    /// <param name="input"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Post("https://spzs.amr.guizhou.gov.cn/gz_cycs/out-api/tCycsTestAdd")]
    Task<string> tCycsTestAdd([Body] List<CycsSupplierAddInfo> input, [Interceptor(InterceptorTypes.Request)] Action<HttpClient, HttpRequestMessage> action = default);



    /// <summary>
    /// 查询地区的编码信息
    /// </summary>
    /// <param name="input"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Get("https://spzs.amr.guizhou.gov.cn/gz_cycs/out-api/tCycsDivision")]
    Task<CycsApiResponse<List<tCycsDivisionInfo>>> tCycsDivision([QueryString] string name = default, [QueryString] string code = default, [Interceptor(InterceptorTypes.Request)] Action<HttpClient, HttpRequestMessage> action = default);

    /// <summary>
    /// 获取产品的分类信息
    /// </summary>
    /// <param name="input"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Get("https://spzs.amr.guizhou.gov.cn/gz_cycs/out-api/tCycgoodsType")]
    Task<CycsApiResponse<List<tCycgoodsTypeInfo>>> tCycgoodsType([QueryString] string name = default, [QueryString] string code = default, [Interceptor(InterceptorTypes.Request)] Action<HttpClient, HttpRequestMessage> action = default);

    /// <summary>
    /// 产品信息查询
    /// </summary>
    /// <param name="input"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Get("https://spzs.amr.guizhou.gov.cn/gz_cycs/out-api/tCycsGoodsList")]
    Task<CycsApiResponse<List<CycsGoodsInfo>>> tCycsGoodsList([QueryString] int cycsId , [QueryString] int type =1, [QueryString] string code = default, [Interceptor(InterceptorTypes.Request)] Action<HttpClient, HttpRequestMessage> action = default);

    /// <summary>
    /// 供应商信息查询
    /// </summary>
    /// <param name="input"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    [Get("https://spzs.amr.guizhou.gov.cn/gz_cycs/out-api/tCycsSupplier")]
    Task<CycsApiResponse<List<CycsSupplierInfo>>> tCycsSupplier([QueryString] int cycsId, [QueryString] string supplyName = default, [Interceptor(InterceptorTypes.Request)] Action<HttpClient, HttpRequestMessage> action = default);



    // 全局拦截，类中每一个方法都会触发
    [Interceptor(InterceptorTypes.Request)]
    static void OnRequesting(HttpRequestMessage req)
    {
        //Console.WriteLine("OnRequesting当前租户：{0}", TenantScoped.TenantId);
        var _cache = App.GetService<ICacheManager>(TenantScoped.ServiceProvider);
        var config = _cache.GetOrCreate("gz_cycs", entry =>
        {
            var rep = App.GetService<ISqlSugarRepository<SysConfigEntity>>(TenantScoped.ServiceProvider);

            var APP_KEY = "gz_cycs.clientId";
            var APP_SECRET = "gz_cycs.secret";
            var configs = rep.Where(it => it.Category == "gz_cycs")
            .Where(it => it.Key == APP_KEY || it.Key == APP_SECRET).Select(it => new
            {
                it.Key,
                it.Value
            }).ToList();

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            var data = new { clientId = configs.FirstOrDefault(it => it.Key == APP_KEY)?.Value, secret = configs.FirstOrDefault(it => it.Key == APP_SECRET)?.Value };

            if (string.IsNullOrEmpty(data.clientId) || string.IsNullOrEmpty(data.secret))
            {
                throw Oops.Oh("请联系管理员，配置贵州超市追溯云平台的秘钥！【gz_cycs.clientId,gz_cycs.secret】");
            }

            return data;
        });

       

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        // 追加更多参数
        var code = $"{config.clientId}:{config.secret}";
        req.Headers.Add("Authorization", $"Basic {code.ToBase64String()}");
    }
}


public class CycsTestAddInfo
{
    [JsonProperty("cycsId")]
    public string CycsId { get; set; } // 所属门店id

    [JsonProperty("checkDate")]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.IsoDateTimeConverter))]
    public DateTime CheckDate { get; set; } // 检测日期

    [JsonProperty("goodsCode")]
    public string GoodsCode { get; set; } // 检测商品id 
    [JsonProperty("detectionUnit")]
    public string DetectionUnit { get; set; } // 检测单位0 企业内部检测 1 第三方检测 2 监管局抽检
    [JsonProperty("inspector")]
    public string Inspector { get; set; } // 检测人
    [JsonProperty("checkResult")]
    public string CheckResult { get; set; } // 检测结果 0 未检出 1 检出
    [JsonProperty("actionResults")]
    public string ActionResults { get; set; } // 处置结果 0 未处置 1处置
    [JsonProperty("disposalDate")]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.IsoDateTimeConverter))]
    public DateTime DisposalDate { get; set; } // 处置日期
    [JsonProperty("reportName")]
    public string ReportName { get; set; } // 附件名称多文件,分割
    [JsonProperty("reportUrl")]
    public string ReportUrl { get; set; } // 附件urlbase64编码，多文件,分割
}

public class CycsGoodsAddInfo
{
    public int cycsId { get; set; } // 商品名称

    public string localName { get; set; } // 商品名称

    public string code { get; set; } // 商品编码

    public string barCode { get; set; } // 商品条形码

    public string manufacturer { get; set; } // 生产厂商名称
}

public class CycsGoodsInfo : CycsGoodsAddInfo
{
    public int id { get; set; }
}

public class CycsSupplierInfo : CycsSupplierAddInfo
{
    public int id { get; set; }
}

public class CycsApiResponseBase
{
    public string msg { get; set; }

    public int code { get; set; }
}

public class CycsApiResponse<T> : CycsApiResponseBase
{

    public T data { get; set; }
}

public class tCycsDivisionInfo
{
    public int id { get; set; }
    public string name { get; set; }
    public string code { get; set; }
    public string parentCode { get; set; }
}

public class tCycgoodsTypeInfo
{
    public int id { get; set; }
    public string name { get; set; }
    public string code { get; set; }
    public string parentCode { get; set; }
}

public class CycsGoodsAddResponse
{
    public string msg { get; set; }

    public int code { get; set; }

    public string ids { get; set; }
}