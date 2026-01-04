using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using QT.Common.Dtos.OAuth;
using QT.Common.Extension;
using QT.Extras.DatabaseAccessor.SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace QT.API.Core.Handlers;

public class TenantOssMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _memoryCache;
    private readonly IHttpClientFactory _httpClientFactory;

    public TenantOssMiddleware(RequestDelegate next, IMemoryCache memoryCache,IHttpClientFactory httpClientFactory)
    {
        _next = next;
        _memoryCache = memoryCache;
        _httpClientFactory = httpClientFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantList = _memoryCache.Get<List<ITenantDbInfo>>("tenant:dbcontext:list");
        if (tenantList.IsAny())
        {
            var path = context.Request.Path.ToString();
            foreach (var item in tenantList)
            {
                if (item is TenantInterFaceOutput interFaceOutput && interFaceOutput.ossConfig!=null && interFaceOutput.ossConfig.BucketName.IsNotEmptyOrNull())
                {
                    var _routePrefix = $"/{interFaceOutput.ossConfig.BucketName}/";
                    if (path.StartsWith(_routePrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        await HandleReverseProxyRequest(context, interFaceOutput.ossConfig);
                        return;
                    }
                   
                }
            }
        }

        await _next(context);
    }


    private async Task HandleReverseProxyRequest(HttpContext context,TenantOssConfig ossConfig)
    {
        var _httpClient = _httpClientFactory.CreateClient();
        // 构造目标URL
        var targetPath = context.Request.Path.ToString();
        var endpoint = ossConfig.Endpoint?.TrimEnd('/');
        endpoint= $"{(ossConfig.IsEnableHttps ? "https" : "http")}://{endpoint}";
        var targetUrl = $"{endpoint}{targetPath}{context.Request.QueryString}";

        using var targetRequest = new HttpRequestMessage();
        // 复制请求方法
        targetRequest.Method = new HttpMethod(context.Request.Method);

        //// 复制请求头（跳过特定头）
        //foreach (var header in context.Request.Headers)
        //{
        //    if (ShouldSkipHeader(header.Key)) continue;

        //    if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
        //    {
        //        targetRequest.Headers.Host = header.Value.ToString();
        //    }
        //    else
        //    {
        //        if (!targetRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
        //        {
        //            targetRequest.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        //        }
        //    }
        //}

        // 复制请求体（非GET/HEAD请求）
        if (context.Request.ContentLength > 0)
        {
            targetRequest.Content = new StreamContent(context.Request.Body);
            // 复制内容类型头
            if (!string.IsNullOrEmpty(context.Request.ContentType))
            {
                targetRequest.Content.Headers.ContentType =
                    MediaTypeHeaderValue.Parse(context.Request.ContentType);
            }
        }

        // 发送请求到目标服务器
        targetRequest.RequestUri = new Uri(targetUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(3); // 设置超时时间
        using var responseMessage = await _httpClient.SendAsync(
                targetRequest,
                HttpCompletionOption.ResponseHeadersRead,
                context.RequestAborted
            );

        // 复制响应状态码
        context.Response.StatusCode = (int)responseMessage.StatusCode;

        // 复制响应头
        foreach (var header in responseMessage.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }
        foreach (var header in responseMessage.Content.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        // 发送响应体
        await responseMessage.Content.CopyToAsync(context.Response.Body);
    }

    private static bool ShouldSkipHeader(string headerName)
    {
        // 跳过不需要转发的头
        string[] skippedHeaders = { "Connection", "Keep-Alive", "Proxy-Authenticate",
                                   "Proxy-Authorization", "TE", "Trailer",
                                   "Transfer-Encoding", "Upgrade" };
        return skippedHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }
}
