
using Microsoft.AspNetCore.Http;

namespace QT.CMS;

/// <summary>
/// 优惠券历史记录
/// </summary>
[Route("api/cms/admin/shop/coupon/history")]
[ApiController]
public class ShopCouponHistoryController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopCouponHistory> _shopCouponHistoryService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ShopCouponHistoryController(ISqlSugarRepository<ShopCouponHistory> shopCouponHistoryService, IUserService userService)
    {
        _shopCouponHistoryService = shopCouponHistoryService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/shop/coupon/history/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopCouponHistoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopCouponHistoryService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopCouponHistoryDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/shop/coupon/history/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] RefundParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopCouponHistoryDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopCouponHistoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        //var resultFrom = await _shopCouponHistoryService.QueryListAsync(top,
        //    x => (!searchParam.OrderNo.IsNotNullOrEmpty() || (x.OrderNo != null && x.OrderNo.Equals(searchParam.OrderNo))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        var resultFrom = await _shopCouponHistoryService.AsQueryable()
            .Includes(x => x.ShopCoupon)
            .WhereIF(searchParam.OrderNo.IsNotEmptyOrNull(), x => x.OrderNo == searchParam.OrderNo)
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .AutoTake(top)
            .ToListAsync();

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopCouponHistoryDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/shop/coupon/history?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopCouponHistoryDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopCouponHistoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        //var list = await _shopCouponHistoryService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => !searchParam.Keyword.IsNotNullOrEmpty() || (x.OrderNo != null && x.OrderNo.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        var list = await _shopCouponHistoryService.AsQueryable()
            .Includes(x => x.ShopCoupon)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.OrderNo.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .ToPagedListAsync(searchParam.currentPage,searchParam.pageSize);

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
        //var resultDto = _mapper.Adapt<IEnumerable<ShopCouponHistoryDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopCouponHistoryDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/shop/coupon/history
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] ShopCouponHistoryEditDto modelDto)
    {
        //检查会员是否正确
        if (!await _shopCouponHistoryService.AnyAsync(x => x.UserId == modelDto.userId))
        {
            throw Oops.Oh("会员账户不存在或已删除");
        }
        //映射成实体
        var model = modelDto.Adapt<ShopCouponHistory>();
        model.AddTime = DateTime.Now;

        //写入数据库
        var mapModel = await _shopCouponHistoryService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = mapModel.Adapt<ShopCouponHistoryDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/shop/coupon/history/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] ShopCouponHistoryEditDto modelDto)
    {
        //检查会员是否正确
        if (!await _shopCouponHistoryService.Context.Queryable<Members>().AnyAsync(x => x.UserId == modelDto.userId))
        {
            throw Oops.Oh("会员账户不存在或已删除");
        }
        //查找记录
        var model = await _shopCouponHistoryService.SingleAsync(x => x.Id == id);

        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        _shopCouponHistoryService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelDto.Adapt(model);
        var result = await _shopCouponHistoryService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/shop/coupon/history/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<ShopCouponHistoryEditDto> patchDocument)
    {
        var model = await _shopCouponHistoryService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<ShopCouponHistoryEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _shopCouponHistoryService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        var result = await _shopCouponHistoryService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/shop/coupon/history/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        if (!await _shopCouponHistoryService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _shopCouponHistoryService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/admin/shop/coupon/history?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.Delete)]
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
        await _shopCouponHistoryService.DeleteAsync(x => arrIds.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 当前用户调用接口========================
    /// <summary>
    /// 获取会员优惠券分页列表
    /// 示例：/account/coupon/history?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/account/coupon/history")]
    [Authorize]
    [NonUnify]
    public async Task<dynamic> AccountGetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopCouponHistoryDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopCouponHistoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        //var list = await _shopCouponHistoryService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.UserId == userId
        //    && x.ShopCoupon != null
        //    && (searchParam.SiteId == 0 || searchParam.SiteId == x.ShopCoupon.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.OrderNo != null && x.OrderNo.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        var list = await _shopCouponHistoryService.AsQueryable()
            .Includes(x => x.ShopCoupon)
            .Where(x => x.UserId == userId)
            .WhereIF(searchParam.SiteId > 0, x => x.ShopCoupon.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.OrderNo.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

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
        //var resultDto = _mapper.Adapt<IEnumerable<ShopCouponHistoryDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopCouponHistoryDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/account/shop/coupon/add
    /// </summary>
    [HttpPost("/account/shop/coupon/add")]
    [Authorize]
    [NonUnify]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> AccountAdd([FromBody] ShopCouponHistoryEditDto modelDto)
    {
        var mapModel = await this.AddAsync(modelDto);
        //映射成DTO再返回，否则出错
        var result = mapModel.Adapt<ShopCouponHistoryDto>();
        return Ok(result);
    }

    /// <summary>
    /// 获取用户可用优惠券列表
    /// 示例：/account/shop/coupon/list
    /// </summary>
    [HttpPost("/account/shop/coupon/list")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetList([FromBody] List<ShopCartEditDto> listDto, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (!searchParam.Fields.IsPropertyExists<ShopCouponHistoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        if (listDto == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }

        //获取当前用户信息
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("会员尚未登录或已超时").StatusCode(StatusCodes.Status401Unauthorized);
        }
        //获取会员组信息
        var memberModel = await _shopCouponHistoryService.Context.Queryable<Members>()
            .Includes(x => x.Group).SingleAsync(x => x.UserId == userId);
        if (memberModel == null || memberModel.Group == null)
        {
            throw Oops.Oh("会员不存在或已删除");
        }
        //检查是否有商品
        var buyLlist = listDto;
        if (buyLlist == null)
        {
            throw Oops.Oh("请选择购买的商品");
        }
        //获取商品列表
        var productList = await _shopCouponHistoryService.Context.Queryable<ShopGoodsProduct>()
            .Includes(x=>x.ShopGoods,t=>t!.CategoryRelations)
            .Includes(x=>x.GroupPrices)
            .Where(x => buyLlist.Select(p => p.productId).Contains(x.Id))
            .ToListAsync();
        if (productList == null)
        {
            throw Oops.Oh("未查询到商品信息");
        }
        //创建待赋值的列表
        List<ShopCartDto> cartList = new();
        //遍历购物车商品
        foreach (var item in productList)
        {
            if (item.ShopGoods == null)
            {
                continue;
            }
            //计算出会员组折扣价格
            decimal groupPrice = item.SellPrice * ((decimal)memberModel.Group.Discount / 100);
            //计算出商品会员组价格
            var priceModel = item.GroupPrices.FirstOrDefault(x => x.GroupId == memberModel.GroupId);
            if (priceModel != null)
            {
                groupPrice = priceModel.Price;
            }
            ShopCartDto modelDto = new ShopCartDto
            {
                id = 0,
                siteId = item.ShopGoods.SiteId,
                userId = memberModel.UserId,
                status = item.ShopGoods.Status == 0 ? (byte)0 : (byte)1,
                addTime = DateTime.Now,
                goodsId = item.GoodsId,
                title = item.ShopGoods.Title,
                specText = item.SpecText,
                imgUrl = item.ImgUrl != null ? item.ImgUrl : item.ShopGoods.ImgUrl,
                marketPrice = item.MarketPrice,
                sellPrice = item.SellPrice,
                groupPrice = groupPrice,
                stockQuantity = item.StockQuantity,
                productId = item.Id,
                quantity = buyLlist.FirstOrDefault(x => x.productId == item.Id)?.quantity ?? 0
            };
            cartList.Add(modelDto);
        }
        //获取用户可用的购物券列表
        var historyCouponList = await _shopCouponHistoryService.Context.Queryable<ShopCouponHistory>()
            .Includes(x => x.ShopCoupon,t=>t!.CategoryRelations)
            .Includes(x => x.ShopCoupon,t=>t!.GoodsRelations)
            .Where(x=>x.UserId == userId && x.Status ==0 && x.IsUse ==0)
            .Where(x=> SqlFunc.Between(DateTime.Now,x.ShopCoupon.StartTime,x.ShopCoupon.EndTime))
            .ToListAsync();

        //创建一个用于存放可用的购物券列表
        var productTotalPrice = cartList.Select(x => x.sellPrice * x.quantity).Sum();
        List<ShopCouponHistory> resultList = new List<ShopCouponHistory>();
        //遍历购物券，查找与商品匹配的购物券加入到可用购物券列表
        foreach (var modelt in historyCouponList)
        {
            if (modelt.ShopCoupon == null)
            {
                continue;
            }
            var cartTotalAmount = cartList.Select(x => x.sellPrice * x.quantity).Sum(); //计算购物车商品总金额
                                                                                        //0.全场通用
            if (modelt.ShopCoupon.UseType == 0)
            {
                //如果不限金额或商品金额大于等于购物券设定的值则有效
                if (cartTotalAmount >= modelt.ShopCoupon.MinAmount && (modelt.ShopCoupon.MinAmount == 0 || cartTotalAmount >= modelt.ShopCoupon.Amount))
                {
                    resultList.Add(modelt);
                }
            }
            //1.指定分类
            else if (modelt.ShopCoupon.UseType == 1)
            {
                //取出优惠券可用商品分类ID
                var couponCategoryIds = modelt.ShopCoupon.CategoryRelations.Select(x => x.CategoryId);
                //查找符合条件的商品
                var listIds = productList.Where(
                    x => x.ShopGoods != null && x.ShopGoods.CategoryRelations.Select(x => x.CategoryId).Intersect(couponCategoryIds).Count() > 0).Select(x => x.Id);
                if (listIds != null
                    && cartTotalAmount >= modelt.ShopCoupon.MinAmount
                    && cartList.Where(x => listIds.Contains(x.productId)).Select(x => x.sellPrice * x.quantity).Sum() >= modelt.ShopCoupon.MinAmount)
                {
                    resultList.Add(modelt);
                }
            }
            //2.指定商品
            else if (modelt.ShopCoupon.UseType == 2)
            {
                //取出优惠券可用商品ID
                var couponGoodsIds = modelt.ShopCoupon.GoodsRelations.Select(x => x.GoodsId);
                //查找符合条件的商品
                var goodsIds = productList.Where(x => couponGoodsIds.Contains(x.GoodsId)).Select(x => x.GoodsId);
                if (goodsIds != null
                    && cartTotalAmount >= modelt.ShopCoupon.MinAmount
                    && cartList.Where(x => goodsIds.Contains(x.goodsId)).Select(x => x.sellPrice * x.quantity).Sum() >= modelt.ShopCoupon.MinAmount)
                {
                    resultList.Add(modelt);
                }
            }
        }

        //映射成DTO，根据字段进行塑形
        var resultDto = resultList.Adapt<IEnumerable<ShopCouponHistoryDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取会员可用优惠券总数量
    /// 示例：/account/shop/coupon/count
    /// </summary>
    [HttpGet("/account/shop/coupon/count")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetCount([FromQuery] GoodsParameter searchParam)
    {
        //获取登录用户ID
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }
        var result = await _shopCouponHistoryService.AsQueryable()
            .Includes(x => x.ShopCoupon)
            .Where(x => x.UserId == userId && x.Status == 0 && x.IsUse == 0)
            .Where(x => SqlFunc.Exists(x.ShopCoupon.Id) && SqlFunc.Between(DateTime.Now, x.ShopCoupon.StartTime, x.ShopCoupon.EndTime))
            .ToListAsync();
        //返回成功200
        return Ok(result);
    }
    #endregion


    /// <summary>
    /// 会员领取优惠券
    /// </summary>
    private async Task<ShopCouponHistory> AddAsync(ShopCouponHistoryEditDto modelDto)
    {
        //获取登录用户ID
        var userId = await  _userService.GetUserIdAsync();
        var memberModel = await _shopCouponHistoryService.Context.Queryable<Members>().SingleAsync(x => x.UserId == userId);
        if (userId.IsNullOrEmpty() || memberModel == null)
        {
            throw Oops.Oh("账户尚未登录").StatusCode(StatusCodes.Status401Unauthorized);
        }
        //检查用户是否领取
        if (await _shopCouponHistoryService.Context.Queryable<ShopCouponHistory>().SingleAsync(x => x.Status == 0
             && x.UserId == userId
             && x.CouponId == modelDto.couponId) != null)
        {
            throw Oops.Oh("已领取过该优惠券");
        }
        //查询优惠券信息
        var couponModel = await _shopCouponHistoryService.Context.Queryable<ShopCoupon>().SingleAsync(x => x.Id == modelDto.couponId
            && SqlFunc.Between(DateTime.Now, x.StartTime, x.EndTime));
        if (couponModel == null)
        {
            throw Oops.Oh("优惠券不存在或已过期");
        }
        if (couponModel.ReceiveCount >= couponModel.PublishCount)
        {
            throw Oops.Oh("优惠券已领完");
        }
        if (couponModel.Point > memberModel.Point)
        {
            throw Oops.Oh("积分不足，无法兑换优惠券");
        }

        //扣取会员积分
        if (couponModel.Point > 0)
        {
            //增加会员积分记录
            await _shopCouponHistoryService.Context.Insertable<MemberPointLog>(new MemberPointLog
            {
                UserId = memberModel.UserId,
                Value = couponModel.Point * -1,
                Remark = $"兑换优惠券，优惠券ID:{couponModel.Id}"
            }).ExecuteCommandAsync();
            //扣减会员账户积分
            memberModel.Point -= couponModel.Point;
            await _shopCouponHistoryService.Context.Updateable<Members>(memberModel).UpdateColumns(x => x.Point).ExecuteCommandAsync();
        }
        //更改优惠券信息
        couponModel.ReceiveCount++;
        await _shopCouponHistoryService.Context.Updateable<ShopCoupon>(couponModel).UpdateColumns(x => x.ReceiveCount).ExecuteCommandAsync();


        //添加优惠券领取记录
        var model = modelDto.Adapt<ShopCouponHistory>();
        model.UserId = userId;
        model.Type = 1;
        model.AddTime = DateTime.Now;
        var result = await _shopCouponHistoryService.InsertReturnIdentityAsync(model) > 0;
        //统一保存到数据库
        if (!result)
        {
            throw Oops.Oh("数据保存时发生意外错误");
        }
        //映射成DTO
        return model.Adapt<ShopCouponHistory>();
    }

}