using Polly;
using QT.CMS.Emum;
using SkiaSharp;
using System.Linq.Expressions;

namespace QT.CMS;

/// <summary>
/// 商品信息
/// </summary>
[Route("api/cms/admin/shop/goods")]
[ApiController]
public class ShopGoodsController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopCategory> _shopCategoryService;
    private readonly ISqlSugarRepository<ShopGoods> _shopGoodsService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ShopGoodsController(ISqlSugarRepository<ShopCategory> shopCategoryService, ISqlSugarRepository<ShopGoods> shopGoodsService, IUserService userService)
    {
        _shopCategoryService = shopCategoryService;
        _shopGoodsService = shopGoodsService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/shop/goods/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopGoods", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopGoodsDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopGoodsService.AsQueryable()
                .Includes(x => x.GoodsProducts, t => t.GroupPrices)
                .Includes(x => x.GoodsAlbums)
                .Includes(x => x.FieldValues)
                .Includes(x => x.LabelRelations, t => t.ShopLabel)
                .Includes(x => x.CategoryRelations, t => t.ShopCategory)
                .Includes(x => x.GoodsSpecs)
                .Includes(x => x.Brand)
                .FirstAsync(x => x.Id == id);
        //var model = await _shopGoodsService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        if (model.GoodsSpecs.IsAny())
        {
            model.GoodsSpecs = model.GoodsSpecs.OrderBy(x => x.SpecId).ToList();
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopGoodsDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取总记录数量
    /// 示例：/admin/shop/goods/view/count
    /// </summary>
    [HttpGet("view/count")]
    [Authorize]
    [AuthorizeFilter("ShopGoods", ActionType.View)]
    public async Task<IActionResult> GetCount([FromQuery] GoodsParameter searchParam)
    {
        var result = await _shopGoodsService.AsQueryable()
            .WhereIF(searchParam.Status > 0, x => x.Status == searchParam.Status)
            .CountAsync();
        //返回成功200
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/shop/goods/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("ShopGoods", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] GoodsParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopGoodsDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopGoodsDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _shopGoodsService.AsQueryable()
                .Includes(x => x.GoodsProducts)
                .Includes(x => x.GoodsAlbums)
                //.Includes(x => x.FieldValues)
                .Includes(x => x.LabelRelations, t => t.ShopLabel)
                .Includes(x => x.CategoryRelations, t => t.ShopCategory)
                //.Includes(x => x.GoodsSpecs)
                .Includes(x => x.Brand)
                .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
                .WhereIF(searchParam.CategoryId > 0, x => x.CategoryRelations.Any(c => c.CategoryId == searchParam.CategoryId))
                .WhereIF(searchParam.LabelId > 0, x => x.LabelRelations.Any(r => r.LabelId == searchParam.LabelId))
                .WhereIF(searchParam.BrandId > 0, x => x.BrandId == searchParam.BrandId)
                .WhereIF(searchParam.Status >= 0, x => x.Status == searchParam.Status)
                .WhereIF(searchParam.MinPrice > 0, x => x.SellPrice >= searchParam.MinPrice)
                .WhereIF(searchParam.MaxPrice > 0, x => x.SellPrice <= searchParam.MaxPrice)
                .WhereIF(searchParam.StartTime.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartTime.Value))
                .WhereIF(searchParam.EndTime.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndTime.Value))
                .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.GoodsNo.Contains(searchParam.Keyword) || x.Title.Contains(searchParam.Keyword))
                .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
                .AutoTake(top)
                .ToListAsync();

        //var resultFrom = await _shopGoodsService.QueryListAsync(top,
        //     x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (searchParam.CategoryId <= 0 || x.CategoryRelations.Any(c => c.CategoryId == searchParam.CategoryId))
        //    && (searchParam.LabelId <= 0 || x.LabelRelations.Any(r => r.LabelId == searchParam.LabelId))
        //    && (searchParam.BrandId <= 0 || x.BrandId == searchParam.BrandId)
        //    && (searchParam.Status < 0 || x.Status == searchParam.Status)
        //    && (searchParam.MinPrice <= 0 || x.SellPrice >= searchParam.MinPrice)
        //    && (searchParam.MaxPrice <= 0 || x.SellPrice <= searchParam.MaxPrice)
        //    && (searchParam.StartTime == null || DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndTime == null || DateTime.Compare(x.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.GoodsNo != null && x.GoodsNo.Contains(searchParam.Keyword)) || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopGoodsDto>>().ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/shop/goods?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("ShopGoods", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] GoodsParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopGoodsDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopGoodsDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopGoodsService.AsQueryable()
          .Includes(x => x.GoodsProducts)
          .Includes(x => x.GoodsAlbums)
          //.Includes(x => x.FieldValues)
          .Includes(x => x.LabelRelations, t => t.ShopLabel)
          .Includes(x => x.CategoryRelations, t => t.ShopCategory)
          //.Includes(x => x.GoodsSpecs)
          .Includes(x => x.Brand)
          .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
          .WhereIF(searchParam.CategoryId > 0, x => x.CategoryRelations.Any(c => c.CategoryId == searchParam.CategoryId))
          .WhereIF(searchParam.LabelId > 0, x => x.LabelRelations.Any(r => r.LabelId == searchParam.LabelId))
          .WhereIF(searchParam.BrandId > 0, x => x.BrandId == searchParam.BrandId)
          .WhereIF(searchParam.Status >= 0, x => x.Status == searchParam.Status)
          .WhereIF(searchParam.MinPrice > 0, x => x.SellPrice >= searchParam.MinPrice)
          .WhereIF(searchParam.MaxPrice > 0, x => x.SellPrice <= searchParam.MaxPrice)
          .WhereIF(searchParam.StartTime.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartTime.Value))
          .WhereIF(searchParam.EndTime.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndTime.Value.ToString("yyyy-MM-dd 23:59:59")))
          .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.GoodsNo.Contains(searchParam.Keyword) || x.Title.Contains(searchParam.Keyword))
          .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
          .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        //var list = await _shopGoodsService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (searchParam.CategoryId <= 0 || x.CategoryRelations.Any(c => c.CategoryId == searchParam.CategoryId))
        //    && (searchParam.LabelId <= 0 || x.LabelRelations.Any(r => r.LabelId == searchParam.LabelId))
        //    && (searchParam.BrandId <= 0 || x.BrandId == searchParam.BrandId)
        //    && (searchParam.Status < 0 || x.Status == searchParam.Status)
        //    && (searchParam.MinPrice <= 0 || x.SellPrice >= searchParam.MinPrice)
        //    && (searchParam.MaxPrice <= 0 || x.SellPrice <= searchParam.MaxPrice)
        //    && (searchParam.StartTime == null || DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndTime == null || DateTime.Compare(x.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.GoodsNo != null && x.GoodsNo.Contains(searchParam.Keyword)) || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
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
        //var resultDto = _mapper.Adapt<IEnumerable<ShopGoodsDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopGoodsDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 获取货品分页列表
    /// 示例：/admin/shop/goods/product?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("product")]
    [Authorize]
    [AuthorizeFilter("ShopGoods", ActionType.View)]
    public async Task<dynamic> GetProductList([FromQuery] GoodsParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopGoodsProductListDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopGoodsProductListDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopGoodsService.Context.Queryable<ShopGoodsProduct>()
               .Includes(x => x.ShopGoods, t => t.CategoryRelations, z => z.ShopCategory)
               .WhereIF(searchParam.SiteId > 0, x => x.ShopGoods.SiteId == searchParam.SiteId)
               .WhereIF(searchParam.BrandId > 0, x => x.ShopGoods.BrandId == searchParam.BrandId)
               .WhereIF(searchParam.Status >= 0, x => x.ShopGoods.Status == searchParam.Status)
               .WhereIF(searchParam.StartTime.HasValue, x => SqlFunc.GreaterThanOrEqual(x.ShopGoods.AddTime, searchParam.StartTime.Value))
               .WhereIF(searchParam.EndTime.HasValue, x => SqlFunc.LessThanOrEqual(x.ShopGoods.AddTime, searchParam.EndTime.Value))
               .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.GoodsNo.Contains(searchParam.keyword) || x.ShopGoods.Title.Contains(searchParam.keyword))
               .OrderBy(searchParam.OrderBy ?? "GoodsId,-Id")
               .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        //var list = await _shopGoodsService.QueryProductPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.ShopGoods != null
        //    && (searchParam.SiteId <= 0 || x.ShopGoods.SiteId == searchParam.SiteId)
        //    && (searchParam.BrandId <= 0 || x.ShopGoods.BrandId == searchParam.BrandId)
        //    && (searchParam.Status < 0 || x.ShopGoods.Status == searchParam.Status)
        //    && (searchParam.StartTime == null || DateTime.Compare(x.ShopGoods.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndTime == null || DateTime.Compare(x.ShopGoods.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.GoodsNo != null && x.GoodsNo.Contains(searchParam.Keyword)) || (x.ShopGoods.Title != null && x.ShopGoods.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "GoodsId,-Id");

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
        //var resultDto = _mapper.Adapt<IEnumerable<ShopGoodsProductListDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopGoodsProductListDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/shop/goods
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("ShopGoods", ActionType.Add)]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> Add([FromBody] ShopGoodsEditDto modelDto)
    {
        var result = await this.AddAsync(modelDto);
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/shop/goods/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopGoods", ActionType.Edit)]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] ShopGoodsEditDto modelDto)
    {
        //检查数据是否存在
        if (!await _shopGoodsService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //调用保存方法
        var result = await this.UpdateAsync(id, modelDto);
        return NoContent();
    }

    /// <summary>
    /// 批量审核记录
    /// 示例：/admin/shop/goods?ids=1,2,3
    /// </summary>
    [HttpPut]
    [Authorize]
    [AuthorizeFilter("ShopGoods", ActionType.Audit)]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> AuditByIds([FromQuery] string Ids)
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
        var list = await _shopGoodsService.Where(x => x.Status == 2 && arrIds.Contains(x.Id)).Take(1000).ToListAsync();
        if (list == null || list.Count() == 0)
        {
            throw Oops.Oh("没有找到需要审核的记录");
        }
        foreach (var item in list)
        {
            item.IsLock = 0;
        }
        //保存到数据库
        await _shopGoodsService.Context.Updateable<ShopGoods>(list).UpdateColumns(x => x.IsLock).ExecuteCommandAsync();

        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/shop/goods/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopGoods", ActionType.Edit)]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<ShopGoodsEditDto> patchDocument)
    {
        var model = await _shopGoodsService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<ShopGoodsEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _shopGoodsService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _shopGoodsService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/shop/goods/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopGoods", ActionType.Delete)]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        if (!await _shopGoodsService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await this.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/shop/goods?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("ShopGoods", ActionType.Delete)]
    [SqlSugarUnitOfWork]
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
        await this.DeleteAsync(x => arrIds.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/client/shop/goods/1
    /// </summary>
    [HttpGet("/client/shop/goods/{id}")]
    [CachingFilter]
    [AllowAnonymous, NonUnify]
    public async Task<IActionResult> ClientGetById([FromRoute] long id, [FromQuery] BaseParameter searchParam)
    {
        if (!searchParam.Fields.IsPropertyExists<ShopGoodsClientDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopGoodsService.AsQueryable()
                .Includes(x => x.GoodsProducts, t => t.GroupPrices)
                .Includes(x => x.GoodsAlbums)
                .Includes(x => x.FieldValues)
                .Includes(x => x.LabelRelations, t => t.ShopLabel)
                .Includes(x => x.CategoryRelations, t => t.ShopCategory)
                .Includes(x => x.GoodsSpecs)
                .Includes(x => x.Brand)
                .FirstAsync(x => x.Id == id);
        //var model = await _shopGoodsService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        if (model.GoodsSpecs.IsAny())
        {
            model.GoodsSpecs = model.GoodsSpecs.OrderBy(x => x.SpecId).ToList();
        }

        //浏览次数加一
        model.Click++;
        //保存到数据库
        await this.UpdateClickAsync(model);
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopGoodsClientDto>().ShapeData(searchParam.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/shop/goods/view/10
    /// </summary>
    [HttpGet("/client/shop/goods/view/{top}")]
    [CachingFilter]
    [AllowAnonymous, NonUnify]
    public async Task<dynamic> ClientGetList([FromRoute] int top, [FromQuery] GoodsParameter searchParam)
    {
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopGoodsDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopGoodsClientDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _shopGoodsService.AsQueryable()
              .Includes(x => x.GoodsProducts)//.ThenInclude(x => x.GroupPrices)
              .Includes(x => x.GoodsAlbums)
              //.Include(x => x.FieldValues)
              .Includes(x => x.LabelRelations, t => t.ShopLabel)
              .Includes(x => x.CategoryRelations, t => t.ShopCategory)
              //.Include(x => x.GoodsSpecs)
              .Includes(x => x.Brand)
              .Where(x => x.IsLock == 0)
              .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
              .WhereIF(searchParam.CategoryId > 0, x => x.SiteId == searchParam.SiteId)
              .WhereIF(searchParam.BrandId > 0, x => x.BrandId == searchParam.BrandId)
              .WhereIF(searchParam.Status >= 0, x => x.Status == searchParam.Status)
              .WhereIF(searchParam.MinPrice > 0, x => x.SellPrice >= searchParam.MinPrice)
              .WhereIF(searchParam.MaxPrice > 0, x => x.SellPrice <= searchParam.MaxPrice)
              .WhereIF(searchParam.StartTime.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartTime.Value))
              .WhereIF(searchParam.EndTime.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndTime.Value))
              .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.GoodsNo.Contains(searchParam.keyword) || x.Title.Contains(searchParam.keyword))
              .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
              .AutoTake(top)
              .ToListAsync();

        //var resultFrom = await _shopGoodsService.QueryListAsync(top,
        //    x => x.IsLock == 0
        //    && (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (searchParam.CategoryId <= 0 || x.CategoryRelations.Any(c => c.CategoryId == searchParam.CategoryId))
        //    && (searchParam.LabelId <= 0 || x.LabelRelations.Any(r => r.LabelId == searchParam.LabelId))
        //    && (searchParam.BrandId <= 0 || x.BrandId == searchParam.BrandId)
        //    && (searchParam.Status < 0 || x.Status == searchParam.Status)
        //    && (searchParam.MinPrice <= 0 || x.SellPrice >= searchParam.MinPrice)
        //    && (searchParam.MaxPrice <= 0 || x.SellPrice <= searchParam.MaxPrice)
        //    && (searchParam.StartTime == null || DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndTime == null || DateTime.Compare(x.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.GoodsNo != null && x.GoodsNo.Contains(searchParam.Keyword)) || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopGoodsClientDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/client/shop/goods?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/client/shop/goods")]
    [CachingFilter]
    [AllowAnonymous, NonUnify]
    public async Task<dynamic> ClientGetList([FromQuery] GoodsParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopGoodsDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopGoodsClientDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopGoodsService.AsQueryable()
             .Includes(x => x.GoodsProducts)//.ThenInclude(x => x.GroupPrices)
             .Includes(x => x.GoodsAlbums)
             //.Include(x => x.FieldValues)
             .Includes(x => x.LabelRelations, t => t.ShopLabel)
             .Includes(x => x.CategoryRelations, t => t.ShopCategory)
             //.Include(x => x.GoodsSpecs)
             .Includes(x => x.Brand)
             .Where(x => x.IsLock == 0)
             .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
             .WhereIF(searchParam.CategoryId > 0, x => x.SiteId == searchParam.SiteId)
             .WhereIF(searchParam.BrandId > 0, x => x.BrandId == searchParam.BrandId)
             .WhereIF(searchParam.Status >= 0, x => x.Status == searchParam.Status)
             .WhereIF(searchParam.MinPrice > 0, x => x.SellPrice >= searchParam.MinPrice)
             .WhereIF(searchParam.MaxPrice > 0, x => x.SellPrice <= searchParam.MaxPrice)
             .WhereIF(searchParam.StartTime.HasValue, x => SqlFunc.GreaterThanOrEqual(x.AddTime, searchParam.StartTime.Value))
             .WhereIF(searchParam.EndTime.HasValue, x => SqlFunc.LessThanOrEqual(x.AddTime, searchParam.EndTime.Value))
             .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.GoodsNo.Contains(searchParam.keyword) || x.Title.Contains(searchParam.keyword))
             .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
             .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        //var list = await _shopGoodsService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.IsLock == 0
        //    && (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (searchParam.CategoryId <= 0 || x.CategoryRelations.Any(c => c.CategoryId == searchParam.CategoryId))
        //    && (searchParam.LabelId <= 0 || x.LabelRelations.Any(r => r.LabelId == searchParam.LabelId))
        //    && (searchParam.BrandId <= 0 || x.BrandId == searchParam.BrandId)
        //    && (searchParam.Status < 0 || x.Status == searchParam.Status)
        //    && (searchParam.MinPrice <= 0 || x.SellPrice >= searchParam.MinPrice)
        //    && (searchParam.MaxPrice <= 0 || x.SellPrice <= searchParam.MaxPrice)
        //    && (searchParam.StartTime == null || DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
        //    && (searchParam.EndTime == null || DateTime.Compare(x.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.GoodsNo != null && x.GoodsNo.Contains(searchParam.Keyword)) || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
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
        //var resultDto = _mapper.Adapt<IEnumerable<ShopGoodsClientDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopGoodsClientDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 获取分类商品指定数量列表
    /// 示例：/client/shop/goods/view/10/2
    /// </summary>
    [HttpGet("/client/shop/goods/view/{categoryTop}/{goodsTop}")]
    [CachingFilter]
    [AllowAnonymous, NonUnify]
    public async Task<IActionResult> ClientGetCategoryList([FromRoute] int categoryTop, int goodsTop, [FromQuery] GoodsParameter searchParam)
    {
        if (!searchParam.Fields.IsPropertyExists<ShopCategoryGoodsClientDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取前几条分类,最多获取前10个分类
        if (categoryTop > 10) categoryTop = 10;
        if (goodsTop > 10) goodsTop = 10;

        var result = await this.QueryGoodsListAsync(categoryTop, goodsTop, 0, x => x.IsLock == 0 && x.Status == 0, searchParam.OrderBy ?? "SortId,Id");
        return Ok(result);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 添加一条记录
    /// </summary>
    private async Task<ShopGoodsDto> AddAsync(ShopGoodsEditDto modelDto)
    {
        //检查站点是否正确
        if (!await _shopGoodsService.Context.Queryable<Sites>().AnyAsync(x => x.Id == modelDto.siteId))
        {
            throw Oops.Oh("站点不存在或已删除");
        }
        //检查品牌是否正确
        if (!await _shopGoodsService.Context.Queryable<ShopBrand>().AnyAsync(x => x.Id == modelDto.brandId))
        {
            throw Oops.Oh("品牌不存在或已删除");
        }
        //检查扩展属性模型是否正确
        if (modelDto.fieldId > 0 && !await _shopGoodsService.Context.Queryable<ShopField>().AnyAsync(x => x.Id == modelDto.fieldId))
        {
            throw Oops.Oh("扩展属性不存在或已删除");
        }
        //检查类别是否正确
        if (modelDto.categoryRelations != null)
        {
            var categoryIds = modelDto.categoryRelations.Select(x => x.categoryId);
            var categoryList = await _shopCategoryService.Where(x => categoryIds.Contains(x.Id)).ToListAsync();
            if (categoryList != null && categoryIds.Except(categoryList.Select(x => x.Id)).Count() > 0)
            {
                throw Oops.Oh("类别列表有误，请检查后操作");
            }
        }
        //检查标签是否正确
        if (modelDto.labelRelations != null)
        {
            var labelIds = modelDto.labelRelations.Select(x => x.labelId);
            var labelList = await _shopCategoryService.Context.Queryable<ShopLabel>().Where(x => labelIds.Contains(x.Id)).ToListAsync();
            if (labelList != null && labelIds.Except(labelList.Select(x => x.Id)).Count() > 0)
            {
                throw Oops.Oh("标签列表有误，请检查后操作");
            }
        }
        //检查商品规格是否正确
        if (modelDto.goodsSpecs != null)
        {
            var specIds = modelDto.goodsSpecs.Select(x => x.specId);
            var specList = await _shopCategoryService.Context.Queryable<ShopSpec>().Where(x => specIds.Contains(x.Id)).ToListAsync();
            if (specList != null && specIds.Except(specList.Select(x => x.Id)).Count() > 0)
            {
                throw Oops.Oh("商品规格列表有误，请检查后操作");
            }
        }
        //获取用户信息
        var userId = await _userService.GetUserIdAsync();
        var manageInfo = await _shopCategoryService.Context.Queryable<Manager>().SingleAsync(x => x.UserId == userId);
        if (manageInfo == null)
        {
            throw Oops.Oh("管理员身份有误，请核实后操作");
        }

        //映射成实体
        var model = modelDto.Adapt<ShopGoods>();
        //记录当前用户名
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //是否需要审核
        if (manageInfo.IsAudit > 0)
        {
            model.IsLock = 1;
        }
        //上架或下架时间
        if (model.Status == 0)
        {
            model.UpTime = DateTime.Now;
        }

        //保存商品信息
        await _shopGoodsService.Context.InsertNav(model)
                                    .Include(x => x.GoodsProducts)
                                    .Include(x => x.GoodsAlbums)
                                    .Include(x => x.FieldValues)
                                    .Include(x => x.LabelRelations)
                                    .Include(x => x.CategoryRelations)
                                    .Include(x => x.GoodsSpecs)
                                    .ExecuteCommandAsync();

        //保存分类规格筛选,用于查询
        if (model.CategoryRelations != null && model.GoodsSpecs != null)
        {
            List<ShopCategorySpec> specList = new();
            foreach (var cmodelt in model.CategoryRelations)
            {
                foreach (var smodelt in model.GoodsSpecs.Where(x => x.ParentId == 0))
                {
                    ShopCategorySpec modelt = new()
                    {
                        CategoryId = cmodelt.CategoryId,
                        SpecId = smodelt.Id,
                        GoodsId = model.Id
                    };
                    //await _context.Set<ShopCategorySpec>().AddAsync(modelt);

                    specList.Add(modelt);
                }
            }

            if (specList.IsAny())
            {
                await _shopGoodsService.Context.Insertable<ShopCategorySpec>(specList).ExecuteCommandAsync();
            }
        }

        //映射成DTO
        var result = model.Adapt<ShopGoodsDto>();
        return result;
    }

    /// <summary>
    /// 修改一条记录
    /// </summary>
    private async Task<bool> UpdateAsync(long id, ShopGoodsEditDto modelDto)
    {
        //检查站点是否正确
        if (!await _shopGoodsService.Context.Queryable<Sites>().AnyAsync(x => x.Id == modelDto.siteId))
        {
            throw Oops.Oh("站点不存在或已删除");
        }
        //检查品牌是否正确
        if (!await _shopGoodsService.Context.Queryable<ShopBrand>().AnyAsync(x => x.Id == modelDto.brandId))
        {
            throw Oops.Oh("品牌不存在或已删除");
        }
        //检查扩展属性模型是否正确
        if (modelDto.fieldId > 0 && !await _shopGoodsService.Context.Queryable<ShopField>().AnyAsync(x => x.Id == modelDto.fieldId))
        {
            throw Oops.Oh("扩展属性不存在或已删除");
        }
        //检查类别是否正确
        if (modelDto.categoryRelations != null)
        {
            var categoryIds = modelDto.categoryRelations.Select(x => x.categoryId);
            var categoryList = await _shopGoodsService.Context.Queryable<ShopCategory>().Where(x => categoryIds.Contains(x.Id)).ToListAsync();
            if (categoryList != null && categoryIds.Except(categoryList.Select(x => x.Id)).Count() > 0)
            {
                throw Oops.Oh("类别列表有误，请检查后操作");
            }
        }
        //检查标签是否正确
        if (modelDto.labelRelations != null)
        {
            var labelIds = modelDto.labelRelations.Select(x => x.labelId);
            var labelList = await _shopGoodsService.Context.Queryable<ShopLabel>().Where(x => labelIds.Contains(x.Id)).ToListAsync();
            if (labelList != null && labelIds.Except(labelList.Select(x => x.Id)).Count() > 0)
            {
                throw Oops.Oh("标签列表有误，请检查后操作");
            }
        }
        //检查商品规格是否正确
        if (modelDto.goodsSpecs != null)
        {
            var specIds = modelDto.goodsSpecs.Select(x => x.specId);
            var specList = await _shopGoodsService.Context.Queryable<ShopSpec>().Where(x => specIds.Contains(x.Id)).ToListAsync();
            if (specList != null && specIds.Except(specList.Select(x => x.Id)).Count() > 0)
            {
                throw Oops.Oh("商品规格列表有误，请检查后操作");
            }
        }
        //获取用户信息
        //var userInfo = await _userService.GetUserAsync();
        var userId = await _userService.GetUserIdAsync();
        var manageInfo = await _shopGoodsService.Context.Queryable<Manager>().SingleAsync(x => x.UserId == userId);
        if (manageInfo == null)
        {
            throw Oops.Oh("管理员身份有误，请核实后操作");
        }

        //根据ID获取记录
        var model = await _shopGoodsService.AsQueryable()
            .Includes(x => x.GoodsProducts, t => t.GroupPrices)
            .Includes(x => x.GoodsAlbums)
            .Includes(x => x.FieldValues)
            .Includes(x => x.LabelRelations)
            .Includes(x => x.CategoryRelations)
            .Includes(x => x.GoodsSpecs)
            .Includes(x => x.Brand)
             .Where(x => x.Id == id)
            .FirstAsync();
        //如果不存在则抛出异常
        if (model == null)
        {
            throw Oops.Oh("商品不存在或已删除");
        }


        //_shopGoodsService.Context.Tracking(model);

        //上架或下架时间
        if (model.Status != modelDto.status)
        {
            if (modelDto.status == 0)
            {
                model.UpTime = DateTime.Now;
            }
        }
        //是否需要审核
        if (manageInfo.IsAudit > 0)
        {
            model.IsLock = 1;
        }
        //设置更新人和更新时间
        model.UpdateBy = await _userService.GetUserNameAsync();
        model.UpdateTime = DateTime.Now;

        modelDto.Adapt(model); //AutoMapper将DTO映射到源数据


        {
            //先查找分类规格筛选并删除
            var specList = await _shopGoodsService.Context.Queryable<ShopCategorySpec>().Where(x => x.GoodsId == model.Id).ToListAsync();
            if (specList != null)
            {
                await _shopGoodsService.Context.Deleteable<ShopCategorySpec>(specList).ExecuteCommandAsync();
            }
            //保存商品信息
            //await _shopGoodsService.AutoUpdateAsync(model);
            var navOptions = new UpdateNavOptions { OneToManyInsertOrUpdate = true };
            await _shopCategoryService.Context.UpdateNav(model)
                                            .Include(x => x.GoodsProducts, navOptions)
                                            .Include(x => x.GoodsAlbums, navOptions)
                                            .Include(x => x.FieldValues, navOptions)
                                            .Include(x => x.LabelRelations, navOptions)
                                            .Include(x => x.CategoryRelations, navOptions)
                                            .Include(x => x.GoodsSpecs, navOptions)
                                            .ExecuteCommandAsync();
        }


        //重新添加分类规格筛选,用于查询
        if (model.CategoryRelations != null && model.GoodsSpecs != null)
        {
            List<ShopCategorySpec> specList = new();
            foreach (var cmodelt in model.CategoryRelations)
            {
                foreach (var smodelt in model.GoodsSpecs.Where(x => x.ParentId == 0))
                {
                    ShopCategorySpec modelt = new ShopCategorySpec()
                    {
                        CategoryId = cmodelt.CategoryId,
                        SpecId = smodelt.Id,
                        GoodsId = model.Id
                    };
                    specList.Add(modelt);
                }
            }

            if (specList.IsAny())
            {
                await _shopGoodsService.Context.Insertable<ShopCategorySpec>(specList).ExecuteCommandAsync();
            }
        }


        return true;
    }

    /// <summary>
    /// 根据条件删除一条记录
    /// </summary>
    private async Task<bool> DeleteAsync(Expression<Func<ShopGoods, bool>> funcWhere)
    {
        //删除商品主从表
        var list = await _shopGoodsService.AsQueryable().ToListAsync();

        if (list == null)
        {
            return false;
        }

        var goodsIdList = list.Select(x => x.Id).ToArray();

        await _shopGoodsService.Context.Deleteable<ShopCouponGoodsRelation>(x => goodsIdList.Contains(x.GoodsId)).ExecuteCommandAsync();
        await _shopGoodsService.Context.Deleteable<ShopCategorySpec>(x => goodsIdList.Contains(x.GoodsId)).ExecuteCommandAsync();
        await _shopGoodsService.Context.Deleteable<ShopGoods>(list).ExecuteCommandAsync();

        return true;
    }

    /// <summary>
    /// 更新浏览量(局部更新)
    /// </summary>
    private async Task<int> UpdateClickAsync(ShopGoods model)
    {
        //提交保存
        bool result = await _shopGoodsService.Context.Updateable<ShopGoods>(model).UpdateColumns(x => x.Click).ExecuteCommandAsync() > 0;
        if (result)
        {
            return model.Click;
        }
        return 0;
    }


    /// <summary>
    /// 根据父ID返回一级列表(带商品)
    /// </summary>
    private async Task<IEnumerable<ShopCategoryGoodsClientDto>> QueryGoodsListAsync(int categoryTop, int goodsTop, int status,
        Expression<Func<ShopGoods, bool>> funcWhere, string orderBy, WriteRoRead writeAndRead = WriteRoRead.Read)
    {
        var list = await _shopCategoryService.Context.Queryable<ShopCategory>()
            .Where(x => x.ParentId == 0 && (status == -1 || x.Status == status))
            .OrderBy(orderBy)
            .Take(categoryTop)
            .Select(c => new ShopCategoryGoodsClientDto
            {
                id = c.Id,
                title = c.Title,
                imgUrl = c.ImgUrl
            })
            .ToListAsync();

        await _shopCategoryService.Context.ThenMapperAsync(list, async c =>
        {
            c.goodsList = await _shopGoodsService.AsQueryable()
                .Includes(x => x.CategoryRelations)
                .Where(x => x.CategoryRelations.Any(r => r.CategoryId == c.id))
                .Where(funcWhere)
                    .OrderBy(x => x.SortId)
                    .OrderByDescending(x => x.Id)
                    .Take(goodsTop)
                    .Select(x => new ShopGoodsClientDto
                    {
                        id = x.Id,
                        title = x.Title,
                        zhaiyao = x.Zhaiyao,
                        imgUrl = x.ImgUrl,
                        marketPrice = x.MarketPrice,
                        sellPrice = x.SellPrice
                    })
                    .ToListAsync();
        });

        return list;

        ////构造查询表达式
        //var query = _shopCategoryService.Context.Queryable<ShopCategory>()
        //    .Where(x => x.ParentId == 0 && (status == -1 || x.Status == status))
        //    .OrderBy(orderBy)
        //    .Take(categoryTop)
        //    .Select(c => new
        //    {
        //        Category = c,
        //        ShopGoods = _context.Set<ShopGoods>()
        //            .Include(x => x.CategoryRelations)
        //            .Where(x => x.CategoryRelations.Any(r => r.CategoryId == c.Id))
        //            .Where(funcWhere)
        //            .OrderBy(x => x.SortId)
        //            .OrderByDescending(x => x.Id)
        //            .Take(goodsTop)
        //            .ToList()
        //    })
        //    .Select(cg => new ShopCategoryGoodsClientDto
        //    {
        //        Id = cg.Category.Id,
        //        Title = cg.Category.Title,
        //        ImgUrl = cg.Category.ImgUrl,
        //        GoodsList = cg.ShopGoods.Select(x => new ShopGoodsClientDto
        //        {
        //            Id = x.Id,
        //            Title = x.Title,
        //            Zhaiyao = x.Zhaiyao,
        //            ImgUrl = x.ImgUrl,
        //            MarketPrice = x.MarketPrice,
        //            SellPrice = x.SellPrice
        //        })
        //    });

        //return await query.ToListAsync();
    }
    #endregion
}