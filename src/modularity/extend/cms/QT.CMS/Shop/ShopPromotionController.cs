namespace QT.CMS;

/// <summary>
/// 促销活动
/// </summary>
[Route("api/cms/admin/shop/promotion")]
[ApiController]
public class ShopPromotionController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopPromotion> _shopPromotionService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ShopPromotionController(ISqlSugarRepository<ShopPromotion> shopPromotionService, IUserService userService)
    {
        _shopPromotionService = shopPromotionService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/shop/promotion/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopPromotion", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopPromotionDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopPromotionService.SingleAsync(x => x.Id == id) ?? throw Oops.Oh($"数据[{id}]不存在或已删除");
        var result = model.Adapt<ShopPromotionDto>();
        if (result.awardType == 4)
        {
            var couponModel = await _shopPromotionService.Context.Queryable<ShopCoupon>().SingleAsync(x => x.Id == result.awardValue);
            if (couponModel != null)
            {
                result.shopCoupon = couponModel.Adapt<ShopCouponDto>();
            }
        }
        if (result.awardType == 5)
        {
            var productModel = await _shopPromotionService.Context.Queryable<ShopGoodsProduct>()
                .Includes(x => x.ShopGoods,t=>t!.CategoryRelations, z=>z.ShopCategory)
                .SingleAsync(x => x.Id == result.awardValue);
            if (productModel != null)
            {
                result.goodsProduct = productModel.Adapt<ShopGoodsProductListDto>();
            }
        } 
         
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var resultExpando = result.Adapt<ShopPromotionDto>().ShapeData(param.Fields);
        return Ok(resultExpando);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/shop/promotion/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("ShopPromotion", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopPromotionDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopPromotionDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _shopPromotionService.AsQueryable()
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _shopPromotionService.QueryListAsync<ShopPromotion>(top,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopPromotionDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/shop/promotion?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("ShopPromotion", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopPromotionDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopPromotionDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopPromotionService.AsQueryable()
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);
        //var list = await _shopPromotionService.QueryPageAsync<ShopPromotion>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

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
        //var resultDto = _mapper.Adapt<IEnumerable<ShopPromotionDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopPromotionDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/shop/promotion
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("ShopPromotion", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] ShopPromotionEditDto modelDto)
    {
        if (!await _shopPromotionService.Context.Queryable<Sites>().AnyAsync(x => x.Id == modelDto.siteId))
        {
            throw Oops.Oh("站点不存在或已删除");
        }
        //映射成实体
        var model = modelDto.Adapt<ShopPromotion>();
        //获取用户名
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        await _shopPromotionService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<ShopPromotionDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/shop/promotion/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopPromotion", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] ShopPromotionEditDto modelDto)
    {
        if (!await _shopPromotionService.Context.Queryable<Sites>().AnyAsync(x => x.Id == modelDto.siteId))
        {
            throw Oops.Oh("站点不存在或已删除");
        }
        //查找记录
        var model = await _shopPromotionService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _shopPromotionService.Context.Tracking(model);
        //DTO映射到源数据
        modelDto.Adapt(model);
        //获取用户名
        model.UpdateBy = await _userService.GetUserNameAsync();
        model.UpdateTime = DateTime.Now;
        //调用保存
        await _shopPromotionService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/shop/promotion/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopPromotion", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<ShopPromotionEditDto> patchDocument)
    {
        var model = await _shopPromotionService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<ShopPromotionEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _shopPromotionService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _shopPromotionService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/shop/promotion/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopPromotion", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        if (!await _shopPromotionService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _shopPromotionService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/admin/shop/promotion?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("ShopPromotion", ActionType.Delete)]
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
        await _shopPromotionService.DeleteAsync(x => arrIds.Contains(x.Id));
        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/shop/promotion/view/10
    /// </summary>
    [HttpGet("/client/shop/promotion/view/{top}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> ClientGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopPromotionDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopPromotionDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _shopPromotionService.AsQueryable()
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .Where(x => SqlFunc.Between(DateTime.Now, x.StartTime, x.EndTime))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _shopPromotionService.QueryListAsync<ShopPromotion>(top,
        //    x => x.Status == 0
        //    && DateTime.Compare(x.StartTime, DateTime.Now) <= 0
        //    && DateTime.Compare(x.EndTime, DateTime.Now) >= 0
        //    && (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopPromotionDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }
    #endregion
}