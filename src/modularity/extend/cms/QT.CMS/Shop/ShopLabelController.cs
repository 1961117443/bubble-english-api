namespace QT.CMS;

/// <summary>
/// 商品标签
/// </summary>
[Route("api/cms/admin/shop/label")]
[ApiController]
public class ShopLabelController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopLabel> _shopLabelService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ShopLabelController(ISqlSugarRepository<ShopLabel> shopLabelService, IUserService userService)
    {
        _shopLabelService = shopLabelService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/shop/label/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopLabel", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopLabelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopLabelService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopLabelDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/shop/label/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("ShopLabel", ActionType.View)]
    public async Task<IEnumerable<ShopLabelDto>> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopLabelDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopLabelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _shopLabelService.AsQueryable()
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _shopLabelService.QueryListAsync<ShopLabel>(top,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<ShopLabelDto>>(); // .ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/shop/label?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("ShopLabel", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopLabelDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopLabelDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopLabelService.AsQueryable()
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        //var list = await _shopLabelService.QueryPageAsync<ShopLabel>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
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
        //var resultDto = _mapper.Adapt<IEnumerable<ShopLabelDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopLabelDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/shop/label
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("ShopLabel", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] ShopLabelEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<ShopLabel>();
        //获取当前用户名
        model.AddBy =  await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        var mapModel = await _shopLabelService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = mapModel.Adapt<ShopLabelDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/shop/label/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopLabel", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] ShopLabelEditDto modelDto)
    {
        //查找记录
        var model = await _shopLabelService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        _shopLabelService.Context.Tracking(model);
        modelDto.Adapt(model);
        //获取当前用户名
        model.UpdateBy = await _userService.GetUserNameAsync();
        model.UpdateTime = DateTime.Now;
        //调用保存即可
        var result = await _shopLabelService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/shop/label/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopLabel", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<ShopLabelEditDto> patchDocument)
    {
        var model = await _shopLabelService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }

        var modelToPatch = model.Adapt<ShopLabelEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _shopLabelService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _shopLabelService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/shop/label/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopLabel", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        if (!await _shopLabelService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        var result = await _shopLabelService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/shop/label?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("ShopLabel", ActionType.Delete)]
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
        await _shopLabelService.DeleteAsync(x => arrIds.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/shop/label/view/10
    /// </summary>
    [HttpGet("/client/shop/label/view/{top}")]
    [CachingFilter]
    [NonUnify]
    public async Task<IEnumerable<ShopLabelDto>> ClientGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        return await GetList(top, searchParam);
    }
    #endregion
}