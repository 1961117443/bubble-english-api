using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.Common.Security;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.Message.Entitys;
using QT.Message.Entitys.Dto.Message;
using QT.Message.Handlers;
using QT.Message.Interfaces.Message;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace QT.Message;

/// <summary>
/// 系统消息



/// 日 期：2021-06-01.
/// </summary>
[ApiDescriptionSettings(Tag = "Message", Name = "message", Order = 240)]
[Route("api/[controller]")]
public class MessageService : IMessageService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<MessageEntity> _repository;

    private readonly IMHandler _imHandler;

    /// <summary>
    /// 用户服务.
    /// </summary>
    private readonly IUsersService _usersService;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="MessageService"/>类型的新实例.
    /// </summary>
    public MessageService(
        ISqlSugarRepository<MessageEntity> messageRepository,
        IUsersService usersService,
        IUserManager userManager,
        IMHandler imHandler,
        ISqlSugarClient context)
    {
        _repository = messageRepository;
        _usersService = usersService;
        _userManager = userManager;
        _imHandler = imHandler;
        _db = context.AsTenant();
    }

    #region Get

    /// <summary>
    /// 列表（通知公告）.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Notice")]
    public async Task<dynamic> GetNoticeList([FromQuery] PageInputBase input)
    {
        var list = await _repository.Context.Queryable<MessageEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.CreatorUserId))
            .Where(a => a.Type == 1 && a.DeleteMark == null)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.Title.Contains(input.keyword))
            .OrderBy(a => a.EnabledMark).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b) => new MessageNoticeOutput
            {
                id = a.Id,
                lastModifyTime = a.LastModifyTime,
                enabledMark = a.EnabledMark,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                title = a.Title,
                type = a.Type,
                creatorTime = a.CreatorTime,
            }).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<MessageNoticeOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 列表（通知公告/系统消息/私信消息）.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetMessageList([FromQuery] MessageListQueryInput input)
    {
        var list = await _repository.Context.Queryable<MessageEntity, MessageReceiveEntity, UserEntity>((a, b, c) => new JoinQueryInfos(JoinType.Left, a.Id == b.MessageId, JoinType.Left, a.CreatorUserId == c.Id))
            .Where((a, b) => b.UserId == _userManager.UserId && a.DeleteMark == null)
            .WhereIF(input.type.IsNotEmptyOrNull(), a => a.Type == input.type)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.Title.Contains(input.keyword))
            .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b, c) => new MessageListOutput
            {
                id = a.Id,
                lastModifyTime = a.LastModifyTime,
                creatorUser = SqlFunc.MergeString(c.RealName, "/", c.Account),
                title = a.Title,
                type = a.Type,
                isRead = b.IsRead
            }).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<MessageListOutput>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        return await _repository.Context.Queryable<MessageEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.CreatorUserId == b.Id))
            .Where(a => a.Id == id && a.DeleteMark == null)
            .Select((a, b) => new MessageInfoOutput
            {
                id = a.Id,
                lastModifyTime = a.LastModifyTime,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                title = a.Title,
                bodyText = a.BodyText,
                files = a.Files,
                toUserIds = a.ToUserIds,
            }).FirstAsync();
    }

    /// <summary>
    /// 读取消息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("ReadInfo/{id}")]
    public async Task<dynamic> ReadInfo(string id)
    {
        var data = await _repository.Context.Queryable<MessageEntity, UserEntity, MessageReceiveEntity>((a, b, c) => new JoinQueryInfos(JoinType.Left, a.CreatorUserId == b.Id, JoinType.Left, a.Id == c.MessageId))
            .Where((a, b, c) => a.Id == id && a.DeleteMark == null && c.UserId == _userManager.UserId)
            .OrderBy(a => a.LastModifyTime)
            .Select((a, b, c) => new MessageReadInfoOutput
            {
                id = a.Id,
                lastModifyTime = a.LastModifyTime,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                title = a.Title,
                bodyText = c.BodyText,
                files = a.Files
            }).FirstAsync();
        if (data != null)
            await MessageRead(id);
        return data;
    }

    #endregion

    #region Post

    /// <summary>
    /// 删除.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        try
        {
            _repository.Context.BeginTran();

            await _repository.Context.Deleteable<MessageReceiveEntity>().Where(x => x.MessageId == id).ExecuteCommandAsync();
            await _repository.Context.Updateable<MessageEntity>().SetColumns(it => new MessageEntity()
            {
                DeleteMark = 1,
                DeleteUserId = _userManager.UserId,
                DeleteTime = SqlFunc.GetDate()
            }).Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

            _repository.Context.CommitTran();
        }
        catch (Exception)
        {
            _repository.Context.RollbackTran();
            throw Oops.Oh(ErrorCode.COM1002);
        }
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] MessageCrInput input)
    {
        var entity = input.Adapt<MessageEntity>();
        entity.Type = 1;
        entity.EnabledMark = 0;
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Creator()).ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="id">主键值</param>
    /// <param name="input">实体对象</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] MessageUpInput input)
    {
        var entity = input.Adapt<MessageEntity>();
        var isOk = await _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 发布公告.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpPut("{id}/Actions/Release")]
    public async Task Release(string id)
    {
        var entity = await _repository.FirstOrDefaultAsync(x => x.Id == id && x.DeleteMark == null);
        if (entity != null)
        {
            // 发送
            await SentNotice(entity);
        }
    }

    /// <summary>
    /// 全部已读.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Actions/ReadAll")]
    public async Task AllRead()
    {
        await MessageRead(string.Empty);
    }

    /// <summary>
    /// 删除记录.
    /// </summary>
    /// <param name="postParam">请求参数.</param>
    /// <returns></returns>
    [HttpDelete("Record")]
    public async Task DeleteRecord_Api([FromBody] dynamic postParam)
    {
        string[] ids = postParam.ids.ToString().Split(',');
        var isOk = await _repository.Context.Deleteable<MessageReceiveEntity>().Where(m => m.UserId == _userManager.UserId && ids.Contains(m.MessageId)).ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }
    #endregion

    #region PublicMethod

    /// <summary>
    /// 创建.
    /// </summary>
    /// <param name="entity">实体对象.</param>
    /// <param name="receiveEntityList">收件用户.</param>
    [NonAction]
    private int Create(MessageEntity entity, List<MessageReceiveEntity> receiveEntityList)
    {
        try
        {
            _repository.Context.BeginTran();

            _repository.Context.Insertable<MessageReceiveEntity>(receiveEntityList).ExecuteCommand();
            var total = _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteCommand();

            _repository.Context.CommitTran();
            return total;
        }
        catch (Exception)
        {
            _repository.Context.RollbackTran();
            return 0;
        }
    }

    /// <summary>
    /// 更新.
    /// </summary>
    /// <param name="entity">实体对象.</param>
    /// <param name="receiveEntityList">收件用户.</param>
    [NonAction]
    private int Update(MessageEntity entity, List<MessageReceiveEntity> receiveEntityList)
    {
        try
        {
            _repository.Context.BeginTran();

            _repository.Context.Insertable<MessageReceiveEntity>(receiveEntityList).ExecuteCommand();
            var total = _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommand();

            _repository.Context.CommitTran();
            return total;
        }
        catch (Exception)
        {
            _repository.Context.RollbackTran();
            return 0;
        }
    }

    /// <summary>
    /// 消息已读（全部）.
    /// </summary>
    /// <param name="id">id.</param>
    [NonAction]
    private async Task MessageRead(string id)
    {
        try
        {
            _repository.Context.BeginTran();

            if (id.IsNullOrEmpty())
            {
                await _repository.Context.Updateable<MessageReceiveEntity>().SetColumns(it => it.ReadCount == it.ReadCount + 1).SetColumns(x => new MessageReceiveEntity()
                {
                    IsRead = 1,
                    ReadTime = DateTime.Now
                }).Where(x => x.UserId == _userManager.UserId && x.IsRead == 0).ExecuteCommandAsync();
            }
            else
            {
                await _repository.Context.Updateable<MessageReceiveEntity>().SetColumns(it => it.ReadCount == it.ReadCount + 1).SetColumns(x => new MessageReceiveEntity()
                {
                    IsRead = 1,
                    ReadTime = DateTime.Now
                }).Where(x => x.UserId == _userManager.UserId && x.IsRead == 0 && x.MessageId == id).ExecuteCommandAsync();
            }

            _repository.Context.CommitTran();
        }
        catch (Exception)
        {
            _repository.Context.RollbackTran();
        }
    }

    /// <summary>
    /// 发送公告.
    /// </summary>
    /// <param name="entity">消息信息.</param>
    [NonAction]
    private async Task SentNotice(MessageEntity entity)
    {
        try
        {
            var toUserIds = new List<string>();
            entity.EnabledMark = 1;
            if (entity.ToUserIds.IsNullOrEmpty())
                toUserIds = (await _usersService.GetList()).Select(x => x.Id).ToList();
            else
                toUserIds = entity.ToUserIds.Split(",").ToList();
            List<MessageReceiveEntity> receiveEntityList = toUserIds
                .Select(x => new MessageReceiveEntity()
                    {
                        Id = SnowflakeIdHelper.NextId(),
                        MessageId = entity.Id,
                        UserId = x,
                        IsRead = 0,
                        BodyText = entity.BodyText,
                    }).ToList();

            Update(entity, receiveEntityList);
            if (entity.ToUserIds.IsNullOrEmpty())
            {
                await _imHandler.SendMessageToTenantAsync(_userManager.TenantId, new { method = "messagePush", messageType = 1, userId = _userManager.UserId, toUserId = toUserIds, title = entity.Title, unreadNoticeCount = 1 }.ToJsonString());
            }
            else
            {
                foreach (var item in toUserIds)
                {
                    // 消息推送 - 指定用户
                    await _imHandler.SendMessageToUserAsync(string.Format("{0}-{1}", _userManager.TenantId, item), new { method = "messagePush", messageType = 2, userId = _userManager.UserId, toUserId = toUserIds, title = entity.Title, unreadNoticeCount = 1 }.ToJsonString());

                }
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ErrorCode.D7003);
        }
    }

    /// <summary>
    /// 发送站内消息.
    /// </summary>
    /// <param name="toUserIds">发送用户.</param>
    /// <param name="title">标题.</param>
    /// <param name="bodyText">内容.</param>
    [NonAction]
    public async Task SentMessage(List<string> toUserIds, string title, string bodyText = null, Dictionary<string, object> bodyDic = null)
    {
        try
        {
            MessageEntity entity = new MessageEntity();
            entity.Id = SnowflakeIdHelper.NextId();
            entity.Title = title;
            entity.BodyText = bodyText;
            entity.Type = 2;
            entity.LastModifyTime = DateTime.Now;
            entity.LastModifyUserId = _userManager.UserId;
            List<MessageReceiveEntity> receiveEntityList = toUserIds
                .Select(x => new MessageReceiveEntity()
                {
                    Id = SnowflakeIdHelper.NextId(),
                    MessageId = entity.Id,
                    UserId = x,
                    IsRead = 0,
                    BodyText = bodyDic.IsNotEmptyOrNull() && bodyDic.ContainsKey(x) ? bodyDic[x].ToJsonString() : null,
                }).ToList();

            if (Create(entity, receiveEntityList) >= 1)
            {
                foreach (var item in toUserIds)
                {
                    //消息推送 - 指定用户
                    await _imHandler.SendMessageToUserAsync(string.Format("{0}-{1}", _userManager.TenantId, item), new { method = "messagePush", messageType = 2, userId = _userManager.UserId, toUserId = toUserIds, title = entity.Title, unreadNoticeCount = 1 }.ToJsonString());
                }
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    #endregion
}