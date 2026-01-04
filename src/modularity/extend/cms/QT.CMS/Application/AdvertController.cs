using QT.CMS.Entitys.Dto.Application;

namespace QT.CMS;

/// <summary>
/// 广告位
/// </summary>
[Route("/api/cms/admin/advert")]
[ApiController]
public class AdvertController : ControllerBase
{
    private readonly ISqlSugarRepository<AdvertEntity> _advertService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public AdvertController(ISqlSugarRepository<AdvertEntity> advertService, IUserService userService)
    {
        _advertService = advertService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/advert/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("Advert", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<AdvertDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _advertService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<AdvertDto>(); //.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/advert/view/0
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("Advert", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<AdvertDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<AdvertDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await  _advertService.AsQueryable()
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _advertService.QueryListAsync<Advert>(top,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<AdvertDto>>();
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取数据列表
    /// 示例：/admin/advert?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("Advert", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<AdvertDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<AdvertDto>())
        {
            throw Oops.Oh("请输入正确的属性参数"  );
        }

        //获取数据列表
        var list = await _advertService.AsQueryable()
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        return Ok(PageResult<AdvertDto>.SqlSugarPageResult(list));

        //var list = await _advertService.QueryPageAsync<Advert>(
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

        ////映射成DTO
        //var resultDto = _mapper.Map<IEnumerable<AdvertDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/advert
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("Advert", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] AdvertEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<AdvertEntity>();
        //获取当前用户名
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        await _advertService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<AdvertDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/advert/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("Advert", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] AdvertEditDto modelDto)
    {
        //查找记录
        var model = await _advertService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"记录[{id}]不存在或已删除");
        }
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        _advertService.Context.Tracking(model); 
        modelDto.Adapt(model);
        var result = await _advertService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/advert/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("Advert", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<AdvertEditDto> patchDocument)
    {
        var model = await _advertService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<AdvertEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _advertService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _advertService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/advert/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("Advert", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!await _advertService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"记录[{id}]不存在或已删除");
        }
        var result = await _advertService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/advert?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("Advert", ActionType.Delete)]
    public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var ids = Ids.ToIEnumerable<int>();
        if (ids == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        await _advertService.DeleteAsync(x => ids.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 根据ID或标识获取数据
    /// 示例：/client/advert/1
    /// </summary>
    [HttpGet("/client/advert/{indexKey}")]
    [CachingFilter]
    public async Task<IActionResult> ClientGetById([FromRoute] string indexKey, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<AdvertDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        AdvertEntity? model = null;
        if (int.TryParse(indexKey, out int advertId))
        {
            model = await _advertService.AsQueryable().Includes(x => x.Banner).SingleAsync(x => x.Id == advertId);
         
        }
        if (model == null)
        {
            model = await _advertService.AsQueryable().Includes(x => x.Banner).SingleAsync(x => x.CallIndex == indexKey);
        }
        if (model == null)
        {
            throw Oops.Oh($"广告已失效或不存在");
        }
        if (model.Banner !=null)
        {
            model.Banner = model.Banner.Where(b => b.Status == 1 && DateTime.Compare(b.StartTime, DateTime.Now) <= 0 && DateTime.Compare(b.EndTime, DateTime.Now) >= 0).OrderBy(b => b.SortId).ToList();
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<AdvertDto>();//.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/client/advert/view/10
    /// </summary>
    [HttpGet("/client/advert/view/{top}")]
    [CachingFilter]
    public async Task<IActionResult> ClientGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<AdvertDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<AdvertDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _advertService.AsQueryable().Includes(x=>x.Banner)
            .Where(x=>x.Banner.Any(b=> b.Status == 1 && SqlFunc.Between(DateTime.Now,b.StartTime,b.EndTime)))
            .WhereIF(searchParam.SiteId >= 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .AutoTake(top)
            .ToListAsync();

        //var resultFrom = await _advertService.QueryListAsync(top,
        //    x => x.Banner.Any(b => b.Status == 1 && DateTime.Compare(b.StartTime, DateTime.Now) <= 0 && DateTime.Compare(b.EndTime, DateTime.Now) >= 0)
        //    && (searchParam.SiteId < 0 || x.SiteId == searchParam.SiteId)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<AdvertDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }
    #endregion
}