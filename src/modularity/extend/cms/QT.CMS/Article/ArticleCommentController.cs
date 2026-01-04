using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Article;
using QT.CMS.Entitys.Dto.Parameter;
using QT.Common.Const;
using QT.Common.Core.Filter;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JsonSerialization;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 文章评论
/// </summary>
//[Route("admin/article/comment")]
//[ApiController]
[ApiDescriptionSettings(ModuleConst.CMS, Tag = "admin", Name = "comment", Order = 200)]
[Route("api/cms/admin/article/[controller]")]
public class ArticleCommentController : IDynamicApiController
{
    private readonly ISqlSugarRepository<ArticleComment> _commentService;
    private readonly IUserService _userService;
    //private readonly IMapper _mapper;
    //private readonly IArticleCommentLikeService _likeService;
    //private readonly IArticleService _articleService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ArticleCommentController(ISqlSugarRepository<ArticleComment> commentService, IUserService userService/*, IArticleCommentLikeService likeService, IMapper mapper, IArticleService articleService*/)
    {
        _commentService = commentService;
        _userService = userService;
        //_likeService = likeService;
        //_mapper = mapper;
        //_articleService = articleService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/article/comment/1/1
    /// </summary>
    [HttpGet("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("ArticleComment", ActionType.View, "channelId")]
    public async Task<dynamic> GetById([FromRoute] int id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ArticleCommentDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _commentService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ArticleCommentDto>().ShapeData(param.Fields);
        return (result);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/article/comment/1?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("ArticleComment", ActionType.View, "channelId")]
    public async Task<dynamic> GetList([FromRoute] int channelId, [FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticleCommentDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticleCommentDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _commentService.AsQueryable()
            .Where(x => x.ChannelId == channelId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Content.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "AddTime,-Id")
            .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);
        //var list = await _commentService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.ChannelId == channelId
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Content != null && x.Content.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "AddTime,-Id");

        //x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.pagination.Total,
        //    pageSize = list.pagination.PageSize,
        //    pageIndex = list.pagination.PageIndex,
        //    totalPages = 0
        //};
        //App.HttpContext.Response.Headers.Add("x-pagination", JSON.Serialize(paginationMetadata));

        //映射成DTO，根据字段进行塑形
        //var resultDto = list.Adapt<IEnumerable<ArticleCommentDto>>().ShapeData(searchParam.Fields);
        return (PageResult<ArticleCommentDto>.SqlSugarPageResult(list));
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/ArticleComment/1/1
    /// </summary>
    [HttpPut("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("ArticleComment", ActionType.Edit, "channelId")]
    public async Task Update([FromRoute] long id, [FromBody] ArticleCommentAddDto modelDto)
    {
        //查找记录
        var model = await _commentService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _commentService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelDto.Adapt(model);
        var result = await _commentService.UpdateAsync(model);
    }

    /// <summary>
    /// 批量审核
    /// 示例：/ArticleComment/1?ids=1,2,3
    /// </summary>
    [HttpPut("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("ArticleComment", ActionType.Audit, "channelId")]
    public async Task Updates([FromRoute] int channelId, [FromQuery] string ids)
    {
        if (ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var idList = ids.ToIEnumerable<long>();
        if (idList == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //查找记录
        var list = await _commentService.Where(x => x.ChannelId == channelId && idList.Contains(x.Id)).ToListAsync();
        //var list = await _commentService.QueryListAsync<ArticleComment>(0, x => x.ChannelId == channelId && idList.Contains(x.Id));
        if (list == null)
        {
            throw Oops.Oh("没有要审核的评论");
        }
        _commentService.Context.Tracking(list);
        foreach (var item in list)
        {
            item.Status = 0;
        }
        var result = await _commentService.UpdateAsync(list);
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/article/comment/1/1
    /// </summary>
    [HttpDelete("{channelId}/{id}")]
    //[Authorize]
    //[AuthorizeFilter("ArticleComment", ActionType.Delete, "channelId")]
    public async Task Delete([FromRoute] int id, [FromRoute] int channelId)
    {
        if (!await _commentService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        var result = await _commentService.DeleteAsync(x => x.Id == id && x.ChannelId == channelId);
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/article/comment/1?ids=1,2,3
    /// </summary>
    [HttpDelete("{channelId}")]
    //[Authorize]
    //[AuthorizeFilter("ArticleComment", ActionType.Delete, "channelId")]
    public async Task DeleteByIds([FromRoute] int channelId, [FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var idList = Ids.ToIEnumerable<long>();
        if (idList == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        await _commentService.DeleteAsync(x => idList.Contains(x.Id) && x.ChannelId == channelId);
    }

    /// <summary>
    /// 软删除一条记录
    /// 示例：/admin/article/comment/mark/1/1
    /// </summary>
    [HttpPut("mark/{channelId}/{id}")]
    //[Authorize]
    //[AuthorizeFilter("ArticleComment", ActionType.Delete, "channelId")]
    public async Task MarkDelete([FromRoute] int id, [FromRoute] int channelId)
    {
        if (!await _commentService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }

        var result = await _commentService.Context.Updateable<ArticleComment>()
            .SetColumns(x => new ArticleComment { IsDelete = 1 })
            .Where(x => x.Id == id && x.ChannelId == channelId)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 批量软删除记录(级联数据)
    /// 示例：/admin/article/comment/mark/1?ids=1,2,3
    /// </summary>
    [HttpPut("mark/{channelId}")]
    //[Authorize]
    //[AuthorizeFilter("ArticleComment", ActionType.Delete, "channelId")]
    public async Task MarkDeleteByIds([FromRoute] int channelId, [FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var idList = Ids.ToIEnumerable<long>();
        if (idList == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        var result = await _commentService.Context.Updateable<ArticleComment>()
           .SetColumns(x => new ArticleComment { IsDelete = 1 })
           .Where(x => idList.Contains(x.Id) && x.ChannelId == channelId)
           .ExecuteCommandAsync();
    }
    #endregion

    #region 普通账户调用接口========================
    /// <summary>
    /// 添加一条记录
    /// 示例：/account/article/comment/add
    /// </summary>
    [Authorize]
    [HttpPost("/account/article/comment/add")]
    public async Task<dynamic> Add([FromBody] ArticleCommentAddDto modelDto)
    {
        //获取文章实体
        var articleModel = await _commentService.Context.Queryable<Articles>().SingleAsync(t => t.Id == modelDto.ArticleId);
        if (articleModel == null)
        {
            throw Oops.Oh("文章不存在或已删除");
        }
        //判断是否关闭评论
        if (articleModel.IsComment == 0 || articleModel.SiteChannel?.IsComment == 0)
        {

            throw Oops.Oh("评论已经关闭！");
        }

        modelDto.UserIp = App.HttpContext.Connection.RemoteIpAddress?.ToString();
        //写入数据库
        var mapModel = await AddAsync(modelDto);

        //映射成DTO再返回，否则出错
        var result = mapModel.Adapt<ArticleCommentDto>();
        var model = new
        {
            Id = result.id,
            ParentId = result.parentId,
            RootId = result.rootId,
            UserName = result.userName,
            UserAvatar = result.userAvatar,
            AtUserName = result.atUserName,
            Content = result.content,
            LikeCount = result.likeCount,
            DateDescription = result.dateDescription,
            Children = result.children,
        };
        return (model);
    }

    /// <summary>
    /// 更新点赞量
    /// 示例：/account/article/comment/like/1
    /// </summary>
    [Authorize]
    [HttpPut("/account/article/comment/like/{commentId}")]
    public async Task<long> UpdateLike([FromRoute] long commentId)
    {
        //获得点赞数据实体
        var userId = await _userService.GetUserIdAsync();
        var likeModel = await _commentService.Context.Queryable<ArticleCommentLike>().SingleAsync(x => x.CommentId == commentId && x.UserId == userId);
        //获取评论旧数据
        var oldcommentModel = await _commentService.SingleAsync(t => t.Id == commentId);
        //用于局部更新
        //ArticleComment commentModel = new ArticleComment();
        if (oldcommentModel != null)
        {
            _commentService.Context.Tracking(oldcommentModel);
            //指明更新主键
            //commentModel.Id = oldcommentModel.Id;
            if (likeModel != null)//存在执行删除操作
            {
                await _commentService.Context.Deleteable<ArticleCommentLike>(likeModel).ExecuteCommandAsync();
                //点赞数
                //commentModel.LikeCount = oldcommentModel.LikeCount - 1;
                oldcommentModel.LikeCount--;
            }
            else//不存在执行添加操作
            {
                ArticleCommentLike like = new()
                {
                    CommentId = commentId,
                    AddTime = DateTime.Now,
                    UserId = await  _userService.GetUserIdAsync()
                };
                await _commentService.Context.Insertable<ArticleCommentLike>(like).ExecuteCommandAsync();
                //点赞数
                oldcommentModel.LikeCount++;
                //commentModel.LikeCount = oldcommentModel.LikeCount + 1;
            }
            await _commentService.Context.Updateable<ArticleComment>(oldcommentModel).ExecuteCommandAsync();
            return oldcommentModel.LikeCount;
        }
        else
        {
            return 0;
        }
        //var entry = _context.Entry<ArticleComment>(commentModel);
        ////设置修改状态
        //entry.State = EntityState.Unchanged;
        //entry.Property(o => o.LikeCount).IsModified = true;
        ////提交保存
        //int res = await _context.SaveChangesAsync();
        //return commentModel.LikeCount;

    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取指定文章评价总数
    /// 示例：/client/article/comment/count/1
    /// </summary>
    [HttpGet("/client/article/comment/count/{articleId}")]
    [CachingFilter]
    [AllowAnonymous,NonUnify]
    public async Task<int> GetByArticleCount([FromRoute] long articleId, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticleCommentDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }

        //获取数据库列表
        var result = await _commentService.Where(x => x.Status == 0 && x.ArticleId == articleId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Content.Contains(searchParam.Keyword))
            .CountAsync();
        //var result = await _commentService.QueryCountAsync(
        //    x => x.Status == 0
        //    && x.ArticleId == articleId
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Content != null && x.Content.Contains(searchParam.Keyword))));
        //返回成功200
        return (result);
    }

    /// <summary>
    /// 获取指定文章评价列表
    /// 示例：/client/article/comment/view/1/10
    /// </summary>
    [HttpGet("/client/article/comment/view/{articleId}/{top}")]
    [CachingFilter]
    [AllowAnonymous, NonUnify]
    public async Task<dynamic> GetByArticleList([FromRoute] long articleId, [FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticleCommentDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticleCommentDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        //获取第一级的分页数据
        var qur = _commentService.AsQueryable() //.Includes(x => x!.Member)
            .Where(x => x.ParentId == 0)
            .Where(x => x.Status == 0 && x.ArticleId == articleId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Content.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "-Id");//调用Linq扩展类排序
        if (top > 0) qur = qur.Take(top);
        var parentList = await qur.ToListAsync();
        //所有主键
        List<long> ids = parentList.Select(t => t.Id).ToList();
        var list = parentList.Adapt<IEnumerable<ArticleCommentDto>>();
        //查询子集数据
        var childrenList = await _commentService.Context.Queryable<ArticleComment>().Where(x => ids.Contains(x.RootId))
            .Where(x => x.Status == 0 && x.ArticleId == articleId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Content.Contains(searchParam.Keyword))
            .ToListAsync();
        //是否有子集
        if (childrenList != null)
        {
            foreach (var item in list)
            {
                var child = childrenList.Where(t => t.RootId == item.id).ToList();
                item.children = child.Adapt<List<ArticleCommentDto>>();
            }
        }

        var resultFrom = list;

        //根据字段进行塑形
        var resultDto = resultFrom.ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 获取指定文章评价分页列表
    /// 示例：/client/article/comment/1?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/client/article/comment/{articleId}")]
    [CachingFilter]
    [AllowAnonymous, NonUnify]
    public async Task<dynamic> GetByArticlePageList([FromRoute] long articleId, [FromQuery] BaseParameter param, [FromQuery] PageParamater pageParam)
    {
        param.Fields = "id,parentId,rootId,userName,userAvatar,atUserName," +
            "content,likeCount,dateDescription,children";

        //检测参数是否合法
        if (param.OrderBy != null
            && !param.OrderBy.Replace("-", "").IsPropertyExists<ArticleCommentDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!param.Fields.IsPropertyExists<ArticleCommentDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        //获取第一级的分页数据
        var page = await _commentService.AsQueryable() //.Set<ArticleComment>().Include(x => x.User).ThenInclude(x => x!.Member)
            .Where(x => x.ParentId == 0)
            .Where(x => x.Status == 0 && x.ArticleId == articleId)
            .WhereIF(param.Keyword.IsNotEmptyOrNull(), x => x.Content.Contains(param.Keyword))
            .OrderBy(param.OrderBy ?? "-Id")
            .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);//调用Linq扩展类排序
                                                                       //var page = await PaginationList<ArticleComment>.CreateAsync(pageIndex, pageSize, result);
                                                                       //获取所有主键
        List<long> ids = page.list.Select(t => t.Id).ToList();
        //查询子集数据
        var childrenList = await _commentService.AsQueryable().Where(x => ids.Contains(x.RootId))
            .Where(x => x.Status == 0 && x.ArticleId == articleId)
            .WhereIF(param.Keyword.IsNotEmptyOrNull(), x => x.Content.Contains(param.Keyword))
            .OrderBy(param.OrderBy ?? "-Id")
            .ToListAsync();


        //将分页数据映射成DTO
        var resultDto = new SqlSugarPagedList<ArticleCommentDto>
        {
            pagination = page.pagination,
            list = page.list.Adapt<IEnumerable<ArticleCommentDto>>()
        }; 

        foreach (var item in resultDto.list)
        {
            var child = childrenList.Where(t => t.RootId == item.id).ToList();
            item.children = child.Adapt<List<ArticleCommentDto>>();
        }
        //var list = page;

        //x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = page.pagination.Total,
        //    pageSize = page.pagination.PageSize,
        //    pageIndex = page.pagination.PageIndex,
        //    totalPages = 0
        //};
        //App.HttpContext.Response.Headers.Add("x-pagination", JSON.Serialize(paginationMetadata));

        ////根据字段进行塑形
        //var resultDto = listDto.ShapeDatas(param.Fields);
        return PageResult<ArticleCommentDto>.SqlSugarPageResult(resultDto);
    }
    #endregion

    #region Private Methods
    private async Task<ArticleComment> AddAsync(ArticleCommentAddDto modelDto)
    {
        var model = modelDto.Adapt<ArticleComment>();
        //var user = await _userService.GetUserAsync();
        //if (user == null)
        //{
        //    throw new ResponseException("用户未登录或已超时");
        //}
        //user.Member = await _commentService.Context.Queryable<Members>().SingleAsync(t => t.UserId == user.Id);
        model.AddTime = DateTime.Now;
        model.UserId = await  _userService.GetUserIdAsync();
        model.UserName = await _userService.GetUserNameAsync();

        //检查文章是否存在
        var articleModel = await _commentService.Context.Queryable<Articles>().SingleAsync(t => t.Id == modelDto.ArticleId);
        if (articleModel == null) return null;
        model.ChannelId = articleModel.ChannelId;
        //存在父级
        var parentModel = await _commentService.SingleAsync(t => t.Id == model.ParentId);
        if (parentModel != null)
        {
            //继承父级的RootId
            model.RootId = parentModel.RootId == 0 ? parentModel.Id : parentModel.RootId;
            model.AtUserId = parentModel.UserId;
            model.AtUserName = parentModel.UserName;
        }
        else
        {
            model.RootId = 0;
            model.ParentId = 0;
        }
        //修改评论信息
        //await _context.Set<ArticleComment>().AddAsync(model);
        await _commentService.InsertAsync(model);
        //修改文章评论总数
        await _commentService.Context.Updateable<Articles>()
            .SetColumns(x => new Articles
            {
                CommentCount = x.CommentCount + 1
            })
            .Where(x => x.Id == articleModel.Id)
            .ExecuteCommandAsync();

        //model.User = user;
        return model;
    }
    #endregion
}
