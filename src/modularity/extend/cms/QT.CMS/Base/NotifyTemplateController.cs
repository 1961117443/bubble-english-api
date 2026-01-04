using QT.CMS.Entitys.Dto.Member;

namespace QT.CMS;

/// <summary>
/// 消息模板
/// </summary>
[Route("api/cms/admin/notify/template")]
[ApiController]
public class NotifyTemplateController : ControllerBase
{
    private readonly ISqlSugarRepository<NotifyTemplate> _notifyTemplateService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public NotifyTemplateController(ISqlSugarRepository<NotifyTemplate> notifyTemplateService)
    {
        _notifyTemplateService = notifyTemplateService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/notify/template?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("NotifyTemplate", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<NotifyTemplateDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<NotifyTemplateDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表
        var list = await _notifyTemplateService.AsQueryable()
             .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x=> x.Title.Contains(searchParam.Keyword))
             .OrderBy(searchParam.OrderBy ?? "-Id")
              .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        return PageResult<NotifyTemplateDto>.SqlSugarPageResult(list);

        //var list = await _notifyTemplateService.QueryPageAsync<NotifyTemplate>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => !searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy ?? "-Id");

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
        //var resultDto = _mapper.Map<IEnumerable<NotifyTemplateDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/notify/template/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("NotifyTemplate", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<NotifyTemplateDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _notifyTemplateService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<NotifyTemplateDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 根据别名获取数据
    /// </summary>
    [HttpGet("{type}/{callIndex}")]
    [Authorize]
    [AuthorizeFilter("NotifyTemplate", ActionType.View)]
    public async Task<IActionResult> GetCallIndex([FromRoute] int type, [FromRoute] string callIndex, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<NotifyTemplateDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _notifyTemplateService.SingleAsync(
            x => x.Type == type && x.CallIndex != null && x.CallIndex.ToLower() == callIndex.ToLower());
        if (model == null)
        {
            throw Oops.Oh($"数据{callIndex}不存在或已删除");
        }
        //根据字段进行塑形
        var result = model.Adapt<NotifyTemplateDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/notify/template
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("NotifyTemplate", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] NotifyTemplateEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<NotifyTemplate>();
        //写入数据库
        await _notifyTemplateService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<NotifyTemplateDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/notify/template/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("NotifyTemplate", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] NotifyTemplateEditDto modelDto)
    {
        //查找记录
        var model = await _notifyTemplateService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _notifyTemplateService.Context.Tracking(model);
        model.UpdateTime = DateTime.Now; //更新时间
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelDto.Adapt(model);
        var result = await _notifyTemplateService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/notify/template/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("NotifyTemplate", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] JsonPatchDocument<NotifyTemplateEditDto> patchDocument)
    {
        var model = await _notifyTemplateService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<NotifyTemplateEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _notifyTemplateService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _notifyTemplateService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/notify/template/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("NotifyTemplate", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        //系统默认不允许删除
        if (!await _notifyTemplateService.AnyAsync(x => x.Id == id && x.IsSystem == 0))
        {
            throw Oops.Oh($"数据[{id}]不存在或无权删除");
        }
        var result = await _notifyTemplateService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/notify/template?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("NotifyTemplate", ActionType.Delete)]
    public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var listIds = Ids.ToIEnumerable<int>();
        if (listIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        await _notifyTemplateService.DeleteAsync(x => listIds.Contains(x.Id) && x.IsSystem == 0);

        return NoContent();
    }
    #endregion
}