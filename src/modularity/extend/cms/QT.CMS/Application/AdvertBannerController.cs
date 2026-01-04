using QT.CMS.Entitys.Dto.Application;
using QT.Emp.Entitys;

namespace QT.CMS;

/// <summary>
/// 广告内容
/// </summary>
[Route("/api/cms/admin/advert/banner")]
[ApiController]
public class AdvertBannerController : ControllerBase
{
    private readonly ISqlSugarRepository<AdvertBannerEntity> _advertBannerService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public AdvertBannerController(ISqlSugarRepository<AdvertBannerEntity> advertBannerService, IUserService userService)
    {
        _advertBannerService = advertBannerService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/advert/banner/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("AdvertBanner", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<AdvertBannerDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _advertBannerService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<AdvertBannerDto>(); //.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取数据列表
    /// 示例：/admin/advert/banner?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("AdvertBanner", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam, [FromQuery] int parentId = 0)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<AdvertBannerDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<AdvertBannerDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表
        var list = await _advertBannerService.AsQueryable().Includes(x=>x.Advert)
            .WhereIF(parentId>0, x=> x.AdvertId == parentId)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,-Id")
            .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        return Ok(PageResult<AdvertBannerDto>.SqlSugarPageResult(list));

        //var list = await _advertBannerService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (parentId == 0 || parentId == x.AdvertId)
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

        ////映射成DTO
        //var resultDto = _mapper.Map<IEnumerable<AdvertBannerDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/advert/banner
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("AdvertBanner", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] AdvertBannerEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<AdvertBannerEntity>();
        //获取当前用户名
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        await _advertBannerService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<AdvertBannerDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/advert/banner/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("AdvertBanner", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] AdvertBannerEditDto modelDto)
    {
        //查找记录
        var model = await _advertBannerService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"记录[{id}]不存在或已删除"        );
        }
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        _advertBannerService.Context.Tracking(model);
        modelDto.Adapt(model);
        var result = await _advertBannerService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/advert/banner/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("AdvertBanner", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<AdvertBannerEditDto> patchDocument)
    {
        var model = await _advertBannerService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<AdvertBannerEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        _advertBannerService.Context.Tracking(model);
        modelToPatch.Adapt(model);
        await _advertBannerService.AutoUpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/advert/banner/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("AdvertBanner", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!await _advertBannerService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"记录[{id}]不存在或已删除");
        }
        var result = await _advertBannerService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/advert/banner?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("AdvertBanner", ActionType.Delete)]
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
        await _advertBannerService.DeleteAsync(x => ids.Contains(x.Id));

        return NoContent();
    }
    #endregion
}
