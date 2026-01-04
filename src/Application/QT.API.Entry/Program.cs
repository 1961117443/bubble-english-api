using System.Text.Encodings.Web;
using System.Text.Json;
using QT.Common.Core.Filter;
using QT.JsonSerialization;
using QT.UnifyResult;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QT;
using Yarp.ReverseProxy.Transforms;
//测试修改
var builder = WebApplication.CreateBuilder(args).Inject();
builder.Host.UseSerilogDefault();
builder.Logging.AddJsonConsole(options =>
{
    options.JsonWriterOptions = new JsonWriterOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
});

builder.Services.AddControllersWithViews()
    .AddMvcFilter<RequestActionFilter>()
    .AddInjectWithUnifyResult<RESTfulResultProvider>()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    })
    .AddNewtonsoftJson(options =>
    {
        // 默认命名规则
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();

        // 设置时区为 UTC
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

        // 格式化json输出的日期格式
        options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";

        // 忽略空值
        // options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

        // 忽略循环引用
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        // 格式化json输出的日期格式为时间戳
        options.SerializerSettings.Converters.Add(new NewtonsoftDateTimeJsonConverter());
    });

// 添加yarp反向代理
builder.Services.AddHttpForwarder();

var app = builder.Build();

app.Configuration.Get<WebHostBuilder>().ConfigureKestrel(options =>
{
    // 长度最好不要设置 null
    options.Limits.MaxRequestBodySize = 52428800;
});
app.Run();