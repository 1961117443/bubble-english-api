using QT.Extras.WebSockets.Models;
using QT.WebSockets;
using QT.Common.Const;
using QT.Common.Configuration;
using QT.Common.Core.Manager;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Models.User;
using QT.Common.Security;
using QT.Common.Core.Security;
using QT.DataEncryption;
using QT.Systems.Entitys.Permission;
using QT.Message.Entitys;
using QT.Message.Entitys.Dto.IM;
using QT.Message.Entitys.Model.IM;
using QT.Message.Entitys.Enums;
using SqlSugar;
using System.Net.WebSockets;
using Mapster;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.AspNetCore.Http;
using Serilog;
using QT.Common.Core.Manager.Tenant;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using QT.Common.Options;
using Microsoft.Extensions.DependencyInjection;

namespace QT.Message.Handlers;

/// <summary>
/// IM 处理程序.
/// </summary>
public class IMHandler : WebSocketHandler
{
    ///// <summary>
    ///// SqlSugarClient客户端.
    ///// </summary>
    //private SqlSugarClient _sqlSugarClient;

    /// <summary>
    /// 缓存管理.
    /// </summary>
    //private ICacheManager _cacheManager;
    private readonly IOptions<TenantOptions> _tenantOptions;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 初始化一个<see cref="IMHandler"/>类型的新实例.
    /// </summary>
    public IMHandler(
        WebSocketConnectionManager webSocketConnectionManager,
        //ISqlSugarClient sqlSugarClient,
        //ICacheManager cacheManager,
        IOptions<TenantOptions> tenantOptions,IServiceProvider serviceProvider)
        : base(webSocketConnectionManager)
    {
        //_sqlSugarClient = (SqlSugarClient)sqlSugarClient;
        //_cacheManager = cacheManager;
        _tenantOptions = tenantOptions;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 消息接收.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="result"></param>
    /// <param name="receivedMessage"></param>
    /// <returns></returns>
    public override async Task ReceiveAsync(WebSocketClient client, WebSocketReceiveResult result, string receivedMessage)
    {
        try
        {
            MessageInput? message = receivedMessage.ToObject<MessageInput>();
            if (!string.IsNullOrEmpty(message.token))
            {
                if (message.token.ToLower().IndexOf("bearer") < 0)
                {
                    await OnDisconnected(client.WebSocket);
                }
                else
                {
                    var token = new JsonWebToken(message.token.Replace("Bearer ", string.Empty).Replace("bearer ", string.Empty));
                    var httpContext = (DefaultHttpContext)App.HttpContext;
                    httpContext.Request.Headers["Authorization"] = message.token;
                    if (!JWTEncryption.ValidateJwtBearerToken(httpContext, out token))
                    {
                        await OnDisconnected(client.WebSocket);
                    }
                    else
                    {
                        var claims = JWTEncryption.ReadJwtToken(message.token.Replace("Bearer ", string.Empty).Replace("bearer ", string.Empty))?.Claims;
                        client.UserId = claims.FirstOrDefault(e => e.Type == ClaimConst.CLAINMUSERID)?.Value;
                        client.TenantId = claims.FirstOrDefault(e => e.Type == ClaimConst.TENANTID)?.Value;
                        client.TenantDBName = claims.FirstOrDefault(e => e.Type == ClaimConst.TENANTDBNAME)?.Value;
                        client.Account = claims.FirstOrDefault(e => e.Type == ClaimConst.CLAINMACCOUNT)?.Value;
                        client.UserName = claims.FirstOrDefault(e => e.Type == ClaimConst.CLAINMREALNAME)?.Value;
                        client.SingleLogin = (LoginMethod)Enum.Parse(typeof(LoginMethod), claims.FirstOrDefault(e => e.Type == ClaimConst.SINGLELOGIN)?.Value);
                        client.LoginTime = string.Format("{0:yyyy-MM-dd HH:mm}", string.Format("{0}000", claims.FirstOrDefault(e => e.Type == "iat")?.Value).TimeStampToDateTime());
                        client.LoginIpAddress = client.LoginIpAddress;
                        client.Token = message.token;
                        client.IsMobileDevice = message.mobileDevice;
                        if (client.WebSocket.State != WebSocketState.Open) return;
                        await OnConnected(client.ConnectionId, client);
                        WebSocketConnectionManager.AddToTenant(client.ConnectionId, client.TenantId);
                        WebSocketConnectionManager.AddToUser(client.ConnectionId, string.Format("{0}-{1}", client.TenantId, client.UserId));
                        message.sendClientId = client.ConnectionId;

                        // 设置租户号
                        if (!string.IsNullOrEmpty(client.TenantId))
                        {
                            httpContext.Items["TenantId"] = client.TenantId;
                        }
                        
                        await MessageRoute(message);
                    }
                }
            }
            else
            {
                await OnDisconnected(client.WebSocket);
            }
        }
        catch (Exception e)
        {
            //Log.Information(e.StackTrace);
        }
    }

    /// <summary>
    /// 消息通道.
    /// </summary>
    /// <param name="message"></param>
    private async Task MessageRoute(MessageInput message)
    {
        WebSocketClient client = WebSocketConnectionManager.GetSocketById(message.sendClientId);
        if (string.IsNullOrEmpty(client.UserId))
        {
            await SendMessageAsync(client.ConnectionId, new { method = "logout" }.ToJsonString());
            return;
        }

        using var scoped = _serviceProvider.CreateScope();
        using var db = scoped.ServiceProvider.GetService<ISqlSugarClient>();
        var _sqlSugarClient = db as SqlSugarClient;
        if (_tenantOptions.Value.MultiTenancy)
        {
            //var cache = App.GetService<IMemoryCache>();
            //var sugarTenant = new TenantManager(null, _sqlSugarClient, cache, _tenantOptions);
            //sugarTenant.Login(client.TenantId);
            //if (sugarTenant == null || !sugarTenant.IsLoggedIn)
            if (!TenantManager.Login(client.TenantId, _sqlSugarClient))
            {
                //var dbType = (DbType)Enum.Parse(typeof(DbType), _config["ConnectionStrings:DBType"]);
                //if (!Context.IsAnyConnection(_tenantId))
                //{
                //    Context.AddConnection(new ConnectionConfig()
                //    {
                //        DbType = dbType,
                //        ConfigId = _tenantId,//设置库的唯一标识
                //        IsAutoCloseConnection = true,
                //        ConnectionString = string.Format($"{_config["ConnectionStrings:DefaultConnection"]}", tenantDbName),
                //        ConfigureExternalServices = new ConfigureExternalServices() { }
                //    });
                //}
                //Context.ChangeDatabase(_tenantId);
                //var ex = new Exception("租户数据库登录失败");

                //throw new Exception("租户数据库登录失败");

                await OnDisconnected(client.WebSocket);
                return;
            }
            _sqlSugarClient.ChangeDatabase(client.TenantId);
        }

        var _cacheManager = scoped.ServiceProvider.GetService<ICacheManager>();

        //_sqlSugarClient = _sqlSugarClient.GetConnection(client.TenantId);
        /*
        _sqlSugarClient = new SqlSugarClient(new ConnectionConfig()
        {
            DbType = (DbType)Enum.Parse(typeof(DbType), App.Configuration["ConnectionStrings:DBType"]),
            ConfigId = client.TenantId, // 设置库的唯一标识
            IsAutoCloseConnection = true,
            ConnectionString = string.Format($"{App.Configuration["ConnectionStrings:DefaultConnection"]}", client.TenantDBName)
        });

        _sqlSugarClient.Ado.CommandTimeOut = 10;

        // 验证连接是否成功
        if (!_sqlSugarClient.Ado.IsValidConnection())
        {
            await OnDisconnected(client.WebSocket);
            return;
        }

        _sqlSugarClient.Aop.OnLogExecuting = (sql, pars) =>
        {
            if (sql.StartsWith("SELECT"))
                Console.ForegroundColor = ConsoleColor.Green;

            if (sql.StartsWith("UPDATE") || sql.StartsWith("INSERT"))
                Console.ForegroundColor = ConsoleColor.White;

            if (sql.StartsWith("DELETE"))
                Console.ForegroundColor = ConsoleColor.Blue;

            // 在控制台输出sql语句
            Console.WriteLine(SqlProfiler.ParameterFormat(sql, pars));
            Console.WriteLine();

            // 在MiniProfiler内显示
            // App.PrintToMiniProfiler("SqlSugar", "Info", SqlProfiler.ParameterFormat(sql, pars));
        };
        */

        if (string.IsNullOrEmpty(client.HeadIcon))
        {
            UserEntity userEntity = await _sqlSugarClient.Queryable<UserEntity>().SingleAsync(it => it.Id == client.UserId);
            if (userEntity != null)
            {
                client.HeadIcon = "/api/file/Image/userAvatar/" + userEntity.HeadIcon;
                await OnConnected(client.ConnectionId, client);
            }
        }

        switch (message.method)
        {
            // 建立连接
            case MothodType.OnConnection:
                {
                    List<UserOnlineModel> list = await GetOnlineUserList(client.TenantId);
                    if (list == null)
                    {
                        list = new List<UserOnlineModel>();
                    }

                    switch (client.SingleLogin)
                    {
                        case LoginMethod.Single:
                            {
                                UserOnlineModel? user = list.Find(it => it.userId.Equals(client.UserId) && it.isMobileDevice.Equals(client.IsMobileDevice));
                                if (user == null)
                                {
                                    list.Add(new UserOnlineModel()
                                    {
                                        connectionId = client.ConnectionId,
                                        userId = client.UserId,
                                        account = client.Account,
                                        userName = client.UserName,
                                        lastTime = DateTime.Now,
                                        lastLoginIp = client.LoginIpAddress,
                                        tenantId = client.TenantId,
                                        lastLoginPlatForm = client.LoginPlatForm,
                                        isMobileDevice = client.IsMobileDevice,
                                        token = message.token
                                    });
                                    await SetOnlineUserList(client.TenantId, list);
                                }

                                // 不同浏览器
                                else if (user != null && !user.token.Equals(message.token))
                                {
                                    var onlineUser = WebSocketConnectionManager.GetSocketById(user.connectionId);
                                    if (onlineUser != null)
                                        await SendMessageAsync(onlineUser.ConnectionId, new { method = MessageSendType.logout.ToString(), msg = "此账号已在其他地方登陆" }.ToJsonString());
                                    list.RemoveAll((x) => x.connectionId == user.connectionId);

                                    list.Add(new UserOnlineModel()
                                    {
                                        connectionId = client.ConnectionId,
                                        userId = client.UserId,
                                        account = client.Account,
                                        userName = client.UserName,
                                        lastTime = DateTime.Now,
                                        lastLoginIp = client.LoginIpAddress,
                                        tenantId = client.TenantId,
                                        lastLoginPlatForm = client.LoginPlatForm,
                                        isMobileDevice = client.IsMobileDevice,
                                        token = message.token
                                    });
                                    await SetOnlineUserList(client.TenantId, list);
                                }
                            }

                            break;
                        case LoginMethod.SameTime:
                            {
                                UserOnlineModel? user = list.Find(it => it.token.Equals(message.token));
                                if (user != null)
                                {
                                    WebSocketClient? onlineUser = WebSocketConnectionManager.GetSocketById(user.connectionId);
                                    if (onlineUser != null)
                                        await SendMessageAsync(onlineUser.ConnectionId, new { method = MessageSendType.closeSocket.ToString() }.ToJsonString());
                                    list.RemoveAll((x) => x.connectionId == user.connectionId);
                                }

                                list.Add(new UserOnlineModel()
                                {
                                    connectionId = client.ConnectionId,
                                    userId = client.UserId,
                                    account = client.Account,
                                    userName = client.UserName,
                                    lastTime = DateTime.Now,
                                    lastLoginIp = client.LoginIpAddress,
                                    tenantId = client.TenantId,
                                    lastLoginPlatForm = client.LoginPlatForm,
                                    isMobileDevice = client.IsMobileDevice,
                                    token = message.token
                                });
                                await SetOnlineUserList(client.TenantId, list);
                            }

                            break;
                    }

                    var onlineUserList = GetAllUserIdFromTenant(client.TenantId);

                    // 获取接收者为当前用户的聊天且未读的信息
                    var imContentList = _sqlSugarClient.Queryable<IMContentEntity>().Where(x => x.ReceiveUserId.Equals(client.UserId) && x.State.Equals(0)).GroupBy(x => new { x.SendUserId, x.ReceiveUserId }).Select(x => new IMContentEntity
                    {
                        State = SqlFunc.AggregateSum(SqlFunc.IIF(x.State == 0, 1, 0)),
                        SendUserId = x.SendUserId,
                        ReceiveUserId = x.ReceiveUserId
                    }).ToList();

                    var receiveList = _sqlSugarClient.Queryable<IMContentEntity>().Where(x => x.ReceiveUserId == client.UserId).OrderBy(x => x.SendTime, OrderByType.Desc).ToList();
                    var unreadNums = imContentList.Adapt<List<IMUnreadNumModel>>();
                    foreach (var item in unreadNums)
                    {
                        var entity = receiveList.FirstOrDefault(x => x.SendUserId == item.sendUserId);
                        item.defaultMessage = entity?.Content;
                        item.defaultMessageType = entity?.ContentType;
                        item.defaultMessageTime = entity?.SendTime.ToString();
                    }

                    var unreadNoticeCount = await _sqlSugarClient.Queryable<MessageEntity, MessageReceiveEntity>((m, mr) => new JoinQueryInfos(JoinType.Left, m.Id == mr.MessageId)).Where((m, mr) => m.Type == 1 && m.DeleteMark == null && mr.UserId == client.UserId && mr.IsRead == 0).Select((m, mr) => new { mr.Id, mr.UserId, mr.IsRead, m.Type, m.DeleteMark }).CountAsync();
                    var unreadMessageCount = await _sqlSugarClient.Queryable<MessageEntity, MessageReceiveEntity>((m, mr) => new JoinQueryInfos(JoinType.Left, m.Id == mr.MessageId)).Select((m, mr) => new { mr.Id, mr.UserId, mr.IsRead, m.Type, m.DeleteMark }).MergeTable().Where(x => x.Type == 2 && x.DeleteMark == null && x.UserId == client.UserId && x.IsRead == 0).CountAsync();
                    var noticeDefault = await _sqlSugarClient.Queryable<MessageEntity>().Where(x => x.Type == 1 && x.DeleteMark == null && x.EnabledMark == 1).OrderBy(x => x.CreatorTime, OrderByType.Desc).FirstAsync();
                    var noticeDefaultText = noticeDefault == null ? string.Empty : noticeDefault.Title;
                    var messageDefault = await _sqlSugarClient.Queryable<MessageEntity, MessageReceiveEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.Id == b.MessageId)).Where((a, b) => a.Type == 2 && a.DeleteMark == null && b.UserId == client.UserId).OrderBy(a => a.CreatorTime, OrderByType.Desc).Select(a => a).FirstAsync();
                    var messageDefaultText = messageDefault == null ? string.Empty : messageDefault.Title;
                    var messageDefaultTime = messageDefault == null ? DateTime.Now : messageDefault.CreatorTime;
                    var noticeDefaultTime = noticeDefault == null ? DateTime.Now : noticeDefault.CreatorTime;
                    await SendMessageAsync(client.ConnectionId, new { method = MessageSendType.initMessage.ToString(), onlineUserList, unreadNums, unreadNoticeCount, noticeDefaultText, unreadMessageCount, messageDefaultText, messageDefaultTime, noticeDefaultTime }.ToJsonString());

                    await SendMessageToTenantAsync(client.TenantId, new { method = MessageSendType.online.ToString(), userId = client.UserId }.ToJsonString(), client.ConnectionId);
                }

                break;

            // 发送消息
            case MothodType.SendMessage:
                {
                    string toUserId = message.toUserId;
                    MessageReceiveType messageType = message.messageType;
                    object messageContent = message.messageContent;
                    string fileName = string.Empty;

                    var toUserEntity = await _sqlSugarClient.Queryable<UserEntity>().SingleAsync(it => it.Id == toUserId);

                    // 将发送消息对象信息补全
                    var toAccount = toUserEntity.Account;
                    var toHeadIcon = toUserEntity.HeadIcon;
                    var toRealName = toUserEntity.RealName;

                    var entity = new IMContentEntity();
                    var toMessage = new object();
                    switch (messageType)
                    {
                        case MessageReceiveType.text:
                            entity = CreateIMContent(client.UserId, toUserId, messageContent.ToString(), messageType.ToString());
                            break;
                        case MessageReceiveType.image:
                            {
                                var directoryPath = FileVariable.IMContentFilePath;
                                if (!Directory.Exists(directoryPath))
                                    Directory.CreateDirectory(directoryPath);
                                var imageInput = messageContent.ToObject<MessagetImageInput>();
                                fileName = fileName = imageInput.name;

                                toMessage = new { path = "/api/file/Image/IM/" + fileName, width = imageInput.width, height = imageInput.height };

                                entity = CreateIMContent(client.UserId, toUserId, toMessage.ToJsonString(), messageType.ToString());
                            }

                            break;
                        case MessageReceiveType.voice:
                            var voiceInput = messageContent.ToObject<MessageVoiceInput>();
                            toMessage = new { path = "/api/file/Image/IM/" + voiceInput.name, length = voiceInput.length };
                            entity = CreateIMContent(client.UserId, toUserId, toMessage.ToJsonString(), messageType.ToString());
                            break;
                    }

                    // 写入到会话表中
                    if (await _sqlSugarClient.Queryable<ImReplyEntity>().AnyAsync(it => it.UserId == client.UserId && it.ReceiveUserId == toUserId))
                    {
                        var imReplyEntity = await _sqlSugarClient.Queryable<ImReplyEntity>().SingleAsync(it => it.UserId == client.UserId && it.ReceiveUserId == toUserId);
                        imReplyEntity.ReceiveTime = entity.SendTime;
                        await _sqlSugarClient.Updateable(imReplyEntity).ExecuteCommandAsync();
                    }
                    else
                    {
                        var imReplyEntity = new ImReplyEntity()
                        {
                            Id = SnowflakeIdHelper.NextId(),
                            UserId = client.UserId,
                            ReceiveUserId = toUserId,
                            ReceiveTime = entity.SendTime
                        };
                        await _sqlSugarClient.Insertable(imReplyEntity).ExecuteCommandAsync();
                    }

                    await _sqlSugarClient.Insertable(entity).ExecuteCommandAsync();

                    switch (messageType)
                    {
                        case MessageReceiveType.text:
                            await SendMessageAsync(client.ConnectionId, new { method = MessageSendType.sendMessage.ToString(), client.UserId, account = client.Account, headIcon = client.HeadIcon, realName = client.UserName, toAccount, toHeadIcon, messageType = messageType.ToString(), toUserId, toRealName, toMessage = messageContent, dateTime = DateTime.Now, latestDate = DateTime.Now }.ToJsonString());
                            break;
                        case MessageReceiveType.image:
                            var imageInput = messageContent.ToObject<MessagetImageInput>();
                            toMessage = new { path = "/api/file/Image/IM/" + fileName, width = imageInput.width, height = imageInput.height };
                            await SendMessageAsync(client.ConnectionId, new { method = MessageSendType.sendMessage.ToString(), client.UserId, account = client.Account, headIcon = client.HeadIcon, realName = client.UserName, toAccount, toHeadIcon, messageType = messageType.ToString(), toUserId, toMessage, dateTime = DateTime.Now, latestDate = DateTime.Now }.ToJsonString());
                            break;
                        case MessageReceiveType.voice:
                            var voiceInput = messageContent.ToObject<MessageVoiceInput>();
                            toMessage = new { path = "/api/file/Image/IM/" + voiceInput.name, length = voiceInput.length };
                            await SendMessageAsync(client.ConnectionId, new { method = MessageSendType.sendMessage.ToString(), client.UserId, account = client.Account, headIcon = client.HeadIcon, realName = client.UserName, toAccount, toHeadIcon, messageType = messageType.ToString(), toUserId, toMessage, dateTime = DateTime.Now }.ToJsonString());
                            break;
                    }

                    if (WebSocketConnectionManager.GetSocketClientToUserCount(string.Format("{0}-{1}", client.TenantId, toUserId)) > 0)
                    {
                        switch (messageType)
                        {
                            case MessageReceiveType.text:
                                await SendMessageToUserAsync(string.Format("{0}-{1}", client.TenantId, toUserId), new { method = MessageSendType.receiveMessage.ToString(), messageType = messageType.ToString(), formUserId = client.UserId, formMessage = messageContent, dateTime = DateTime.Now, latestDate = DateTime.Now, headIcon = client.HeadIcon, realName = client.UserName, account = client.Account }.ToJsonString());
                                break;
                            case MessageReceiveType.image:
                                var imageInput = messageContent.ToObject<MessagetImageInput>();
                                var formMessage = new { path = "/api/file/Image/IM/" + fileName, width = imageInput.width, height = imageInput.height };
                                await SendMessageToUserAsync(string.Format("{0}-{1}", client.TenantId, toUserId), new { method = MessageSendType.receiveMessage.ToString(), messageType = messageType.ToString(), formUserId = client.UserId, formMessage, dateTime = DateTime.Now, latestDate = DateTime.Now, headIcon = client.HeadIcon, realName = client.UserName, account = client.Account }.ToJsonString());
                                break;
                            case MessageReceiveType.voice:
                                var voiceInput = messageContent.ToObject<MessageVoiceInput>();
                                toMessage = new { path = "/api/file/Image/IM/" + voiceInput.name, length = voiceInput.length };
                                await SendMessageToUserAsync(string.Format("{0}-{1}", client.TenantId, toUserId), new { method = MessageSendType.receiveMessage.ToString(), messageType = messageType.ToString(), formUserId = client.UserId, formMessage = toMessage, dateTime = DateTime.Now, latestDate = DateTime.Now, headIcon = client.HeadIcon, realName = client.UserName, account = client.Account }.ToJsonString());
                                break;
                        }
                    }
                }

                break;
            case MothodType.UpdateReadMessage:
                var fromUserId = message.formUserId;
                await _sqlSugarClient.Updateable<IMContentEntity>()
                    .SetColumns(x => new IMContentEntity()
                    {
                        State = 1,
                        ReceiveTime = DateTime.Now
                    }).Where(x => x.State == 0 && x.SendUserId == fromUserId && x.ReceiveUserId == client.UserId).ExecuteCommandAsync();
                break;
            case MothodType.MessageList:
                var sendUserId = message.toUserId; // 发送者
                var receiveUserId = message.formUserId; // 接收者

                var data = await _sqlSugarClient.Queryable<IMContentEntity>().WhereIF(!string.IsNullOrEmpty(message.keyword), it => it.Content.Contains(message.keyword))
                        .Where(i => (i.SendUserId == message.toUserId && i.ReceiveUserId == message.formUserId) || (i.SendUserId == message.formUserId && i.ReceiveUserId == message.toUserId)).OrderBy(it => it.SendTime, message.sord == "asc" ? OrderByType.Asc : OrderByType.Desc)
                        .Select(it => new IMContentListOutput
                        {
                            id = it.Id,
                            sendUserId = it.SendUserId,
                            sendTime = it.SendTime,
                            receiveUserId = it.ReceiveUserId,
                            receiveTime = it.ReceiveTime,
                            content = it.Content,
                            contentType = it.ContentType,
                            state = it.State
                        }).ToPagedListAsync(message.currentPage, message.pageSize);

                await SendMessageAsync(client.ConnectionId, new { method = MessageSendType.messageList.ToString(), list = data.list.OrderBy(x => x.sendTime), pagination = data.pagination }.ToJsonString());
                break;
            case MothodType.HeartCheck: break;
        }
    }

    /// <summary>
    /// 获取在线用户列表.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <returns></returns>
    public async Task<List<UserOnlineModel>> GetOnlineUserList(string tenantId)
    {
        string cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYONLINEUSER, tenantId);
        //if (!string.IsNullOrEmpty(tenantId) && App.HttpContext!=null && !App.HttpContext.Items.ContainsKey("TenantId"))
        //{

        //}
        var _cacheManager = App.GetService<ICacheManager>();
        return await _cacheManager.GetAsync<List<UserOnlineModel>>(cacheKey);
    }

    /// <summary>
    /// 保存在线用户列表.
    /// </summary>
    /// <param name="tenantId">租户ID.</param>
    /// <param name="onlineList">在线用户列表.</param>
    /// <returns></returns>
    public async Task<bool> SetOnlineUserList(string tenantId, List<UserOnlineModel> onlineList)
    {
        var _cacheManager = App.GetService<ICacheManager>();
        return await _cacheManager.SetAsync(string.Format("{0}{1}", CommonConst.CACHEKEYONLINEUSER, tenantId), onlineList);
    }

    /// <summary>
    /// 创建IM内容.
    /// </summary>
    /// <returns></returns>
    private IMContentEntity CreateIMContent(string sendUserId, string receiveUserId, string message, string messageType)
    {
        return new IMContentEntity()
        {
            Id = SnowflakeIdHelper.NextId(),
            SendUserId = sendUserId,
            SendTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
            ReceiveUserId = receiveUserId,
            State = 0,
            Content = message,
            ContentType = messageType
        };
    }
}