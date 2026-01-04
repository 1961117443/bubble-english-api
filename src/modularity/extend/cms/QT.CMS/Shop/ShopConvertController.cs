using Microsoft.AspNetCore.Http;

namespace QT.CMS;

/// <summary>
/// 积分兑换列表
/// </summary>
[Route("api/cms/admin/shop/convert")]
[ApiController]
public class ShopConvertController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopConvert> _shopPointConvert;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ShopConvertController(ISqlSugarRepository<ShopConvert> shopCouponService, IUserService userService)
    {
        _shopPointConvert = shopCouponService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/shop/convert/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopConvertDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopPointConvert.AsQueryable().Includes(x=>x.GoodsProduct,t=> t!.ShopGoods).SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopConvertDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/shop/convert/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopConvertDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopConvertDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _shopPointConvert.AsQueryable()
            .Includes(x => x.GoodsProduct, t => t!.ShopGoods)
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .AutoTake(top)
            .ToListAsync();

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopConvertDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/shop/convert?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopConvertDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopConvertDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopPointConvert.AsQueryable()
            .Includes(x => x.GoodsProduct, t => t!.ShopGoods)
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

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
        //var resultDto = _mapper.Adapt<IEnumerable<ShopConvertDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopConvertDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/shop/convert
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] ShopConvertEditDto modelDto)
    {
        //检查站点是否正确
        if (!await _shopPointConvert.Context.Queryable<Sites>().AnyAsync(x => x.Id == modelDto.siteId))
        {
            throw Oops.Oh("站点不存在或已删除");
        }
        //映射成实体
        var model = modelDto.Adapt<ShopConvert>();
        //获取用户名
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        await _shopPointConvert.InsertReturnIdentityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<ShopConvertDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/shop/convert/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] ShopConvertEditDto modelDto)
    {
        //检查站点是否正确
        if (!await _shopPointConvert.Context.Queryable<Sites>().AnyAsync(x => x.Id == modelDto.siteId))
        {
            throw Oops.Oh("站点不存在或已删除");
        }
        //查找记录
        var model = await _shopPointConvert.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _shopPointConvert.Context.Tracking(model);
        //获取当前用户名
        model.UpdateBy = await _userService.GetUserNameAsync();
        model.UpdateTime = DateTime.Now;

        //调用保存
        modelDto.Adapt(model);
        var result = await _shopPointConvert.UpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/shop/convert/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<ShopConvertEditDto> patchDocument)
    {
        var model = await _shopPointConvert.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<ShopConvertEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _shopPointConvert.Context.Tracking(model);

        //调用保存即可
        modelToPatch.Adapt(model);
        await _shopPointConvert.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/shop/convert/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.Delete)]
    [SqlSugarUnitOfWork]
    public async Task Delete([FromRoute] long id)
    {
        if (!await _shopPointConvert.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        await _shopPointConvert.DeleteAsync(x => x.Id == id);
        await _shopPointConvert.Context.Deleteable<ShopConvertHistory>(x => x.ConvertId == id).ExecuteCommandAsync();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/admin/shop/convert?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.Delete)]
    [SqlSugarUnitOfWork]
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
        await _shopPointConvert.DeleteAsync(x => arrIds.Contains(x.Id));
        await _shopPointConvert.Context.Deleteable<ShopConvertHistory>(x => arrIds.Contains(x.ConvertId)).ExecuteCommandAsync();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/client/shop/convert/1
    /// </summary>
    [HttpGet("/client/shop/convert/{id}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> ClientGetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopConvertDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //查询数据库获取实体
        var model = await _shopPointConvert.AsQueryable().Includes(x => x.GoodsProduct, t => t!.ShopGoods)
            .Where(x => x.Status == 0 && x.Id == id)
            .FirstAsync();
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopConvertDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/client/shop/convert?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/client/shop/convert")]
    [CachingFilter]
    [NonUnify]
    public async Task<dynamic> ClientGetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopConvertDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopConvertDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopPointConvert.AsQueryable()
            .Includes(x => x.GoodsProduct, t => t!.ShopGoods)
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .Where(x=>x.Status == 0 && x.GoodsProduct.ShopGoods.Status == 0)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword) || x.GoodsProduct.ShopGoods.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);


        ////获取数据列表，如果ID大于0则查询该用户下所有的列表
        //var list = await _shopPointConvert.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.Status == 0
        //    && x.GoodsProduct != null
        //    && x.GoodsProduct.ShopGoods != null
        //    && (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && x.GoodsProduct.ShopGoods.Status == 0
        //    && (!searchParam.Keyword.IsNotNullOrEmpty()
        //    || (x.Title != null && x.Title.Contains(searchParam.Keyword))
        //    || (x.GoodsProduct.ShopGoods.Title != null && x.GoodsProduct.ShopGoods.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.TotalCount,
        //    pageSize = list.PageSize,
        //    pageIndex = list.PageIndex,
        //    totalPages = list.TotalPages
        //};
        //Response.Headers.Add("x-pagination", SerializeHelper.SerializeObject(paginationMetadata));

        ////映射成DTO，根据字段进行塑形
        //var resultDto = list.Adapt<IEnumerable<ShopConvertDto>>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopConvertDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 根据ID获取兑换商品列表
    /// 示例：/client/shop/convert/buy/1
    /// </summary>
    [HttpGet("/client/shop/convert/buy/{id}")]
    [Authorize]
    [NonUnify]
    public async Task<ShopCartTotalDto> ClientGetBuyList([FromRoute] long id)
    {
        //获取当前用户信息
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("会员尚未登录或已超时").StatusCode(StatusCodes.Status401Unauthorized);
        }
        //获取会员组信息
        var memberModel = await _shopPointConvert.Context.Queryable<Members>().Where(x => x.UserId == userId).Includes(x => x.Group).FirstAsync();
        if (memberModel == null)
        {
            throw Oops.Oh("会员不存在或已删除");
        }
        //获取换购信息
        var model = await _shopPointConvert.AsQueryable()
            .Includes(x => x.GoodsProduct, t => t!.ShopGoods)
            .Where(x => x.Id == id && x.Status == 0)
            .Where(x => SqlFunc.Exists(x.GoodsProduct.Id) && SqlFunc.Exists(x.GoodsProduct.ShopGoods.Id) && x.GoodsProduct.ShopGoods.Status == 0 && x.GoodsProduct.StockQuantity > 0)
            .FirstAsync();
            
        if (model == null)
        {
            throw Oops.Oh("换购活动不存在或已过期");
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
        ShopCartDto modelDto = new ShopCartDto
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
            groupPrice = model.GoodsProduct.SellPrice,
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
        result.realAmount = 0;
        result.orderAmount = result.realAmount;
        //兑换积分
        result.totalPoint = model.Point;

        return result;
    }
    #endregion
}