using Microsoft.AspNetCore.Http;
using QT.CMS.Entitys.Dto.Member;
using QT.RemoteRequest.Extensions;

namespace QT.CMS;

/// <summary>
/// 会员商品收藏
/// </summary>
[Route("api/cms/admin/member/favorite")]
[ApiController]
public class MemberFavoriteController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopGoodsFavorite> _shopGoodsFavoriteService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public MemberFavoriteController(ISqlSugarRepository<ShopGoodsFavorite> shopGoodsFavoriteService, IUserService userService)
    {
        _shopGoodsFavoriteService = shopGoodsFavoriteService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/member/favorite/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberFavorite", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopGoodsFavoriteDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopGoodsFavoriteService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopGoodsFavoriteDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/member/favorite/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("MemberFavorite", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopGoodsFavoriteDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopGoodsFavoriteDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _shopGoodsFavoriteService.AsQueryable()
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "-Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _shopGoodsFavoriteService.QueryListAsync<ShopGoodsFavorite>(top,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopGoodsFavoriteDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/member/favorite?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("MemberFavorite", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopGoodsFavoriteDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopGoodsFavoriteDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopGoodsFavoriteService.AsQueryable()
           .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
           .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
           .OrderBy(searchParam.OrderBy ?? "-Id")
             .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);
        return PageResult<ShopGoodsFavoriteDto>.SqlSugarPageResult(list);

        //var list = await _shopGoodsFavoriteService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-Id");

        ////x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.TotalCount,
        //    pageSize = list.PageSize,
        //    pageIndex = list.PageIndex,
        //    totalPages = list.TotalPages
        //};
        //Response.Headers.Add("x-pagination", SerializeHelper.SerializeObject(paginationMetadata));

        ////映射成DTO，根据字段进行塑形
        //var resultDto = _mapper.Map<IEnumerable<ShopGoodsFavoriteDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/member/favorite/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberFavorite", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!await _shopGoodsFavoriteService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        var result = await _shopGoodsFavoriteService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/admin/member/favorite?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("MemberFavorite", ActionType.Delete)]
    public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
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
        await _shopGoodsFavoriteService.DeleteAsync(x => arrIds.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 当前用户调用接口========================
    /// <summary>
    /// 根据商品ID获取数据
    /// 示例：/account/member/favorite/goods/1
    /// </summary>
    [HttpGet("/account/member/favorite/goods/{id}")]
    [NonUnify]
    [Authorize]
    public async Task<IActionResult> GetByGoodsId([FromRoute] int id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopGoodsFavoriteDto>())
        {
            throw Oops.Oh("请输入正确的属性参数").StatusCode(StatusCodes.Status404NotFound);
        }
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录").StatusCode(StatusCodes.Status404NotFound);
        }
        //查询数据库获取实体
        var model = await _shopGoodsFavoriteService
            .AsQueryable()
            .Where(x=>x.UserId == userId && x.ShopGoods.Id == id)
            .SingleAsync();
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除").StatusCode(StatusCodes.Status404NotFound);
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopGoodsFavoriteDto>();//.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/account/member/favorite?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/account/member/favorite")]
    [Authorize]
    [NonUnify]
    public async Task<dynamic> AccountGetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopGoodsFavoriteDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopGoodsFavoriteDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopGoodsFavoriteService.AsQueryable()
            .Includes(x=> x.ShopGoods)
            .Where(x=>x.UserId==userId  && x.ShopGoods.Status == 0)
        .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
        .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
        .OrderBy(searchParam.OrderBy ?? "-Id")
          .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);
        return PageResult<ShopGoodsFavoriteDto>.SqlSugarPageResult(list);

        //var list = await _shopGoodsFavoriteService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.UserId == userId
        //    && x.ShopGoods != null
        //    && x.ShopGoods.Status == 0
        //    && (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-Id");

        ////x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.TotalCount,
        //    pageSize = list.PageSize,
        //    pageIndex = list.PageIndex,
        //    totalPages = list.TotalPages
        //};
        //Response.Headers.Add("x-pagination", SerializeHelper.SerializeObject(paginationMetadata));

        ////映射成DTO，根据字段进行塑形
        //var resultDto = _mapper.Map<IEnumerable<ShopGoodsFavoriteDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/account/member/favorite
    /// </summary>
    [HttpPost("/account/member/favorite")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountAdd([FromBody] ShopGoodsFavoriteAddDto modelDto)
    {
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }
        //获取收藏记录，检查是否重复增加
        var favoriteModel = await _shopGoodsFavoriteService.SingleAsync(x => x.UserId == userId && x.GoodsId == modelDto.goodsId);
        if (favoriteModel != null)
        {
            return Ok(favoriteModel.Adapt<ShopGoodsFavoriteDto>());
        }
        //获取商品信息
        var goodsModel = await _shopGoodsFavoriteService.Context.Queryable<ShopGoods>().SingleAsync(x => x.Id == modelDto.goodsId);
        if (goodsModel == null)
        {
            throw Oops.Oh($"商品不存在或已删除");
        }

        //映射成实体
        var dtoModel = modelDto.Adapt<ShopGoodsFavorite>();
        dtoModel.UserId = userId;
        dtoModel.Title = goodsModel.Title;
        dtoModel.ImgUrl = goodsModel.ImgUrl;
        //写入数据库
        var mapModel = await _shopGoodsFavoriteService.InsertReturnEntityAsync(dtoModel);
        //映射成DTO再返回，否则出错
        var result = mapModel.Adapt<ShopGoodsFavoriteDto>();
        return Ok(result);
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/account/member/favorite/1
    /// </summary>
    [HttpDelete("/account/member/favorite/{id}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountDelete([FromRoute] int id)
    {
        if (!await _shopGoodsFavoriteService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }
        var result = await _shopGoodsFavoriteService.DeleteAsync(x => x.UserId == userId && x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/account/member/favorite?ids=1,2,3
    /// </summary>
    [HttpDelete("/account/member/favorite")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountDeleteByIds([FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }

        //将ID列表转换成IEnumerable
        var arrIds = Ids.ToIEnumerable<long>();
        if (arrIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        await _shopGoodsFavoriteService.DeleteAsync(x => x.UserId == userId && arrIds.Contains(x.Id));

        return NoContent();
    }
    #endregion
}