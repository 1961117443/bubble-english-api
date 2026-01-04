using System.Net.WebSockets;
using System.Text;
using QT.Common.Net;
using QT.Common.Security;
using QT.Extras.WebSockets.Models;
using Microsoft.AspNetCore.Http;

namespace QT.WebSockets;

/// <summary>
/// WebSocket 中间件.
/// </summary>
public class WebSocketMiddleware
{
    /// <summary>
    /// 请求委托.
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// webSocket 处理程序.
    /// </summary>
    private WebSocketHandler _webSocketHandler { get; set; }

    /// <summary>
    /// 初始化一个<see cref="WebSocketMiddleware"/>类型的新实例.
    /// </summary>
    /// <param name="next"></param>
    /// <param name="webSocketHandler"></param>
    public WebSocketMiddleware(
        RequestDelegate next,
        WebSocketHandler webSocketHandler)
    {
        _next = next;
        _webSocketHandler = webSocketHandler;
    }

    /// <summary>
    /// 异步调用.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            await _next.Invoke(context);
            return;
        }

        WebSocket? socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);

        var connectionId = Guid.NewGuid().ToString("N");
        var wsClient = new WebSocketClient
        {
            ConnectionId = connectionId,
            WebSocket = socket,
            LoginIpAddress = NetHelper.Ip,
            LoginPlatForm = string.Format("{0}-{1}", UserAgent.GetSystem(), UserAgent.GetBrowser())
        };

        await _webSocketHandler.OnConnected(connectionId, wsClient).ConfigureAwait(false);

        await Receive(wsClient, async (result, serializedMessage) =>
        {
            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                    await _webSocketHandler.ReceiveAsync(wsClient, result, serializedMessage).ConfigureAwait(false);
                    break;
                case WebSocketMessageType.Close:
                    await _webSocketHandler.OnDisconnected(socket);
                    break;
                case WebSocketMessageType.Binary:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        });
    }

    /// <summary>
    /// 接收数据.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="handleMessage"></param>
    /// <returns></returns>
    private async Task Receive(WebSocketClient client, Action<WebSocketReceiveResult, string> handleMessage)
    {
        while (client.WebSocket.State == WebSocketState.Open)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024 * 4]);
            string message = string.Empty;
            WebSocketReceiveResult result = null;
            try
            {
                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await client.WebSocket.ReceiveAsync(buffer, CancellationToken.None);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    }
                    while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        message = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }

                handleMessage(result, message);
            }
            catch (WebSocketException e)
            {
                if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    client.WebSocket.Abort();
                }
            }
        }

        await _webSocketHandler.OnDisconnected(client.WebSocket);
    }
}