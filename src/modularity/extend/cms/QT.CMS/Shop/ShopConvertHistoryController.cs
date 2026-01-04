namespace QT.CMS;

/// <summary>
/// 积分兑换记录
/// </summary>
[Route("api/cms/admin/shop/convert/history")]
[ApiController]
public class ShopConvertHistoryController : ControllerBase
{
    private readonly ISqlSugarRepository<ShopConvertHistory> _shopConvertHistoryService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ShopConvertHistoryController(ISqlSugarRepository<ShopConvertHistory> shopConvertHistoryService, IUserService userService)
    {
        _shopConvertHistoryService = shopConvertHistoryService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/shop/convert/history/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ShopConvertHistoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _shopConvertHistoryService.AsQueryable()
            .Includes(x=>x.Order)
            .Includes(x=>x.ShopConvert)
            .Includes(x=>x.UserId)
            .SingleAsync(x => x.Id == id);

        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var result = model.Adapt<ShopConvertHistoryDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/shop/convert/history?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<ShopConvertHistoryDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<ShopConvertHistoryDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _shopConvertHistoryService.AsQueryable()
            .WhereIF(searchParam.Status >= 0, x => x.Status == searchParam.Status)
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
        //var resultDto = _mapper.Adapt<IEnumerable<ShopConvertHistoryDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<ShopConvertHistoryDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/shop/convert/history
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] ShopConvertHistoryEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<ShopConvertHistory>();
        model.AddTime = DateTime.Now;

        //写入数据库
        var mapModel = await _shopConvertHistoryService.InsertReturnIdentityAsync(model);
        //查询带导航属性的实体
        var modelFrom = await _shopConvertHistoryService.AsQueryable()
            .Includes(x => x.Order)
            .Includes(x => x.ShopConvert)
            .Includes(x => x.UserId)
            .SingleAsync(x => x.Id == model.Id);
        //映射成DTO再返回，否则出错
        var result = modelFrom.Adapt<ShopConvertHistoryDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/shop/convert/history/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] ShopConvertHistoryEditDto modelDto)
    {
        //查找记录
        var model = await _shopConvertHistoryService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _shopConvertHistoryService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelDto.Adapt(model);
        var result = await _shopConvertHistoryService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/shop/convert/history/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<ShopConvertHistoryEditDto> patchDocument)
    {
        var model = await _shopConvertHistoryService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<ShopConvertHistoryEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _shopConvertHistoryService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _shopConvertHistoryService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/shop/convert/history/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        if (!await _shopConvertHistoryService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _shopConvertHistoryService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/admin/shop/convert/history?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("ShopConvert", ActionType.Delete)]
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
        await _shopConvertHistoryService.DeleteAsync(x => arrIds.Contains(x.Id));

        return NoContent();
    }
    #endregion
}