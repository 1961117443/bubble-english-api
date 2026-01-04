using QT.Common.Extension;
using QT.Common.Security;
using QT.Logistics.Entitys;
using QT.Logistics.Entitys.Dto.LogArticle;
using QT.Systems.Entitys.Permission;
using QT.Systems.Interfaces.Permission;
using QT.Systems.Interfaces.System;

namespace QT.LogArticle;

/// <summary>
/// 业务实现：文章系统.
/// </summary>
[ApiDescriptionSettings(ModuleConst.Logistics, Tag = "文章系统管理", Name = "LogArticle", Order = 200)]
[Route("api/Logistics/[controller]")]
public class LogArticleService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<LogArticleEntity> _repository;


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
    /// 初始化一个<see cref="LogArticleService"/>类型的新实例.
    /// </summary>
    public LogArticleService(
        ISqlSugarRepository<LogArticleEntity> LogArticleRepository,
        IUsersService usersService,
        IUserManager userManager,
        ISqlSugarClient context)
    {
        _repository = LogArticleRepository;
        _usersService = usersService;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    #region Get

    /// <summary>
    /// 列表（通知公告）.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("Notice")]
    public async Task<dynamic> GetNoticeList([FromQuery] LogArticleNoticeInput input)
    {
        var list = await _repository.Context.Queryable<LogArticleEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, b.Id == a.CreatorUserId))
            .WhereIF(input.type.HasValue, a => a.Type == input.type && a.DeleteMark == null)
            .WhereIF(!string.IsNullOrEmpty(input.keyword), a => a.Title.Contains(input.keyword))
            .OrderBy(a => a.EnabledMark).OrderBy(a => a.CreatorTime, OrderByType.Desc)
            .OrderBy(a => a.LastModifyTime, OrderByType.Desc)
            .Select((a, b) => new LogArticleNoticeOutput
            {
                id = a.Id,
                lastModifyTime = a.EnabledMark == 1 ? a.LastModifyTime : null,
                enabledMark = a.EnabledMark,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                title = a.Title,
                type = a.Type,
                creatorTime = a.CreatorTime,
                uniqueKey = a.UniqueKey
            }).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<LogArticleNoticeOutput>.SqlSugarPageResult(list); 
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="code">编码.</param>
    /// <returns></returns>
    [HttpGet("getId/{code}")]
    public async Task<string> GetId_Code(int code)
    {
        var entity = await _repository.Where(it => it.Type == code).FirstAsync();
        if (entity!=null)
        {
            return entity.Id;
        }
        return string.Empty;
    }

    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo_Api(string id)
    {
        return await _repository.Context.Queryable<LogArticleEntity, UserEntity>((a, b) => new JoinQueryInfos(JoinType.Left, a.CreatorUserId == b.Id))
            .Where(a => a.Id == id && a.DeleteMark == null)
            .Select((a, b) => new LogArticleInfoOutput
            {
                id = a.Id,
                lastModifyTime = a.LastModifyTime,
                creatorUser = SqlFunc.MergeString(b.RealName, "/", b.Account),
                title = a.Title,
                bodyText = a.BodyText,
                files = a.Files,
                toUserIds = a.ToUserIds,
                type = a.Type,
                uniqueKey = a.UniqueKey
            }).FirstAsync();
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
            _db.BeginTran();

            //await _repository.Context.Deleteable<LogArticleReceiveEntity>().Where(x => x.LogArticleId == id).ExecuteCommandAsync();
            await _repository.Context.Updateable<LogArticleEntity>().SetColumns(it => new LogArticleEntity()
            {
                DeleteMark = 1,
                DeleteUserId = _userManager.UserId,
                DeleteTime = SqlFunc.GetDate()
            }).Where(it => it.Id.Equals(id)).ExecuteCommandAsync();

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.COM1002);
        }
    }

    /// <summary>
    /// 新建.
    /// </summary>
    /// <param name="input">实体对象.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] LogArticleCrInput input)
    {
        var entity = input.Adapt<LogArticleEntity>();


        // type < 2000 属于内置的固定文章，不允许重复
        if (!input.type.HasValue)
        {
            throw Oops.Oh("文章类型不能为空！");
        }
        if (input.type < 2000)
        {
            if (await _repository.AnyAsync(it => it.Type == input.type))
            {
                throw Oops.Oh($"【{input.type}】类型已存在，不允许重复添加！");
            }
        }

        if (!string.IsNullOrEmpty(input.uniqueKey))
        {
            if (await _repository.AnyAsync(it => it.UniqueKey == input.uniqueKey))
            {
                throw Oops.Oh($"唯一标识已存在，不允许重复添加！");
            }
        }


        //entity.Type = 1;
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
    public async Task Update(string id, [FromBody] LogArticleUpInput input)
    {
        var entity = input.Adapt<LogArticleEntity>();
        if (!input.type.HasValue)
        {
            throw Oops.Oh("文章类型不能为空！");
        }
        if (input.type < 2000)
        {
            if (await _repository.AnyAsync(it => it.Type == input.type && it.Id!=entity.Id))
            {
                throw Oops.Oh($"【{input.type}】类型已存在，不允许重复添加！");
            }
        }

        if (!string.IsNullOrEmpty(input.uniqueKey))
        {
            if (await _repository.AnyAsync(it => it.UniqueKey == input.uniqueKey && it.Id != entity.Id))
            {
                throw Oops.Oh($"唯一标识已存在，不允许重复添加！");
            }
        }

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
            _repository.Context.Tracking(entity);
            // 更新状态
            entity.EnabledMark = 1;
            entity.LastModify();

            await _repository.Context.Updateable<LogArticleEntity>(entity).ExecuteCommandAsync();
            // 发送
            //await SentNotice(entity);
        }
    }

    ///// <summary>
    ///// 全部已读.
    ///// </summary>
    ///// <returns></returns>
    //[HttpPost("Actions/ReadAll")]
    //public async Task AllRead()
    //{
    //    await LogArticleRead(string.Empty);
    //}

    ///// <summary>
    ///// 删除记录.
    ///// </summary>
    ///// <param name="postParam">请求参数.</param>
    ///// <returns></returns>
    //[HttpDelete("Record")]
    //public async Task DeleteRecord_Api([FromBody] dynamic postParam)
    //{
    //    string[] ids = postParam.ids.ToString().Split(',');
    //    var isOk = await _repository.Context.Deleteable<LogArticleReceiveEntity>().Where(m => m.UserId == _userManager.UserId && ids.Contains(m.LogArticleId)).ExecuteCommandHasChangeAsync();
    //    if (!isOk)
    //        throw Oops.Oh(ErrorCode.COM1002);
    //}
    #endregion

    #region PublicMethod

    ///// <summary>
    ///// 创建.
    ///// </summary>
    ///// <param name="entity">实体对象.</param>
    ///// <param name="receiveEntityList">收件用户.</param>
    //[NonAction]
    //private int Create(LogArticleEntity entity, List<LogArticleReceiveEntity> receiveEntityList)
    //{
    //    try
    //    {
    //        _db.BeginTran();

    //        _repository.Context.Insertable(receiveEntityList).ExecuteCommandAsync();
    //        var total = _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).CallEntityMethod(m => m.Create()).ExecuteCommand();

    //        _db.CommitTran();
    //        return total;
    //    }
    //    catch (Exception)
    //    {
    //        _db.RollbackTran();
    //        return 0;
    //    }
    //}

    ///// <summary>
    ///// 更新.
    ///// </summary>
    ///// <param name="entity">实体对象.</param>
    ///// <param name="receiveEntityList">收件用户.</param>
    //[NonAction]
    //private int Update(LogArticleEntity entity, List<LogArticleReceiveEntity> receiveEntityList)
    //{
    //    try
    //    {
    //        _db.BeginTran();

    //        _repository.Context.Insertable(receiveEntityList).ExecuteCommandAsync();
    //        var total = _repository.Context.Updateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommand();

    //        _db.CommitTran();
    //        return total;
    //    }
    //    catch (Exception)
    //    {
    //        _db.RollbackTran();
    //        return 0;
    //    }
    //}

    ///// <summary>
    ///// 消息已读（全部）.
    ///// </summary>
    ///// <param name="id">id.</param>
    //[NonAction]
    //private async Task LogArticleRead(string id)
    //{
    //    try
    //    {
    //        _db.BeginTran();

    //        if (id.IsNullOrEmpty())
    //        {
    //            await _repository.Context.Updateable<LogArticleReceiveEntity>().SetColumns(it => it.ReadCount == it.ReadCount + 1).SetColumns(x => new LogArticleReceiveEntity()
    //            {
    //                IsRead = 1,
    //                ReadTime = DateTime.Now
    //            }).Where(x => x.UserId == _userManager.UserId && x.IsRead == 0).ExecuteCommandAsync();
    //        }
    //        else
    //        {
    //            await _repository.Context.Updateable<LogArticleReceiveEntity>().SetColumns(it => it.ReadCount == it.ReadCount + 1).SetColumns(x => new LogArticleReceiveEntity()
    //            {
    //                IsRead = 1,
    //                ReadTime = DateTime.Now
    //            }).Where(x => x.UserId == _userManager.UserId && x.IsRead == 0 && x.LogArticleId == id).ExecuteCommandAsync();
    //        }

    //        _db.CommitTran();
    //    }
    //    catch (Exception)
    //    {
    //        _db.RollbackTran();
    //    }
    //}

    ///// <summary>
    ///// 发送公告.
    ///// </summary>
    ///// <param name="entity">消息信息.</param>
    //[NonAction]
    //private async Task SentNotice(LogArticleEntity entity)
    //{
    //    try
    //    {
    //        var toUserIds = new List<string>();
    //        entity.EnabledMark = 1;
    //        if (entity.ToUserIds.IsNullOrEmpty())
    //            toUserIds = (await _usersService.GetList()).Select(x => x.Id).ToList();
    //        else
    //            toUserIds = entity.ToUserIds.Split(",").ToList();
    //        List<LogArticleReceiveEntity> receiveEntityList = toUserIds
    //            .Select(x => new LogArticleReceiveEntity()
    //                {
    //                    Id = SnowflakeIdHelper.NextId(),
    //                    LogArticleId = entity.Id,
    //                    UserId = x,
    //                    IsRead = 0,
    //                    BodyText = entity.BodyText,
    //                }).ToList();

    //        Update(entity, receiveEntityList);
    //        if (entity.ToUserIds.IsNullOrEmpty())
    //        {
    //            await _imHandler.SendLogArticleToTenantAsync(_userManager.TenantId, new { method = "LogArticlePush", LogArticleType = 1, userId = _userManager.UserId, toUserId = toUserIds, title = entity.Title, unreadNoticeCount = 1 }.ToJsonString());
    //        }
    //        else
    //        {
    //            foreach (var item in toUserIds)
    //            {
    //                // 消息推送 - 指定用户
    //                await _imHandler.SendLogArticleToUserAsync(string.Format("{0}-{1}", _userManager.TenantId, item), new { method = "LogArticlePush", LogArticleType = 2, userId = _userManager.UserId, toUserId = toUserIds, title = entity.Title, unreadNoticeCount = 1 }.ToJsonString());

    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        throw Oops.Oh(ErrorCode.D7003);
    //    }
    //}

    ///// <summary>
    ///// 发送站内消息.
    ///// </summary>
    ///// <param name="toUserIds">发送用户.</param>
    ///// <param name="title">标题.</param>
    ///// <param name="bodyText">内容.</param>
    //[NonAction]
    //public async Task SentLogArticle(List<string> toUserIds, string title, string bodyText = null, Dictionary<string, object> bodyDic = null)
    //{
    //    try
    //    {
    //        LogArticleEntity entity = new LogArticleEntity();
    //        entity.Id = SnowflakeIdHelper.NextId();
    //        entity.Title = title;
    //        entity.BodyText = bodyText;
    //        entity.Type = 2;
    //        entity.LastModifyTime = DateTime.Now;
    //        entity.LastModifyUserId = _userManager.UserId;
    //        List<LogArticleReceiveEntity> receiveEntityList = toUserIds
    //            .Select(x => new LogArticleReceiveEntity()
    //            {
    //                Id = SnowflakeIdHelper.NextId(),
    //                LogArticleId = entity.Id,
    //                UserId = x,
    //                IsRead = 0,
    //                BodyText = bodyDic.IsNotEmptyOrNull() && bodyDic.ContainsKey(x) ? bodyDic[x].ToJsonString() : null,
    //            }).ToList();

    //        if (Create(entity, receiveEntityList) >= 1)
    //        {
    //            foreach (var item in toUserIds)
    //            {
    //                //消息推送 - 指定用户
    //                await _imHandler.SendLogArticleToUserAsync(string.Format("{0}-{1}", _userManager.TenantId, item), new { method = "LogArticlePush", LogArticleType = 2, userId = _userManager.UserId, toUserId = toUserIds, title = entity.Title, unreadNoticeCount = 1 }.ToJsonString());
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        throw;
    //    }
    //}

    #endregion
}