using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Emum;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Parameter;
using QT.Common.Core.Manager;
using QT.Common.Extension;
using SqlSugar;
using System.Linq.Expressions;

namespace QT.CMS;

/// <summary>
/// 购物车
/// </summary>
[Route("account/shop/cart")]
[ApiController]
[NonUnify]
public class ShopCartController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopCart> _shopCartService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ShopCartController(ISqlSugarRepository<ShopCart> shopCartService, IUserService userService)
    {
        _shopCartService = shopCartService;
        _userService = userService;
    }

    #region 普通账户调用接口========================
    /// <summary>
    /// 获取购物车列表
    /// 示例：/account/shop/cart/list
    /// </summary>
    [HttpGet("list")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopCartDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopCartDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取当前用户信息
        var userId =  await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("会员尚未登录或已超时").StatusCode(StatusCodes.Status401Unauthorized);
        }
        //获取会员组信息
        var memberModel = await _shopCartService.Context.Queryable<Members>()
            .Where(x => x.UserId == userId)
            .Includes(x => x.Group)
            .FirstAsync();
        if (memberModel == null || memberModel.Group == null)
        {
            throw Oops.Oh("会员不存在或已删除");
        }

        //创建待赋值的实体
        ICollection<ShopCartDto> result = new List<ShopCartDto>();
        //获取购物车列表
        //var list = await _shopCartService.AsQueryable<ShopCart>().Where(x => x.UserId == userId).Where(funcWhere).ToListAsync();
        var list = await _shopCartService.AsQueryable()
            .Where(x => x.UserId == userId)
           .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
           .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
           .ToListAsync();

        var productIdList = list.Select(x => x.ProductId).Distinct().ToList();

        var productList = await _shopCartService.Context.Queryable<ShopGoodsProduct>().Where(x => productIdList.Contains(x.Id))
                 .Includes(x => x.ShopGoods)
                 .Includes(x => x.GroupPrices)
                 .ToListAsync();

        //遍历购物车商品
        foreach (var item in list)
        {
            //创建购物车实体用于存放,使用AutoMapper映射
            var mapModel = item.Adapt<ShopCartDto>();
            //获取商品信息
            var productModel = productList.Where(x => x.Id == item.ProductId).FirstOrDefault();
            //var productModel = await _shopCartService.Context.Queryable<ShopGoodsProduct>().Where(x => x.Id == item.ProductId)
            //    .Includes(x => x.ShopGoods)
            //    .Includes(x => x.GroupPrices)
            //    .FirstAsync();
            if (productModel != null && productModel.ShopGoods != null)
            {
                //计算出会员组折扣价格
                decimal groupPrice = productModel.SellPrice * ((decimal)memberModel.Group.Discount / 100);
                //计算出商品会员组价格
                var priceModel = productModel.GroupPrices.FirstOrDefault(x => x.GroupId == memberModel.GroupId);
                if (priceModel != null)
                {
                    groupPrice = priceModel.Price;
                }
                //给购物车实体赋值
                mapModel.goodsId = productModel.GoodsId;
                mapModel.marketPrice = productModel.MarketPrice;
                mapModel.sellPrice = productModel.SellPrice;
                mapModel.groupPrice = groupPrice;
                mapModel.weight = productModel.Weight;
                mapModel.stockQuantity = productModel.StockQuantity;
                mapModel.status = productModel.ShopGoods.Status == 0 ? (byte)0 : (byte)1;
            }
            result.Add(mapModel);
        }

        //根据字段进行塑形
        var resultDto = result.AsEnumerable(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/account/shop/cart
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Add([FromBody] ShopCartEditDto modelDto)
    {
        //保存数据
        var result = await this.AddAsync(modelDto);
        if (!result)
        {
            throw Oops.Oh("购物车商品更新失败");
        }
        return NoContent();
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/account/shop/cart/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update([FromRoute] long id, [FromQuery] int quantity)
    {
        //保存数据
        var result = await this.UpdateAsync(id, quantity);
        if (!result)
        {
            throw Oops.Oh("购物车商品更新失败");
        }
        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/account/shop/cart/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task Delete([FromRoute] long id)
    {
        if (!await _shopCartService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        await this.DeleteAsync(x=>x.Id == id);
    }

   

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/account/shop/cart?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
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
        await _shopCartService.DeleteAsync(x => arrIds.Contains(x.Id));
    }

    /// <summary>
    /// 获取购买商品列表
    /// 示例：/account/shop/cart/buy
    /// </summary>
    [HttpPost("buy")]
    [Authorize]
    public async Task<ShopCartTotalDto> GetBuyList([FromBody] List<ShopCartEditDto> listDto, [FromQuery] BaseParameter searchParam)
    {
        //检查参数是否正确
        if (listDto == null || searchParam.SiteId < 1)
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
        var memberModel = await _shopCartService.Context.Queryable<Members>()
            .Includes(x => x.Group)
            .FirstAsync(x => x.UserId == userId);
        if (memberModel == null || memberModel.Group == null)
        {
            throw Oops.Oh("会员不存在或已删除");
        }
        var buyLlist = listDto;
        //检查是否有商品
        if (buyLlist == null)
        {
            throw Oops.Oh("请选择购买的商品");
        }

        //获取商品列表
        var productList = await _shopCartService.Context.Queryable<ShopGoodsProduct>()
            .Where(x => buyLlist.Select(p => p.productId).Contains(x.Id))
            .Includes(x => x.ShopGoods)
            .Includes(x => x.GroupPrices).ToListAsync();
        if (productList == null)
        {
            throw Oops.Oh("未查询到商品信息");
        }
        //创建待赋值的实体
        ShopCartTotalDto result = new();
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
            ShopCartDto modelDto = new()
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
                weight = item.Weight,
                stockQuantity = item.StockQuantity,
                productId = item.Id,
                quantity = buyLlist.FirstOrDefault(x => x.productId == item.Id)?.quantity ?? 0
            };
            //非免运费商品标记
            if (item.ShopGoods.IsDeliveryFee == 0)
            {
                result.freeFreight = 0;
            }
            result.cartList.Add(modelDto);
        }
        //统计购物车属性
        result.totalQuantity = result.cartList.Select(x => x.quantity).Sum();
        result.totalWeight = result.cartList.Select(x => x.weight * x.quantity).Sum();
        result.payableAmount = result.cartList.Select(x => x.sellPrice * x.quantity).Sum();
        result.realAmount = result.cartList.Select(x => x.groupPrice * x.quantity).Sum();
        result.orderAmount = result.realAmount;

        //获取一条活动记录(区分站点)
        var promotionModel = await _shopCartService.Context.Queryable<ShopPromotion>()
            .Where(x => x.SiteId == 0 && x.SiteId == searchParam.SiteId && (SqlFunc.IsNullOrEmpty(x.GroupIds) || x.GroupIds.Contains($",{memberModel.GroupId},")))
            .Where(x => SqlFunc.Between(DateTime.Now, x.StartTime, x.EndTime))
            .FirstAsync();

        //计算有无活动满减折扣
        if (promotionModel != null)
        {
            //奖励阶梯满减
            if (promotionModel.AwardType == 1 && result.realAmount > promotionModel.Condition)
            {
                //计算阶梯满减倍数
                var discountNum = (int)(result.realAmount / promotionModel.Condition);
                //订单金额=原实付-阶梯满减金额
                result.promotionAmount = discountNum * promotionModel.AwardValue;
                result.orderAmount = result.realAmount - result.promotionAmount;

            }
            //奖励折扣
            else if (promotionModel.AwardType == 2 && result.realAmount > promotionModel.Condition)
            {
                if (promotionModel.AwardValue < 1 || promotionModel.AwardValue > 100)
                {
                    throw Oops.Oh($"活动{promotionModel.Id}折扣设置有误");
                }
                //订单金额=原实付*折扣
                result.orderAmount = result.realAmount * ((decimal)promotionModel.AwardValue / 100);
                result.promotionAmount = Math.Round(result.realAmount - result.orderAmount);
            }
            //促销活动信息
            result.promotion = promotionModel.Adapt<ShopPromotionDto>();
        }

        return result;
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取总记录数量
    /// 示例：/client/shop/cart/count
    /// </summary>
    [HttpGet("/client/shop/cart/count")]
    [NonUnify]
    public async Task<IActionResult> GetCount([FromQuery] BaseParameter searchParam)
    {
        //获取登录用户ID
        var userId = await  _userService.GetUserIdAsync();
        var result = await _shopCartService.Where(x => x.UserId == userId).CountAsync();
        //返回成功200
        return Ok(result);
    }
    #endregion



    #region Private Method============================
    /// <summary>
    /// 添加到购物车
    /// </summary>
    private async Task<bool> AddAsync(ShopCartEditDto modelDto, WriteRoRead writeAndRead = WriteRoRead.Write)
    {
        //获取当前用户信息
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("会员尚未登录或已超时").StatusCode(StatusCodes.Status401Unauthorized);
        }
        //查询商品货品信息
        var productModel = await _shopCartService.Context.Queryable<ShopGoodsProduct>()
            .Includes(x => x.ShopGoods)
            .Includes(x => x.GroupPrices)
            .FirstAsync(x => x.Id == modelDto.productId && SqlFunc.Exists(x.ShopGoods.Id) && x.ShopGoods.Status == 0);
        if (productModel == null || productModel.ShopGoods == null)
        {
            throw Oops.Oh($"商品已下架或已删除");
        }

        //检查是否已存在，不存在新增否则修改数量
        var cartModel = await _shopCartService.Context.Queryable<ShopCart>().FirstAsync(x => x.UserId == userId && x.ProductId == modelDto.productId);
        if (cartModel != null)
        {
            _shopCartService.Context.Tracking(cartModel);
            cartModel.Quantity += modelDto.quantity;
            //检查最小购买数量
            if (cartModel.Quantity < productModel.MinQuantity)
            {
                throw Oops.Oh($"该商品至少购买[{productModel.MinQuantity}]件");
            }
            return await _shopCartService.AutoUpdateAsync(cartModel) > 0;
        }
        else
        {
            //检查最小购买数量
            if (modelDto.quantity < productModel.MinQuantity)
            {
                throw Oops.Oh($"该商品至少购买[{productModel.MinQuantity}]件");
            }
            //查询商品信息保存到数据
            var model = new ShopCart
            {
                SiteId = productModel.ShopGoods.SiteId,
                UserId = userId,
                Title = productModel.ShopGoods.Title,
                SpecText = productModel.SpecText,
                ImgUrl = productModel.ImgUrl.IsNotEmptyOrNull() ? productModel.ImgUrl : productModel.ShopGoods.ImgUrl,
                ProductId = productModel.Id,
                Quantity = modelDto.quantity
            };
            return await _shopCartService.InsertAsync(model) > 0;
        }
    }
    /// <summary>
    /// 更新购物车数量
    /// </summary>
    private async Task<bool> UpdateAsync(long id, int quantity, WriteRoRead writeAndRead = WriteRoRead.Write)
    {
        //获取当前用户信息
        var userId =  await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("会员尚未登录或已超时").StatusCode(StatusCodes.Status401Unauthorized);
        }
        //检查是否已存在，不存在新增否则修改数量
        var model = await _shopCartService.Context.Queryable<ShopCart>().FirstAsync(x => x.UserId == userId && x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        _shopCartService.Context.Tracking(model);
        //查询商品货品信息
        var productModel = await _shopCartService.Context.Queryable<ShopGoodsProduct>()
                .Includes(x => x.ShopGoods)
                .Includes(x => x.GroupPrices)
                .FirstAsync(x => x.Id == model.ProductId && x.ShopGoods != null && x.ShopGoods.Status == 0);
        if (productModel == null || productModel.ShopGoods == null)
        {
            throw Oops.Oh($"商品已下架或已删除");
        }
        //检查最小购买数量
        if (quantity < productModel.MinQuantity)
        {
            throw Oops.Oh($"该商品至少购买[{productModel.MinQuantity}]件");
        }
        model.Quantity = quantity;
        return await _shopCartService.AutoUpdateAsync(model) > 0;
    }

    /// <summary>
    /// 删除购物车
    /// </summary>
    /// <param name="funcWhere"></param>
    /// <returns></returns>
    private async Task DeleteAsync(Expression<Func<ShopCart, bool>> funcWhere)
    {
        //获取当前用户信息
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("会员尚未登录或已超时").StatusCode(StatusCodes.Status401Unauthorized);
        }
        //查询购物车列表
        var list = await _shopCartService.AsQueryable().Where(x => x.UserId == userId).Where(funcWhere).ToListAsync();
        if (list == null)
        {
            return;
        }
        var ids = list.Select(x => x.Id).ToArray();
        await _shopCartService.DeleteAsync(x=> ids.Contains(x.Id));
    }
    #endregion
}