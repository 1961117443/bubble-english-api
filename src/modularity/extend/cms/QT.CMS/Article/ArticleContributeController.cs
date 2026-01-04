using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using QT;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Article;
using QT.CMS.Entitys.Dto.Parameter;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DynamicApiController;
using QT.FriendlyException;
using QT.JsonSerialization;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 文章投稿
/// </summary>
[Route("/api/cms/admin/article/contribute")]
[ApiController]
[ApiDescriptionSettings(ModuleConst.CMS, Tag = "admin", Name = "contribute", Order = 200)]
public class ArticleContributeController : IDynamicApiController
{
    private readonly ISqlSugarRepository<ArticleContribute> _repository;
    private readonly IUserService _userService;
    //private readonly IMapper _mapper;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ArticleContributeController(ISqlSugarRepository<ArticleContribute> repository,
        IUserService userService)
    {
        //_articleContributeService = articleContributeService;
        _repository = repository;
        _userService = userService;
        //_mapper = mapper;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/article/contribute/1/1
    /// </summary>
    [HttpGet("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("ArticleContribute", ActionType.View, "channelId")]
    public async Task<dynamic> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ArticleContributeDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _repository.SingleAsync(x=>x.Id==id);
        //var model = await _articleContributeService.QueryAsync<ArticleContribute>(x => x.Id == id, WriteRoRead.Write);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ArticleContributeDto>().ShapeData(param.Fields);
        return (result);
    }

    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/article/contribute/1/1/view
    /// </summary>
    [HttpGet("{channelId}/{id}/view")]
    [Authorize]
    //[AuthorizeFilter("ArticleContribute", ActionType.View, "channelId")]
    public async Task<dynamic> GetByIdView([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ArticleContributeDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _repository.SingleAsync(x=>x.Id ==id);
        //var model = await _articleContributeService.QueryAsync<ArticleContribute>(x => x.Id == id, WriteRoRead.Write);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ArticleContributeViewDto>().ShapeData(param.Fields);
        return (result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/article/contribute/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    //[AuthorizeFilter("ArticleContribute", ActionType.View, "channelId")]
    public async Task<dynamic> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticleContributeDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticleContributeDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        ////获取数据库列表
       var resultFrom = await  _repository.AsQueryable()
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "Status,-Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _articleContributeService.QueryListAsync<ArticleContribute>(top,
        //    x =>  || (x.Title != null && x.Title.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy ?? "Status,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<List<ArticleContributeDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/article/contribute/1?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("ArticleContribute", ActionType.View, "channelId")]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam, [FromRoute] long channelId)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticleContributeDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticleContributeDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list =await _repository.AsQueryable()
            .Where(x => x.ChannelId == channelId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword) || x.UserName.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "Status,-Id")
            .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);


        //var list = await _articleContributeService.QueryPageAsync<ArticleContribute>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.ChannelId == channelId
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)))
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.UserName != null && x.UserName.Contains(searchParam.Keyword)))
        //    , searchParam.OrderBy ?? "Status,-Id");

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
        //var resultDto = list.Adapt<IEnumerable<ArticleContributeDto>>().ShapeData(searchParam.Fields);
        return (PageResult<ArticleContributeDto>.SqlSugarPageResult(list));
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/article/contribute/1
    /// </summary>
    [HttpPost("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("ArticleContribute", ActionType.Add, "channelId")]
    public async Task<ArticleContributeDto> Add([FromBody] ArticleContributeAddDto modelDto)
    {
        var siteChannel = await _repository.Context.Queryable<SiteChannel>().SingleAsync(x => x.Id == modelDto.channelId);
        //检查频道是否存在
        if (siteChannel == null)
        {
            throw Oops.Oh("频道不存在或已删除");
        }
        //检查是否可以投稿
        if (siteChannel.IsContribute == 0)
        {
            throw Oops.Oh("该频道不允许投稿");
        }
        //检查站点是否存在
        modelDto.siteId = siteChannel.SiteId;
        if (!await _repository.Context.Queryable<Sites>().AnyAsync(x => x.Id == modelDto.siteId))
        {
            throw Oops.Oh("站点不存在或已删除");
        }

        //映射成实体
        var model = modelDto.Adapt<ArticleContribute>();
        //获取当前用户名
        model.UserId = await  _userService.GetUserIdAsync();
        model.UserName = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        await _repository.InsertAsync(model);
        //var mapModel = await _articleContributeService.AddAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<ArticleContributeDto>();
        return (result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/article/contribute/1/1
    /// </summary>
    [HttpPut("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("ArticleContribute", ActionType.Edit, "channelId")]
    public async Task Update([FromRoute] long id, [FromBody] ArticleContributeEditDto modelDto)
    {
        ////验证数据是否合法
        //if (!TryValidateModel(modelDto))
        //{
        //    return ValidationProblem(ModelState);
        //}
        //var user = await _userService.GetUserAsync();
        //if (user == null)
        //{
        //    throw Oops.Oh("用户未登录或已超时");
        //}
        var model = await _repository.SingleAsync(x => x.Id == id);
        _repository.Context.Tracking(model);
        modelDto.Adapt(model);
        model.UpdateBy = await _userService.GetUserNameAsync();
        model.UpdateTime = DateTime.Now;
        await _repository.UpdateAsync(model);
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/article/contribute/1/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("ArticleContribute", ActionType.Edit, "channelId")]
    public async Task Update([FromRoute] long id, [FromBody] JsonPatchDocument<ArticleContributeEditDto> patchDocument)
    {
        var model = await _repository.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _repository.Context.Tracking(model);

        //var modelToPatch = model.Adapt<ArticleContributeEditDto>();
        patchDocument.Adapt(model);
        //patchDocument.ApplyTo(modelToPatch, ModelState);
        ////验证数据是否合法
        //if (!TryValidateModel(modelToPatch))
        //{
        //    return ValidationProblem(ModelState);
        //}
        ////更新操作AutoMapper替我们完成，只需要调用保存即可
        //_mapper.Map(modelToPatch, model);
        await _repository.UpdateAsync(model);
         
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/article/contribute/1/1
    /// </summary>
    [HttpDelete("{channelId}/{id}")]
    [Authorize]
    //[AuthorizeFilter("ArticleContribute", ActionType.Delete, "channelId")]
    public async Task Delete([FromRoute] long id)
    {
        if (!await _repository.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        var result = await _repository.DeleteAsync(x => x.Id == id);
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/admin/article/contribute/1?ids=1,2,3
    /// </summary>
    [HttpDelete("{channelId}")]
    [Authorize]
    //[AuthorizeFilter("ArticleContribute", ActionType.Delete, "channelId")]
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
    #endregion

    #region 普通账户调用接口========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/account/article/contribute/1
    /// </summary>
    [HttpGet("/account/article/contribute/{id}")]
    [Authorize]
    [NonUnify]
    public async Task<dynamic> ClientGetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ArticleContributeDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        var userId = await  _userService.GetUserIdAsync();
        //查询数据库获取实体
        var model = await _repository.SingleAsync(x => x.Id == id && x.UserId == userId);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ArticleContributeDto>().ShapeData(param.Fields);
        return (result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/account/article/contribute/view/10
    /// </summary>
    [HttpGet("/account/article/contribute/view/{top}")]
    [Authorize]
    [NonUnify]
    public async Task<dynamic> ClientGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticleContributeDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticleContributeDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        var userId = await  _userService.GetUserIdAsync();
        //获取数据库列表
        var resultFrom = await _repository.Context.Queryable<ArticleContribute>()
            .Where(x => x.UserId == userId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "Status,-Id")
            .AutoTake(top)
            .ToListAsync();

        //var resultFrom = await _articleContributeService.QueryListAsync<ArticleContribute>(top,
        //    x => x.UserId == userId
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "Status,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ArticleContributeDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/account/article/contribute?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/account/article/contribute")]
    [Authorize]
    [NonUnify]
    public async Task<dynamic> ClientGetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ArticleContributeDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ArticleContributeDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        var userId = await  _userService.GetUserIdAsync();

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _repository.AsQueryable()
            .Where(x => x.UserId == userId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "Status,-Id")
            .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        //var list = await _articleContributeService.QueryPageAsync<ArticleContribute>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.UserId == userId
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "Status,-Id");

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
        //var resultDto = list.Adapt<IEnumerable<ArticleContributeDto>>(); //.ShapeData(searchParam.Fields);
        return PageResult<ShopGoodsFavoriteDto>.SqlSugarPageResult(list); ;
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/account/article/contribute
    /// </summary>
    [HttpPost("/account/article/contribute")]
    [Authorize]
    [NonUnify]
    public async Task<dynamic> ClientAdd([FromBody] ArticleContributeAddDto modelDto)
    {
        ////验证数据是否合法
        //if (!TryValidateModel(modelDto))
        //{
        //    return ValidationProblem(ModelState);
        //}
        var siteChannel = await _repository.Context.Queryable<SiteChannel>().SingleAsync(x => x.Id == modelDto.channelId);
        //检查频道是否存在
        if (siteChannel == null)
        {
            throw Oops.Oh("频道不存在或已删除");
        }
        //检查是否可以投稿
        if (siteChannel.IsContribute == 0)
        {
            throw Oops.Oh("该频道不允许投稿");
        }
        //检查站点是否存在
        modelDto.siteId = siteChannel.SiteId;
        if (!await _repository.Context.Queryable<Sites>().AnyAsync(x => x.Id == modelDto.siteId))
        {
            throw Oops.Oh("站点不存在或已删除");
        }

        //映射成实体
        var model = modelDto.Adapt<ArticleContribute>();
        //获取当前用户名
        model.UserId = await  _userService.GetUserIdAsync();
        model.UserName = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        await _repository.InsertAsync(model);
        //var mapModel = await _articleContributeService.AddAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<ArticleContributeDto>();
        return (result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/account/article/contribute/1
    /// </summary>
    [HttpPut("/account/article/contribute/{id}")]
    [Authorize]
    [NonUnify]
    public async Task ClientUpdate([FromRoute] long id, [FromBody] ArticleContributeEditDto modelDto)
    {
        ////验证数据是否合法
        //if (!TryValidateModel(modelDto))
        //{
        //    return ValidationProblem(ModelState);
        //}
        //var user = await _userService.GetUserAsync();
        //if (user == null)
        //{
        //    throw Oops.Oh($"用户未登录或已超时");
        //}

        var model = await _repository.SingleAsync(x => x.Id == id);
        _repository.Context.Tracking(model);
        modelDto.Adapt(model);
        model.UserId = await  _userService.GetUserIdAsync();
        model.UserName = await _userService.GetUserNameAsync();
        model.UpdateBy = await _userService.GetUserNameAsync();
        await _repository.UpdateAsync(model);
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/account/article/contribute/1
    /// </summary>
    [HttpDelete("/account/article/contribute/{id}")]
    [Authorize]
    [NonUnify]
    public async Task ClientDelete([FromRoute] long id)
    {
        var userId = await _userService.GetUserIdAsync();//获取当前用户ID
        if (!await _repository.AnyAsync(x => x.Id == id && x.UserId == userId))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        await _repository.DeleteAsync(x => x.Id == id);
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/account/article/contribute?ids=1,2,3
    /// </summary>
    [HttpDelete("/account/article/contribute")]
    [Authorize]
    [NonUnify]
    public async Task ClientDeleteByIds([FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        var userId = await  _userService.GetUserIdAsync();//获取当前用户ID
        //将ID列表转换成IEnumerable
        var arrIds = Ids.ToIEnumerable<long>();
        if (arrIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        await _repository.DeleteAsync(x => arrIds.Contains(x.Id) && x.UserId == userId);
    }
    #endregion
}