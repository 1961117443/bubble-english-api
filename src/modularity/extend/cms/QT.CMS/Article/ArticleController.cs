using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
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
/// 文章接口
/// </summary>
[Route("api/cms/admin/article")]
[ApiController]
[ApiDescriptionSettings(ModuleConst.CMS, Tag = "文章接口", Name = "article", Order = 200)]
public class ArticleController : ControllerBase, IDynamicApiController
{
    private readonly ISqlSugarRepository<Articles> _repository;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ArticleController(ISqlSugarRepository<Articles> repository, IUserService userService)
    {
        _repository = repository;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/article/1/1
    /// </summary>
    [HttpGet("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("Article", ActionType.View, "channelId")]
    public async Task<dynamic> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ArticlesDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _repository.AsQueryable()
                .Includes(x => x.ArticleAlbums)
                .Includes(x => x.ArticleAttachs)
                .Includes(x => x.ArticleFields)
                .Includes(x => x.CategoryRelations, t => t.Category)
                .Includes(x => x.LabelRelations)
                .SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ArticlesDto>().ShapeData(param.Fields);
        return (result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/article/view/1/10
    /// </summary>
    [HttpGet("view/{channelId}/{top}")]
    [Authorize]
    //[AuthorizeFilter("Article", ActionType.View, "channelId")]
    public async Task<dynamic> GetList([FromRoute] int channelId, [FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticlesDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticlesDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _repository.AsQueryable()
            .Where(x => x.ChannelId == channelId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .AutoTake(top)
            .ToListAsync();

        //var resultFrom = await _articleService.QueryListAsync<Articles>(top,
        //    x => x.ChannelId == channelId
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ArticlesDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/article/1?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("Article", ActionType.View, "channelId")]
    public async Task<dynamic> GetList([FromQuery] ArticleParameter searchParam, [FromQuery] PageParamater pageParam, [FromRoute] int channelId)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticlesDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticlesDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _repository.AsQueryable()
            .Includes(x=>x.CategoryRelations, cr=>cr.Category)
            .Where(x => x.ChannelId == channelId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .WhereIF(searchParam.LabelId > 0, x => SqlFunc.Subqueryable<ArticleLabelRelation>().Where(d => d.LabelId == searchParam.LabelId).Any())
            .WhereIF(searchParam.StartDate.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartDate.Value))
            .WhereIF(searchParam.EndDate.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndDate.Value))
            .WhereIF(searchParam.Status.HasValue, x=>x.Status == searchParam.Status)
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        //var list = await _articleService.QueryPageAsync(pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.ChannelId == channelId
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)))
        //    && (searchParam.CategoryId <= 0 || x.CategoryRelations.Any(x => x.CategoryId == searchParam.CategoryId))
        //    && (searchParam.LabelId <= 0 || x.LabelRelations.Any(x => x.LabelId == searchParam.LabelId))
        //    && (searchParam.StartDate == null || DateTime.Compare(x.AddTime, searchParam.StartDate.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndDate == null || DateTime.Compare(x.AddTime, searchParam.EndDate.GetValueOrDefault()) <= 0),
        //    searchParam.OrderBy ?? "SortId,-Id");

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
        //var resultDto = list.Adapt<IEnumerable<ArticlesDto>>().ShapeData(searchParam.Fields);
        return PageResult<ArticlesDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/article/1
    /// </summary>
    [HttpPost("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("Article", ActionType.Add, "channelId")]
    public async Task<ArticlesDto> Add([FromBody] ArticlesAddDto modelDto)
    {
        ////验证数据是否合法
        //if (!TryValidateModel(modelDto))
        //{
        //    return ValidationProblem(ModelState);
        //}
        //检查频道是否存在
        var channelModel = await _repository.Context.Queryable<SiteChannel>().SingleAsync(x => x.Id == modelDto.channelId);
        if (channelModel == null)
        {
            throw Oops.Oh("频道不存在或已删除");
        }

        ////获取当前用户信息
        //var userInfo = await _userService.GetUserAsync();
        //if (userInfo == null)
        //{
        //    return BadRequest(ResponseMessage.Error($"用户未登录或已超时"));
        //}
        //var manageInfo = await _repository.Context.Queryable<Manager>(x => x.UserId == userInfo.Id);
        //if (manageInfo == null)
        //{
        //    return BadRequest(ResponseMessage.Error($"管理员身份有误，请核实后操作"));
        //}

        if (!await _userService.IsSuperAdminAsync())
        {
            throw Oops.Oh($"管理员身份有误，请核实后操作");
        }

        modelDto.siteId = channelModel.SiteId;
        //if (manageInfo.IsAudit > 0)
        {
            modelDto.status = 1;
        }
        modelDto.AddBy = await _userService.GetUserNameAsync();
        modelDto.AddTime = DateTime.Now;
        //映射成实体
        var model = modelDto.Adapt<Articles>();
        //内容摘要提取内容前255个字符
        if (string.IsNullOrWhiteSpace(model.Zhaiyao) && !string.IsNullOrWhiteSpace(model.Content))
        {
            model.Zhaiyao = HtmlHelper.CutString(model.Content, 250);
        }
        //写入数据库
        await _repository.Context.InsertNav(model)
                .Include(x => x.ArticleAlbums)
                .Include(x => x.ArticleAttachs)
                .Include(x => x.ArticleFields)
                .Include(x => x.CategoryRelations)
                .Include(x => x.LabelRelations)
                .ExecuteCommandAsync();
        //映射成DTO再返回，否则出错
        var result = model.Adapt<ArticlesDto>();
        return (result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/article/1/15
    /// </summary>
    [HttpPut("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("Article", ActionType.Edit, "channelId")]
    public async Task Update([FromRoute] long id, [FromBody] ArticlesEditDto modelDto)
    {
        ////验证数据是否合法
        //if (!TryValidateModel(modelDto))
        //{
        //    return ValidationProblem(ModelState);
        //}
        //内容摘要提取内容前255个字符
        if (string.IsNullOrWhiteSpace(modelDto.zhaiyao) && !string.IsNullOrWhiteSpace(modelDto.content))
        {
            modelDto.zhaiyao = HtmlHelper.CutString(modelDto.content, 250);
        }
        //获取当前用户信息
        //var userInfo = await _userService.GetUserAsync();
        //if (userInfo == null)
        //{
        //    return BadRequest(ResponseMessage.Error($"用户未登录或已超时"));
        //}
        var userId = await _userService.GetUserIdAsync();
        var manageInfo = await _repository.Context.Queryable<Manager>().SingleAsync(x => x.UserId == userId);
        if (manageInfo == null)
        {
            throw Oops.Oh("管理员身份有误，请核实后操作");
        }
        if (manageInfo.IsAudit > 0)
        {
            modelDto.status = 1;
        }
        var model = await _repository.AsQueryable()
                .Includes(x => x.ArticleAlbums)
                .Includes(x => x.ArticleAttachs)
                .Includes(x => x.ArticleFields)
                .Includes(x => x.CategoryRelations)
                .Includes(x => x.LabelRelations)
                .SingleAsync(x => x.Id == id);
        _repository.Context.Tracking(model);
        modelDto.Adapt(model);
        model.UpdateBy = await _userService.GetUserNameAsync();
        model.UpdateTime = DateTime.Now;
        await _repository.Context.UpdateNav(model)
                .Include(x => x.ArticleAlbums)
                .Include(x => x.ArticleAttachs)
                .Include(x => x.ArticleFields)
                .Include(x => x.CategoryRelations)
                .Include(x => x.LabelRelations)
                .ExecuteCommandAsync();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/article/1/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("Article", ActionType.Edit, "channelId")]
    public async Task Update([FromRoute] long id, [FromBody] JsonPatchDocument<ArticlesEditDto> patchDocument)
    {
        //注意：要使用写的数据库进行查询，才能正确写入数据主库
        var model = await _repository.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        _repository.Context.Tracking(model);

        var modelToPatch = model.Adapt<ArticlesEditDto>();
        patchDocument.ApplyTo(modelToPatch);
        ////验证数据是否合法
        //if (!TryValidateModel(modelToPatch))
        //{
        //    return ValidationProblem(ModelState);
        //}
        _repository.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _repository.UpdateAsync(model);
    }

    /// <summary>
    /// 批量审核记录
    /// 示例：/admin/article/1?ids=1,2,3
    /// </summary>
    [HttpPut("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("Article", ActionType.Audit, "channelId")]
    public async Task AuditByIds([FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var arrIds = Ids.ToIEnumerable<long>();
        if (arrIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //找出符合条件的记录
        var list = await _repository.AsQueryable()
            .Where(x => x.Status == 1 && arrIds.Contains(x.Id))
            .Take(1000)
            .ToListAsync();
        //var list = await _articleService.QueryListAsync<Articles>(1000, x => x.Status == 1 && arrIds.Contains(x.Id), WriteRoRead.Write);
        if (list.Count() == 0)
        {
            throw Oops.Oh("没有找到需要审核的记录");
        }
        foreach (var item in list)
        {
            _repository.Context.Tracking(item);
            item.Status = 0;
        }
        //保存到数据库
        await _repository.UpdateAsync(list);
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/article/1/1
    /// </summary>
    [HttpDelete("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("Article", ActionType.Delete, "channelId")]
    public async Task Delete([FromRoute] long id)
    {
        if (!await _repository.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _repository.DeleteAsync(x => x.Id == id);
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/article/1?ids=1,2,3
    /// </summary>
    [HttpDelete("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("Article", ActionType.Delete, "channelId")]
    public async Task DeleteByIds([FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var arrIds = Ids.ToIEnumerable<long>();
        if (arrIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        await _repository.DeleteAsync(x => arrIds.Contains(x.Id));
    }

    /// <summary>
    /// 获取总记录数量
    /// 示例：/admin/article/view/count
    /// </summary>
    [HttpGet("view/count")]
    [Authorize]
    public async Task<int> GetCount([FromQuery] ArticleParameter searchParam)
    {
        var result = await _repository.CountAsync(
            x => (searchParam.Status <= 0 || x.Status == searchParam.Status));
        //返回成功200
        return (result);
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 根据文章ID获取同类别下的其它列表
    /// 示例：/client/article/view/1/10
    /// </summary>
    [HttpGet("/client/article/view/{id}/{top}")]
    [CachingFilter(60, true)]
    [AllowAnonymous,NonUnify]
    public async Task<dynamic> ClientGetList([FromRoute] int id, [FromRoute] int top, [FromQuery] ArticleParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticlesClientDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticlesClientDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取文章实体
        var model = await _repository.AsQueryable()
                .Includes(x => x.ArticleAlbums)
                .Includes(x => x.ArticleAttachs)
                .Includes(x => x.ArticleFields)
                .Includes(x => x.CategoryRelations, t => t.Category)
                .Includes(x => x.LabelRelations)
                .SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //检查用户是否登录筛选数据
        var userId = await  _userService.GetUserIdAsync();
        int groupId = 0;
        if (userId.IsNotEmptyOrNull())
        {
            var memberModel = await _repository.Context.Queryable<Members>().SingleAsync(x => x.UserId == userId);
            if (memberModel != null)
            {
                groupId = memberModel.GroupId;
            }
        }
        List<long> categorys = new();
        categorys = model.CategoryRelations.Select(a => a.CategoryId).ToList();
        if (top ==0)
        {
            top = 999;
        }
        //获取数据库列表
        var resultFrom = await _repository.AsQueryable()
                .Includes(x => x.ArticleAlbums)
                .Includes(x => x.ArticleAttachs)
                .Includes(x => x.ArticleFields)
                .Includes(x => x.CategoryRelations, t => t.Category)
                .Includes(x => x.LabelRelations)
                .Where(x => x.Id != id && x.CategoryRelations.Any(y=>categorys.Contains(y.CategoryId)))
                .WhereIF(groupId > 0, x => SqlFunc.IsNullOrEmpty(x.GroupIds) || x.GroupIds.Contains($",{groupId},"))
                //.WhereIF(searchParam.CategoryId > 0, x => SqlFunc.Subqueryable<ArticleCategoryRelation>().Where(d => d.ArticleId == x.Id && d.CategoryId == searchParam.CategoryId).Any())
                .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
                .WhereIF(searchParam.LabelId > 0, x => SqlFunc.Subqueryable<ArticleLabelRelation>().Where(d => d.ArticleId == x.Id && d.LabelId == searchParam.LabelId).Any())
                .WhereIF(searchParam.StartDate.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartDate.Value))
                .WhereIF(searchParam.EndDate.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndDate.Value))
                .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
                .AutoTake(top)
                .ToListAsync();

        //var resultFrom = await _articleService.QueryListAsync(top,
        //    x => (string.IsNullOrEmpty(x.GroupIds) || x.GroupIds.Contains($",{groupId},"))//检查是否有权限
        //    && x.CategoryRelations.Any(x => categorys.Contains(x.CategoryId))//当前文章的分类
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)))//搜索关键字
        //    && (searchParam.LabelId <= 0 || x.LabelRelations.Any(t => t.LabelId == searchParam.LabelId))//标签搜索
        //    && (searchParam.StartDate == null || DateTime.Compare(x.AddTime, searchParam.StartDate.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndDate == null || DateTime.Compare(x.AddTime, searchParam.EndDate.GetValueOrDefault()) <= 0)
        //    && x.Id != id,
        //    searchParam.OrderBy ?? "SortId,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ArticlesClientDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 根据频道名称获取指定数量列表
    /// 示例：/client/article/view/channel/news/10?categoryId=1
    /// </summary>
    [HttpGet("/client/article/view/channel/{channelName}/{top}")]
    [CachingFilter(60, true)]
    [AllowAnonymous, NonUnify]
    public async Task<dynamic> ClientGetList([FromRoute] string channelName, [FromRoute] int top, [FromQuery] ArticleParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticlesClientDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticlesClientDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        if (string.IsNullOrWhiteSpace(channelName))
        {
            throw Oops.Oh("请传入频道名称");
        }
        //获得频道实体
        var channelModel = await _repository.Context.Queryable<SiteChannel>().SingleAsync(x => x.Name != null && x.Name.Equals(channelName) && x.SiteId == searchParam.SiteId);
        if (channelModel == null)
        {
            throw Oops.Oh("频道不存在或已删除");
        }
        //检查用户是否登录筛选数据
        var userId = await  _userService.GetUserIdAsync();
        long groupId = 0;
        if (userId.IsNotEmptyOrNull())
        {
            var memberModel = await _repository.Context.Queryable<Members>().SingleAsync(x => x.UserId == userId);
            if (memberModel != null)
            {
                groupId = memberModel.GroupId;
            }
        }

        if (searchParam.CategoryIndex.IsNotEmptyOrNull())
        {
            var categoryId = (int)await _repository.Context.Queryable<ArticleCategory>().Where(x => x.CallIndex == searchParam.CategoryIndex).Select(x => x.Id).FirstAsync();

            searchParam.CategoryId = categoryId == 0 ? 999999 : categoryId;
        }

        if (top == 0)
        {
            top = 999;
        }
        //获取数据库列表
        var resultFrom = await _repository.AsQueryable()
                .Includes(x => x.ArticleAlbums)
                .Includes(x => x.ArticleAttachs)
                .Includes(x => x.ArticleFields)
                .Includes(x => x.CategoryRelations, t => t.Category)
                .Includes(x => x.LabelRelations,l=>l.Label)
            .Where(x => x.Status == 0)
            .Where(x => x.ChannelId == channelModel.Id)
           .WhereIF(groupId > 0, x => SqlFunc.IsNullOrEmpty(x.GroupIds) ||  x.GroupIds.Contains($",{groupId},"))
           .WhereIF(searchParam.CategoryId > 0, x => SqlFunc.Subqueryable<ArticleCategoryRelation>().Where(d => d.ArticleId == x.Id && d.CategoryId == searchParam.CategoryId).Any())
           .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
           .WhereIF(searchParam.LabelId > 0, x => SqlFunc.Subqueryable<ArticleLabelRelation>().Where(d => d.ArticleId == x.Id && d.LabelId == searchParam.LabelId).Any())
           .WhereIF(searchParam.StartDate.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartDate.Value))
           .WhereIF(searchParam.EndDate.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndDate.Value))
           .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
           .AutoTake(top)
           .ToListAsync();

        //var resultFrom = await _articleService.QueryListAsync(top,
        //    x => x.ChannelId == channelModel.Id
        //    && (string.IsNullOrEmpty(x.GroupIds) || x.GroupIds.Contains($",{groupId},"))//检查是否有权限
        //    && (searchParam.CategoryId <= 0 || x.CategoryRelations.Any(t => t.CategoryId == searchParam.CategoryId))//查询分类
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)))//查询关键字
        //    && (searchParam.LabelId <= 0 || x.LabelRelations.Any(t => t.LabelId == searchParam.LabelId))//标签搜索
        //    && (searchParam.StartDate == null || DateTime.Compare(x.AddTime, searchParam.StartDate.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndDate == null || DateTime.Compare(x.AddTime, searchParam.EndDate.GetValueOrDefault()) <= 0),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ArticlesClientDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 根据频道ID获取指定数量列表
    /// 示例：/client/channel/article/view/1/10?categoryId=1
    /// </summary>
    [HttpGet("/client/channel/article/view/{channelId}/{top}")]
    [CachingFilter(60, true)]
    [AllowAnonymous, NonUnify]
    public async Task<dynamic> ClientGetListByChannelId([FromRoute] int channelId, [FromRoute] int top, [FromQuery] ArticleParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticlesClientDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticlesClientDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //检查用户是否登录筛选数据
        var userId = await  _userService.GetUserIdAsync();
        int groupId = 0;
        //if (userId > 0)
        //{
        //    var memberModel = await _articleService.QueryAsync<Members>(x => x.UserId == userId);
        //    if (memberModel != null)
        //    {
        //        groupId = memberModel.GroupId;
        //    }
        //}

        //获取数据库列表
        var resultFrom = await _repository.AsQueryable()
                .Includes(x => x.ArticleAlbums)
                .Includes(x => x.ArticleAttachs)
                .Includes(x => x.ArticleFields)
                .Includes(x => x.CategoryRelations, t => t.Category)
                .Includes(x => x.LabelRelations)
            .Where(x => x.Status == 0)
           .WhereIF(groupId > 0, x => SqlFunc.IsNullOrEmpty(x.GroupIds) || x.GroupIds.Contains($",{groupId},"))
           .WhereIF(searchParam.CategoryId > 0, x => SqlFunc.Subqueryable<ArticleCategoryRelation>().Where(d => d.ArticleId == x.Id && d.CategoryId == searchParam.CategoryId).Any())
           .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
           .WhereIF(searchParam.LabelId > 0, x => SqlFunc.Subqueryable<ArticleLabelRelation>().Where(d => d.ArticleId == x.Id && d.LabelId == searchParam.LabelId).Any())
           .WhereIF(searchParam.StartDate.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartDate.Value))
           .WhereIF(searchParam.EndDate.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndDate.Value))
           .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
           .AutoTake(top)
           .ToListAsync();

        //var resultFrom = await _articleService.QueryListAsync(top,
        //    x => (string.IsNullOrEmpty(x.GroupIds) || x.GroupIds.Contains($",{groupId},"))//检查是否有权限
        //    && (searchParam.CategoryId <= 0 || x.CategoryRelations.Any(t => t.CategoryId == searchParam.CategoryId))//查询分类
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)))//查询关键字
        //    && (searchParam.LabelId <= 0 || x.LabelRelations.Any(t => t.LabelId == searchParam.LabelId))//标签搜索
        //    && (searchParam.StartDate == null || DateTime.Compare(x.AddTime, searchParam.StartDate.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndDate == null || DateTime.Compare(x.AddTime, searchParam.EndDate.GetValueOrDefault()) <= 0)
        //    && x.ChannelId == channelId,//频道查询
        //    searchParam.OrderBy ?? "SortId,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ArticlesClientDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 根据频道名称获取分页列表
    /// 示例：/client/news/article?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/client/{channelName}/article")]
    [CachingFilter(60, true)]
    [AllowAnonymous, NonUnify]
    public async Task<dynamic> ClientGetList([FromRoute] string channelName, [FromQuery] ArticleParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticlesClientDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticlesClientDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获得频道实体
        var channelModel = await _repository.Context.Queryable<SiteChannel>().SingleAsync(x => x.Name != null && x.Name.Equals(channelName) && x.SiteId == searchParam.SiteId);
        if (channelModel == null)
        {
            throw Oops.Oh("频道不存在或已删除");
        }
        //检查用户是否登录筛选数据
        var userId = await  _userService.GetUserIdAsync();
        int groupId = 0;
        if (userId.IsNotEmptyOrNull())
        {
            var memberModel = await _repository.Context.Queryable<Members>().SingleAsync(x => x.UserId == userId);
            if (memberModel != null)
            {
                groupId = memberModel.GroupId;
            }
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _repository.AsQueryable()
                .Includes(x => x.ArticleAlbums)
                .Includes(x => x.ArticleAttachs)
                .Includes(x => x.ArticleFields)
                .Includes(x => x.CategoryRelations, t => t.Category)
                .Includes(x => x.LabelRelations)
                .Where(x=>x.Status == 0)
            .Where(x => x.ChannelId == channelModel.Id)
           .WhereIF(groupId > 0, x => SqlFunc.IsNullOrEmpty(x.GroupIds) || x.GroupIds.Contains($",{groupId},"))
           .WhereIF(searchParam.CategoryId > 0, x => SqlFunc.Subqueryable<ArticleCategoryRelation>().Where(d => d.ArticleId == x.Id && d.CategoryId == searchParam.CategoryId).Any())
           .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
           .WhereIF(searchParam.LabelId > 0, x => SqlFunc.Subqueryable<ArticleLabelRelation>().Where(d => d.ArticleId == x.Id && d.LabelId == searchParam.LabelId).Any())
           .WhereIF(searchParam.StartDate.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartDate.Value))
           .WhereIF(searchParam.EndDate.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndDate.Value))
           .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
           .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        //var list = await _articleService.QueryPageAsync(pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (string.IsNullOrEmpty(x.GroupIds) || x.GroupIds.Contains($",{groupId},"))//检查是否有权限
        //    && x.ChannelId == channelModel.Id
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)))
        //    && (searchParam.CategoryId <= 0 || x.CategoryRelations.Any(x => x.CategoryId == searchParam.CategoryId))
        //    && (searchParam.LabelId <= 0 || x.LabelRelations.Any(x => x.LabelId == searchParam.LabelId))
        //    && (searchParam.StartDate == null || DateTime.Compare(x.AddTime, searchParam.StartDate.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndDate == null || DateTime.Compare(x.AddTime, searchParam.EndDate.GetValueOrDefault()) <= 0),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.pagination.Total,
        //    pageSize = list.pagination.PageSize,
        //    pageIndex = list.pagination.PageIndex,
        //    totalPages = 0
        //};
        //App.HttpContext.Response.Headers.Add("x-pagination", JSON.Serialize(paginationMetadata));

        ////映射成DTO，根据字段进行塑形
        //var resultDto = list.Adapt<IEnumerable<ArticlesClientDto>>().ShapeData(searchParam.Fields);
        return (PageResult<ArticlesClientDto>.SqlSugarPageResult(list));
    }

    /// <summary>
    /// 根据频道ID获取文章分页列表
    /// 示例：/client/article/1?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/client/article/{channelId}")]
    [CachingFilter(60, true)]
    [AllowAnonymous, NonUnify]
    public async Task<dynamic> ClientGetList([FromRoute] int channelId, [FromQuery] ArticleParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticlesClientDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticlesClientDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //检查用户是否登录筛选数据
        var userId = await  _userService.GetUserIdAsync();
        int groupId = 0;
        //if (userId > 0)
        //{
        //    var memberModel = await _articleService.QueryAsync<Members>(x => x.UserId == userId);
        //    if (memberModel != null)
        //    {
        //        groupId = memberModel.GroupId;
        //    }
        //}

        //获取数据库列表
        var resultFrom = await _repository.AsQueryable()
                .Includes(x => x.ArticleAlbums)
                .Includes(x => x.ArticleAttachs)
                .Includes(x => x.ArticleFields)
                .Includes(x => x.CategoryRelations, t => t.Category)
                .Includes(x => x.LabelRelations)
           .Where(x => x.ChannelId == channelId)
          .WhereIF(groupId > 0, x => SqlFunc.IsNullOrEmpty(x.GroupIds) || x.GroupIds.Contains($",{groupId},"))
          .WhereIF(searchParam.CategoryId > 0, x => SqlFunc.Subqueryable<ArticleCategoryRelation>().Where(d => d.ArticleId == x.Id && d.CategoryId == searchParam.CategoryId).Any())
          .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
          .WhereIF(searchParam.LabelId > 0, x => SqlFunc.Subqueryable<ArticleLabelRelation>().Where(d => d.ArticleId == x.Id && d.LabelId == searchParam.LabelId).Any())
          .WhereIF(searchParam.StartDate.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartDate.Value))
          .WhereIF(searchParam.EndDate.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndDate.Value))
          .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
          .ToListAsync();

        //var resultFrom = await _articleService.QueryListAsync(0,
        //    x => (string.IsNullOrEmpty(x.GroupIds) || x.GroupIds.Contains($",{groupId},"))//检查是否有权限
        //    && x.ChannelId.Equals(channelId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)))
        //    && (searchParam.CategoryId <= 0 || x.CategoryRelations.Any(x => x.CategoryId == searchParam.CategoryId))
        //    && (searchParam.LabelId <= 0 || x.LabelRelations.Any(x => x.LabelId == searchParam.LabelId))
        //    && (searchParam.StartDate == null || DateTime.Compare(x.AddTime, searchParam.StartDate.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndDate == null || DateTime.Compare(x.AddTime, searchParam.EndDate.GetValueOrDefault()) <= 0),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ArticlesClientDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 获得每个分类下的文章前几条数据
    /// </summary>
    [HttpGet("/client/article/view/{channel}/{categoryTop}/{arcitleTop}")]
    [CachingFilter(60, true)]
    [AllowAnonymous, NonUnify]
    public async Task<List<ArticleCategoryClientDto>> GetList([FromRoute] string channel, [FromRoute] int categoryTop, [FromRoute] int arcitleTop, [FromQuery] BaseParameter searchParam)
    {
        //获得频道实体
        var channelModel = await _repository.Context.Queryable<SiteChannel>().SingleAsync(x => x.Name == channel && x.SiteId == searchParam.SiteId);
        if (channelModel == null)
        {
            throw Oops.Oh("频道不存在，请检查后重试。");
        }
        //检查用户是否登录筛选数据
        var userId = await  _userService.GetUserIdAsync();
        long groupId = 0;
        if (userId.IsNotEmptyOrNull())
        {
            var memberModel = await _repository.Context.Queryable<Members>().SingleAsync(x => x.UserId == userId);
            if (memberModel != null)
            {
                groupId = memberModel.GroupId;
            }
        }

        //获取前几条分类,最多获取前10个分类
        if (categoryTop > 10) categoryTop = 10;
        if (arcitleTop > 10) arcitleTop = 10;

        var categoryList = await _repository.Context.Queryable<ArticleCategory>()
             .Where(x => x.ChannelId == channelModel.Id && x.ParentId == 0 && x.Status == 0)
             .OrderBy(searchParam.OrderBy ?? "SortId,Id")
             .Take(categoryTop)
             .ToListAsync();

        var categoryIds = categoryList.Select(x => x.Id).ToArray();

        var articleCategoryRelations = await _repository.Context.Queryable<ArticleCategoryRelation>()
            .LeftJoin<Articles>((x,a)=>x.ArticleId == a.Id && a.Status == 0)
             .Where(x => categoryIds.Contains(x.CategoryId))
             .Where((x,a)=>a.ChannelId == channelModel.Id)
             .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), (x,a)=> a.Title.Contains(searchParam.Keyword))
             .Select((x,a) => new
             {
                 index2 = SqlFunc.RowNumber(a.SortId, x.CategoryId),
                 ArticleId = x.ArticleId,
                 CategoryId = x.CategoryId
             })
             .MergeTable()
             .Where(x => x.index2 <= arcitleTop)
             .OrderByDescending(x=>x.ArticleId)
             .Select(x => new ArticleCategoryRelation
             {
                 ArticleId = x.ArticleId,
                 CategoryId = x.CategoryId
             })
             .ToListAsync();

        var articleIds = articleCategoryRelations.Select(x => x.ArticleId).ToArray();

        var articles = await _repository.AsQueryable()
                .Includes(x => x.ArticleAlbums)
                .Includes(x => x.ArticleAttachs)
                .Includes(x => x.ArticleFields)
                .Includes(x => x.CategoryRelations, t => t.Category)
                .Includes(x => x.LabelRelations)
            //.Includes(x => x.ArticleAlbums)
            .Where(x=> articleIds.Contains(x.Id))
            .ToListAsync();

        //List<ArticleCategoryClientDto> result = categoryList.Select(item => new ArticleCategoryClientDto
        //{
        //    id = item.Id,
        //    title = item.Title,
        //    imgUrl = item.ImgUrl,
        //    data = articles.Where(x => articleCategoryRelations.Any(r => r.ArticleId == x.Id && r.CategoryId == item.Id))
        //                        .Select(x => new ArticlesClientDto
        //                        {
        //                            id = x.Id,
        //                            title = x.Title,
        //                            source = x.Source,
        //                            zhaiyao = x.Zhaiyao,
        //                            imgUrl = x.ImgUrl,
        //                            click = x.Click,
        //                            likeCount = x.LikeCount,
        //                            addTime = x.AddTime,
        //                            articleAlbums = x.ArticleAlbums.Select(ab => new ArticleAlbumDto
        //                            {
        //                                id = ab.Id,
        //                                articleId = ab.ArticleId,
        //                                thumbPath = ab.ThumbPath,
        //                                originalPath = ab.OriginalPath,
        //                                remark = ab.Remark,
        //                                addTime = ab.AddTime,
        //                                sortId = ab.SortId
        //                            })
        //                        })
        //}).ToList();

        List<ArticleCategoryClientDto> result = new List<ArticleCategoryClientDto>();

        foreach (var item in categoryList)
        {
            var dto = new ArticleCategoryClientDto
            {
                id = item.Id,
                title = item.Title,
                imgUrl = item.ImgUrl,
                data = articles.Where(x => articleCategoryRelations.Any(r => r.ArticleId == x.Id && r.CategoryId == item.Id))
                                .Select(x => new ArticlesClientDto
                                {
                                    id = x.Id,
                                    title = x.Title,
                                    source = x.Source,
                                    zhaiyao = x.Zhaiyao,
                                    imgUrl = x.ImgUrl,
                                    click = x.Click,
                                    likeCount = x.LikeCount,
                                    addTime = x.AddTime,
                                    articleAlbums = x.ArticleAlbums.Select(ab => new ArticleAlbumDto
                                    {
                                        id = ab.Id,
                                        articleId = ab.ArticleId,
                                        thumbPath = ab.ThumbPath,
                                        originalPath = ab.OriginalPath,
                                        remark = ab.Remark,
                                        addTime = ab.AddTime,
                                        sortId = ab.SortId
                                    })
                                })
                                .ToList()
            };
            // 排除重复
            if (dto.data.IsAny())
            {
                foreach (var article in dto.data)
                {
                    var index = articles.FindIndex(x => x.Id == article.id);
                    if (index >-1)
                    {
                        articles.RemoveAt(index);
                    }
                }
                result.Add(dto);
            }
        }

        //var result = await _categoryService.QueryArticleListAsync(channelModel.Id, categoryTop, arcitleTop, 0,
        //    x => (string.IsNullOrEmpty(x.GroupIds) || x.GroupIds.Contains($",{groupId},"))//检查是否有权限
        //    && x.ChannelId == channelModel.Id
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,Id");

        return (result);
    }

    /// <summary>
    /// 根据文章别名或ID获取数据
    /// 示例：/client/article/show/1
    /// </summary>
    [HttpGet("/client/article/show/{key}")]
    [CachingFilter(60, true)]
    [AllowAnonymous, NonUnify]
    public async Task<dynamic> ClientGetByKey([FromRoute] string key, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ArticlesClientDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        Articles? model = null;
        if (int.TryParse(key, out int articleId))
        {
            model = await _repository.AsQueryable()
                .Includes(x => x.ArticleAlbums)
                .Includes(x => x.ArticleAttachs)
                .Includes(x => x.ArticleFields)
                .Includes(x => x.CategoryRelations,c=>c.Category)
                .Includes(x => x.LabelRelations,l=>l.Label)
                .Includes(x => x.SiteChannel)
                .SingleAsync(x => x.Id == articleId && x.Status == 0);
        }
        if (model == null)
        {
            model = await _repository.AsQueryable()
                .Includes(x => x.ArticleAlbums)
                .Includes(x => x.ArticleAttachs)
                .Includes(x => x.ArticleFields)
                .Includes(x => x.CategoryRelations, c => c.Category)
                .Includes(x => x.LabelRelations, l => l.Label)
                .Includes(x => x.SiteChannel)
                .WhereIF(param.SiteId > 0, x => x.SiteId == param.SiteId)
                .SingleAsync(x => x.Status == 0 && x.CallIndex == key);
                //.SingleAsync(x => x.Status == 0
                //&& (x.CallIndex != null && x.CallIndex.ToLower() == key.ToLower())
                //&& (param.SiteId <= 0 || x.SiteId == param.SiteId));
        }
        //查询数据库获取实体
        if (model == null)
        {
            throw Oops.Oh($"数据{key}不存在或已删除");
        }
        //检查用户组是否有权限
        if (model.GroupIds != null && model.GroupIds.IsNotEmptyOrNull() && !string.IsNullOrWhiteSpace(model.GroupIds))
        {
            var userId = await  _userService.GetUserIdAsync();
            var memberModel = await _repository.Context.Queryable<Members>().SingleAsync(x => x.UserId == userId);
            if (memberModel == null)
            {
                throw Oops.Oh($"请登录后查看内容");
            }
            if (!model.GroupIds.Contains($",{memberModel.GroupId},"))
            {
                throw Oops.Oh("出错了，此内容仅对部分会员开放");
            }
        }
        //浏览量加一
        model.Click++;
        await _repository.Context.Updateable(model).UpdateColumns(nameof(Articles.Click)).ExecuteCommandAsync();
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ArticlesClientDto>(); // .ShapeData(param.Fields);
        return (result);
    }

    /// <summary>
    /// 更新点击量
    /// 示例：/client/article/1
    /// </summary>
    [HttpPut("/client/article/{id}")]
    [AllowAnonymous, NonUnify]
    public async Task<int> UpdateClick([FromRoute] long id)
    {
        //查出原来的数据
        var oldArticleModel = await _repository.SingleAsync(x => x.Id == id);
        //检测是否为空
        if (oldArticleModel == null)
        {
            throw Oops.Oh("请求参数不正确");
        }
        _repository.Context.Tracking(oldArticleModel);
        oldArticleModel.Click++;
        _repository.Context.Updateable(oldArticleModel);

        return oldArticleModel.Click;
    }

    /// <summary>
    /// 更新点赞量
    /// 示例：/client/article/like/1
    /// </summary>
    [Authorize]
    [HttpPut("/client/article/like/{articleId}")]
    [SqlSugarUnitOfWork]
    public async Task<int> UpdateLike([FromRoute] long articleId)
    {
        var userId = await  _userService.GetUserIdAsync();
        //获得点赞数据实体
        var likeModel = await _repository.Context.Queryable<ArticleLike>().SingleAsync(x => x.ArticleId == articleId && x.UserId == userId);
        //获取文章旧数据
        var oldArticleModel = await _repository.SingleAsync(t => t.Id == articleId);
        if (oldArticleModel == null)
        {
            return 0;
        }
        //用于局部更新
        //Articles articleModel = new Articles();
        if (oldArticleModel != null)
        {
            _repository.Context.Tracking(oldArticleModel);
            //指明更新主键
            //articleModel.Id = oldArticleModel.Id;
            //存在执行删除操作
            if (likeModel != null)
            {
                await _repository.Context.Deleteable<ArticleLike>(likeModel).ExecuteCommandAsync();
                //点赞数
                oldArticleModel.LikeCount = oldArticleModel.LikeCount - 1;
            }
            //不存在执行添加操作
            else
            {
                ArticleLike like = new ArticleLike();
                like.ArticleId = articleId;
                like.AddTime = DateTime.Now;
                like.UserId = userId;
                await _repository.Context.Insertable<ArticleLike>(like).ExecuteCommandAsync();
                //点赞数
                oldArticleModel.LikeCount = oldArticleModel.LikeCount + 1;
            }
            await _repository.Context.Updateable(oldArticleModel).ExecuteCommandAsync();
        }
        else
        {
            return 0;
        }
       
        return oldArticleModel.LikeCount;
    }

    /// <summary>
    /// 获得分类文章导航树
    /// </summary>
    [HttpGet("/client/article/view/{channel}/nav")]
    [CachingFilter(60, true)]
    [AllowAnonymous, NonUnify]
    public async Task<List<ArticlesCategoryNavClientDto>> GetArticleCategoryNav([FromRoute] string channel, /*[FromRoute] int categoryTop, [FromRoute] int arcitleTop,*/ [FromQuery] BaseParameter searchParam)
    {
        //获得频道实体
        var channelModel = await _repository.Context.Queryable<SiteChannel>().SingleAsync(x => x.Name == channel && x.SiteId == searchParam.SiteId);
        if (channelModel == null)
        {
            throw Oops.Oh("频道不存在，请检查后重试。");
        }
        //检查用户是否登录筛选数据
        var userId = await  _userService.GetUserIdAsync();
        int groupId = 0;
        if (userId.IsNotEmptyOrNull())
        {
            var memberModel = await _repository.Context.Queryable<Members>().SingleAsync(x => x.UserId == userId);
            if (memberModel != null)
            {
                groupId = memberModel.GroupId;
            }
        }

        //获取前几条分类,最多获取前10个分类
        //if (categoryTop > 10) categoryTop = 10;
        //if (arcitleTop > 10) arcitleTop = 10;

        var categoryList = await _repository.Context.Queryable<ArticleCategory>()
             .Where(x => x.ChannelId == channelModel.Id && /*x.ParentId == 0 &&*/ x.Status == 0)
             .OrderBy(searchParam.OrderBy ?? "SortId,Id")
             .Select(x => new ArticlesCategoryNavClientDto
             {
                 id = $"n{x.Id}",
                 parentId = $"n{x.ParentId}",
                 label = x.Title ?? ""
             })
             //.Take(categoryTop)
             .ToListAsync();



        //var categoryIds = categoryList.Select(x => x.Id).ToArray();

        var articleCategoryRelations = await _repository.Context.Queryable<ArticleCategoryRelation>()
            .LeftJoin<Articles>((x, a) => x.ArticleId == a.Id)
             //.Where(x=> SqlFunc.Subqueryable<ArticleCategory>())
             //.LeftJoin<ArticleCategory>((x,a,b)=> x.CategoryId == b.Id)
             //.Where(x => categoryIds.Contains(x.CategoryId))
             .Where(x=> SqlFunc.Subqueryable<ArticleCategory>().Where(z=>z.Id == x.CategoryId && z.ChannelId == channelModel.Id && z.Status == 0).Any())
             .Where((x, a) => a.ChannelId == channelModel.Id && a.Status == 0)
             .GroupBy((x,a)=> new { a.Id , a.Title})
             .OrderBy(searchParam.OrderBy ?? "SortId,Id")
             //.WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), (x, a) => a.Title.Contains(searchParam.Keyword))
             .Select((x, a) => new ArticlesCategoryNavClientDto
             {
                 id = SqlFunc.ToString(a.Id),
                 parentId =  $"n{SqlFunc.AggregateMax(x.CategoryId)}",
                 label = a.Title,
                 href = a.LinkUrl ?? ""
             })
             .ToListAsync();

        foreach (var item in categoryList)
        {
            item.children = articleCategoryRelations.Where(x => x.parentId == item.id).ToList() ?? new List<ArticlesCategoryNavClientDto>();
        }

        //if (articleCategoryRelations.IsAny())
        //{
        //    categoryList.AddRange(articleCategoryRelations);
        //}


        Action<ArticlesCategoryNavClientDto> action = null;

        action = (node) =>
        {
            var children = categoryList.Where(x => x.parentId == node.id);

            if (children.IsAny())
            {
                foreach (var item in children)
                {
                    action(item);
                    node.children.Add(item);
                }
            }
        };

        //List<ArticlesCategoryNavClientDto> result = new List<ArticlesCategoryNavClientDto>();
        var result = categoryList.Where(x => x.parentId == "n0").ToList();

        foreach (var category in result)
        {
            action(category);
        }
        return (result);
    }
    #endregion
}