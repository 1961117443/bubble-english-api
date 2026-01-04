namespace QT.CMS;

/// <summary>
/// 优惠券
/// </summary>
[Route("api/cms/admin/shop/coupon")]
[ApiController]
public class ShopCouponController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopCoupon> _shopCouponService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ShopCouponController(ISqlSugarRepository<ShopCoupon> shopCouponService,IUserService userService)
    {
        _shopCouponService = shopCouponService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/shop/coupon/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopCouponDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopCouponService.AsQueryable()
            .Includes(x=>x.GoodsRelations,t=>t.ShopGoods)
            .Includes(x=>x.CategoryRelations,t=>t.ShopCategory)
            .SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopCouponDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/shop/coupon/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopCouponDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopCouponDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        //var resultFrom = await _shopCouponService.QueryListAsync(top,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        var resultFrom = await _shopCouponService.AsQueryable()
            .Includes(x => x.GoodsRelations)
            .Includes(x => x.CategoryRelations)
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .AutoTake(top)
            .ToListAsync();

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopCouponDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/shop/coupon?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopCouponDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopCouponDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        //var list = await _shopCouponService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        var list = await _shopCouponService.AsQueryable()
           .Includes(x => x.GoodsRelations)
           .Includes(x => x.CategoryRelations)
           .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
           .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
           .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
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
        //var resultDto = _mapper.Adapt<IEnumerable<ShopCouponDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopCouponDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/shop/coupon
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.Add)]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> Add([FromBody] ShopCouponEditDto modelDto)
    {
        //保存数据
        var result = await this.AddAsync(modelDto);
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/shop/coupon/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.Edit)]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] ShopCouponEditDto modelDto)
    {
        //保存数据
        var result = await this.UpdateAsync(id, modelDto);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/shop/coupon/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<ShopCouponEditDto> patchDocument)
    {
        var model = await _shopCouponService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<ShopCouponEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _shopCouponService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _shopCouponService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/shop/coupon/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopCoupon", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        if (!await _shopCouponService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _shopCouponService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/shop/coupon?ids=1,2,3
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
        await _shopCouponService.DeleteAsync(x => arrIds.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/shop/coupon/view/10
    /// </summary>
    [HttpGet("/client/shop/coupon/view/{top}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IActionResult> ClientGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopCouponDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopCouponDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        //var resultFrom = await _shopCouponService.QueryListAsync(top,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && DateTime.Compare(x.EnableTime, DateTime.Now) <= 0
        //    && DateTime.Compare(x.EndTime, DateTime.Now) >= 0
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        var resultFrom = await _shopCouponService.AsQueryable()
            .Includes(x => x.GoodsRelations)
            .Includes(x => x.CategoryRelations)
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .Where(x => SqlFunc.Between(DateTime.Now, x.EnableTime, x.EndTime))
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .AutoTake(top)
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .ToListAsync();

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopCouponDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/client/shop/coupon?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/client/shop/coupon")]
    [CachingFilter]
    [NonUnify]
    public async Task<dynamic> ClientGetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopCouponDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopCouponDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        //var list = await _shopCouponService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //     x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && DateTime.Compare(x.EnableTime, DateTime.Now) <= 0
        //    && DateTime.Compare(x.EndTime, DateTime.Now) >= 0
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        var list = await _shopCouponService.AsQueryable()
            .Includes(x => x.GoodsRelations)
            .Includes(x => x.CategoryRelations)
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .Where(x => SqlFunc.Between(DateTime.Now, x.EnableTime, x.EndTime))
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
            .ToPagedListAsync(searchParam.currentPage,searchParam.pageSize);

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
        //var resultDto = list.Adapt<IEnumerable<ShopCouponDto>>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopCouponDto>.SqlSugarPageResult(list);
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 添加一条记录
    /// </summary>
    private async Task<ShopCouponDto> AddAsync(ShopCouponEditDto modelDto)
    {
        //检查类别或商品是否正确
        if (modelDto.categoryRelations != null)
        {
            var categoryIds = modelDto.categoryRelations.Select(x => x.categoryId);
            var categoryList = await _shopCouponService.Context.Queryable<ShopCategory>().Where(x => categoryIds.Contains(x.Id)).ToListAsync();
            if (categoryList != null && categoryIds.Except(categoryList.Select(x => x.Id)).Count() > 0)
            {
                throw Oops.Oh("类别列表有误，请检查后操作");
            }
        }
        if (modelDto.goodsRelations != null)
        {
            var goodsIds = modelDto.goodsRelations.Select(x => x.goodsId);
            var goodsList = await _shopCouponService.Context.Queryable<ShopGoods>().Where(x => goodsIds.Contains(x.Id)).ToListAsync();
            if (goodsList != null && goodsIds.Except(goodsList.Select(x => x.Id)).Count() > 0)
            {
                throw Oops.Oh("商品列表有误，请检查后操作");
            }
        }

        //映射成实体
        var model = modelDto.Adapt<ShopCoupon>();
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //保存
        var modellt = await _shopCouponService.Context.InsertNav(model)
            .Include(x=>x.GoodsRelations)
            .Include(x=>x.CategoryRelations)
            .ExecuteReturnEntityAsync();

        //映射成DTO
        return modellt.Adapt<ShopCouponDto>();
    }

    /// <summary>
    /// 修改一条记录
    /// </summary>
    private async Task<bool> UpdateAsync(long id, ShopCouponEditDto modelDto)
    {
        //检查类别或商品是否正确
        if (modelDto.categoryRelations != null)
        {
            var categoryIds = modelDto.categoryRelations.Select(x => x.categoryId);
            var categoryList = await _shopCouponService.Context.Queryable<ShopCategory>().Where(x => categoryIds.Contains(x.Id)).ToListAsync();
            if (categoryList != null && categoryIds.Except(categoryList.Select(x => x.Id)).Count() > 0)
            {
                throw Oops.Oh("类别列表有误，请检查后操作");
            }
        }
        if (modelDto.goodsRelations != null)
        {
            var goodsIds = modelDto.goodsRelations.Select(x => x.goodsId);
            var goodsList = await _shopCouponService.Context.Queryable<ShopGoods>().Where(x => goodsIds.Contains(x.Id)).ToListAsync();
            if (goodsList != null && goodsIds.Except(goodsList.Select(x => x.Id)).Count() > 0)
            {
                throw Oops.Oh("商品列表有误，请检查后操作");
            }
        }

        //根据ID获取记录
        //var model = await _shopCouponService.Context.Queryable<ShopCoupon>().Where(x => x.Id == id)
        //    .Includes(x => x.CategoryRelations)
        //    .Includes(x => x.GoodsRelations)
        //    .FirstAsync();

        var model = await _shopCouponService.SingleAsync(x => x.Id == id);
        //如果不存在则抛出异常
        if (model == null)
        {
            throw Oops.Oh("数据不存在或已删除");
        }
        _shopCouponService.Context.Tracking(model);
        //将DTO映射到源数据
        var mapModel = modelDto.Adapt(model);
        //设置更新人和更新时间
        mapModel.UpdateBy = await _userService.GetUserNameAsync();
        mapModel.UpdateTime = DateTime.Now;

        var changes = _shopCouponService.Context.GetChanges(model);
        _shopCouponService.Context.ClearTracking();
        var rootOption = new UpdateNavRootOptions { };
        if (changes.IsAny())
        {
            rootOption.UpdateColumns = changes.ToArray();
        }
        //保存
        var opt = new UpdateNavOptions { OneToManyInsertOrUpdate = true};
        return await _shopCouponService.Context.UpdateNav(model, rootOption)
            .Include(x => x.GoodsRelations, opt)
            .Include(x => x.CategoryRelations,opt)
            .ExecuteCommandAsync();
    } 
    #endregion
}