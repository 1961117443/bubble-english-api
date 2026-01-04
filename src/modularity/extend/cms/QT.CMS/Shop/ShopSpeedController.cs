using Microsoft.AspNetCore.Http;
using QT.CMS.Emum;

namespace QT.CMS;

/// <summary>
/// 限时抢购
/// </summary>
[Route("api/cms/admin/shop/speed")]
[ApiController]
public class ShopSpeedController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopSpeed> _shopSpeedService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ShopSpeedController(ISqlSugarRepository<ShopSpeed> shopSpeedService, IUserService userService)
    {
        _shopSpeedService = shopSpeedService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/shop/speed/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopSpeed", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopSpeedDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopSpeedService.AsQueryable().Includes(x=>x.GoodsProduct,t=>t!.ShopGoods).SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopSpeedDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/shop/speed/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("ShopSpeed", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopSpeedDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopSpeedDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _shopSpeedService.AsQueryable()
            .Includes(x => x.GoodsProduct, t => t!.ShopGoods)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.GoodsProduct.ShopGoods.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _shopSpeedService.QueryListAsync(top,
        //    x => x.GoodsProduct != null
        //    && x.GoodsProduct.ShopGoods != null
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.GoodsProduct.ShopGoods.Title != null && x.GoodsProduct.ShopGoods.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopSpeedDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/shop/speed?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("ShopSpeed", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopSpeedDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopSpeedDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopSpeedService.AsQueryable()
            .Includes(x => x.GoodsProduct, t => t!.ShopGoods)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.GoodsProduct.ShopGoods.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        //var list = await _shopSpeedService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.GoodsProduct != null
        //    && x.GoodsProduct.ShopGoods != null
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.GoodsProduct.ShopGoods.Title != null && x.GoodsProduct.ShopGoods.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

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
        //var resultDto = _mapper.Adapt<IEnumerable<ShopSpeedDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopSpeedDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/shop/speed
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("ShopSpeed", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] ShopSpeedEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<ShopSpeed>();
        //获取当前用户名
        model.AddBy =  await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        var mapModel = await _shopSpeedService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = mapModel.Adapt<ShopSpeedDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/shop/speed/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopSpeed", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] ShopSpeedEditDto modelDto)
    {
        //检查站点是否正确
        if (!await _shopSpeedService.Context.Queryable<Sites>().AnyAsync(x => x.Id == modelDto.siteId))
        {
            throw Oops.Oh("站点不存在或已删除");
        }
        //查找记录
        var model = await _shopSpeedService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _shopSpeedService.Context.Tracking(model);
        //获取当前用户名
        model.UpdateBy = await _userService.GetUserNameAsync();
        model.UpdateTime = DateTime.Now;

        //调用保存
        modelDto.Adapt(model);
        var result = await _shopSpeedService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/shop/speed/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopSpeed", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<ShopSpeedEditDto> patchDocument)
    {
        var model = await _shopSpeedService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<ShopSpeedEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _shopSpeedService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _shopSpeedService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/shop/speed/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopSpeed", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        if (!await _shopSpeedService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _shopSpeedService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/shop/speed?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("ShopSpeed", ActionType.Delete)]
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
        await _shopSpeedService.DeleteAsync(x => arrIds.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 根据ID获取抢购商品列表
    /// 示例：/client/shop/speed/buy/1
    /// </summary>
    [HttpGet("/client/shop/speed/buy/{id}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> ClientGetBuyList([FromRoute] long id)
    {
        //获取数据库列表
        var result = await this.GetBuyListAsync(id);
        //返回成功200
        return Ok(result);
    }

    /// <summary>
    /// 根据货品获取数据
    /// 示例：/client/shop/speed/product/1
    /// </summary>
    [HttpGet("/client/shop/speed/product/{id}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> ClientGetByProductId([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopSpeedDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopSpeedService.AsQueryable()
            .Includes(x => x.GoodsProduct, t => t!.ShopGoods)
            .Where(x => x.ProductId == id && x.Status == 0)
            .Where(x => SqlFunc.Between(DateTime.Now, x.StartTime, x.EndTime))
            .FirstAsync(); 

        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除").StatusCode(StatusCodes.Status404NotFound);
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopSpeedDto>();//.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/shop/speed/view/10
    /// </summary>
    [HttpGet("/client/shop/speed/view/{top}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> ClientGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopSpeedDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopSpeedDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _shopSpeedService.AsQueryable()
            .Includes(x => x.GoodsProduct, t => t!.ShopGoods)
            .Where(x => x.Status == 0 && x.GoodsProduct.ShopGoods.Status == 0)
            .Where(x => SqlFunc.Between(DateTime.Now, x.StartTime, x.EndTime))
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.GoodsProduct.ShopGoods.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .AutoTake(top)
            .ToListAsync();

        //return resultFrom;

        //var resultFrom = await _shopSpeedService.QueryListAsync(top,
        //    x => x.Status == 0
        //    && x.GoodsProduct != null
        //    && x.GoodsProduct.ShopGoods != null
        //    && x.GoodsProduct.ShopGoods.Status == 0
        //    && DateTime.Compare(x.StartTime, DateTime.Now) <= 0
        //    && DateTime.Compare(x.EndTime, DateTime.Now) >= 0
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.GoodsProduct.ShopGoods.Title != null && x.GoodsProduct.ShopGoods.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopSpeedDto>>(); //.ShapeData(searchParam.Fields);
        ////返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/client/shop/speed?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/client/shop/speed")]
    [CachingFilter]
    [NonUnify]
    public async Task<dynamic> ClientGetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopSpeedDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopSpeedDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopSpeedService.AsQueryable()
           .Includes(x => x.GoodsProduct, t => t!.ShopGoods)
           .Where(x => x.Status == 0 && x.GoodsProduct.ShopGoods.Status == 0)
           .Where(x => SqlFunc.Between(DateTime.Now, x.StartTime, x.EndTime))
           .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.GoodsProduct.ShopGoods.Title.Contains(searchParam.keyword))
           .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
           .ToPagedListAsync(pageParam.PageSize, pageParam.PageIndex);

        //var list = await _shopSpeedService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.Status == 0
        //    && x.GoodsProduct != null
        //    && x.GoodsProduct.ShopGoods != null
        //    && x.GoodsProduct.ShopGoods.Status == 0
        //    && DateTime.Compare(x.StartTime, DateTime.Now) <= 0
        //    && DateTime.Compare(x.EndTime, DateTime.Now) >= 0
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.GoodsProduct.ShopGoods.Title != null && x.GoodsProduct.ShopGoods.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

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
        //var resultDto = list.Adapt<IEnumerable<ShopSpeedDto>>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopSpeedDto>.SqlSugarPageResult(list);
    }
    #endregion

    #region Private Method============================

    /// <summary>
    /// 获取抢购商品统计和列表
    /// </summary>
    private async Task<ShopCartTotalDto> GetBuyListAsync(long id)
    {
        //获取当前用户信息
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("会员尚未登录或已超时").StatusCode(StatusCodes.Status401Unauthorized);
        }
        //获取会员组信息
        var memberModel = await _shopSpeedService.Context.Queryable<Members>().Where(x => x.UserId == userId).Includes(x => x.Group).FirstAsync();
        if (memberModel == null)
        {
            throw Oops.Oh("会员不存在或已删除");
        }
        //获取抢购信息
        var model = await _shopSpeedService.AsQueryable()
            .Includes(x => x.GoodsProduct, t => t.ShopGoods)
            .Where(x => x.Id == id && x.Status == 0 && x.GoodsProduct.ShopGoods.Status == 0 && x.GoodsProduct.StockQuantity > 0)
            .Where(x => SqlFunc.Between(DateTime.Now, x.StartTime, x.EndTime))
            .FirstAsync();

        if (model == null)
        {
            throw Oops.Oh("抢购活动不存在或已过期");
        }
        if (model.GoodsProduct == null || model.GoodsProduct.ShopGoods == null)
        {
            throw Oops.Oh("商品已下架或已删除");
        }
        //检查是否有权限兑换
        var groupIdArr = model.GroupIds.ToIEnumerable<int>();
        if (groupIdArr != null && !groupIdArr.Contains(memberModel.GroupId))
        {
            throw Oops.Oh("该活动仅对部分会员开放");
        }
        //创建待赋值的实体
        ShopCartTotalDto result = new ShopCartTotalDto();
        ShopCartDto modelDto = new()
        {
            id = 0,
            siteId = model.GoodsProduct.ShopGoods.SiteId,
            userId = memberModel.UserId,
            status = model.GoodsProduct.ShopGoods.Status == 0 ? (byte)0 : (byte)1,
            addTime = DateTime.Now,
            goodsId = model.GoodsProduct.GoodsId,
            title = model.GoodsProduct.ShopGoods.Title,
            specText = model.GoodsProduct.SpecText,
            imgUrl = model.GoodsProduct.ImgUrl != null ? model.GoodsProduct.ImgUrl : model.GoodsProduct.ShopGoods.ImgUrl,
            marketPrice = model.GoodsProduct.MarketPrice,
            sellPrice = model.GoodsProduct.SellPrice,
            groupPrice = model.Price,
            weight = model.GoodsProduct.Weight,
            stockQuantity = model.GoodsProduct.StockQuantity,
            productId = model.GoodsProduct.Id,
            quantity = 1
        };
        //非免运费商品标记
        if (model.GoodsProduct.ShopGoods.IsDeliveryFee == 0)
        {
            result.freeFreight = 0;
        }
        result.cartList.Add(modelDto);
        //统计购物车属性
        result.totalQuantity = result.cartList.Select(x => x.quantity).Sum();
        result.totalWeight = result.cartList.Select(x => x.weight * x.quantity).Sum();
        result.payableAmount = result.cartList.Select(x => x.sellPrice * x.quantity).Sum();
        result.realAmount = result.cartList.Select(x => x.groupPrice * x.quantity).Sum();
        result.orderAmount = result.realAmount;

        return result;
    } 
    #endregion
}