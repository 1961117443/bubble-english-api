using QT.CMS.Entitys.Dto.Member;

namespace QT.CMS;

/// <summary>
/// 会员组别
/// </summary>
[Route("api/cms/admin/member/group")]
[ApiController]
public class MemberGroupController : ControllerBase
{
    private readonly ISqlSugarRepository<MemberGroup> _memberGroupService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public MemberGroupController(ISqlSugarRepository<MemberGroup> memberGroupService, IUserService userService)
    {
        _memberGroupService = memberGroupService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/member/group/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberGroup", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<MemberGroupDto>())
        {
           throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var resultFrom = await _memberGroupService.SingleAsync(x => x.Id == id);
        if (resultFrom == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = resultFrom.Adapt<MemberGroupDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/member/group/view/0
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("MemberGroup", ActionType.View)]
    public async Task<IEnumerable<MemberGroupDto>> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberGroupDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberGroupDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var resultFrom = await _memberGroupService.AsQueryable()
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "AddTime,Id")
            .ToListAsync();
        //var resultFrom = await _memberGroupService.QueryListAsync<MemberGroup>(top,
        //    x => !searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy ?? "AddTime,Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<MemberGroupDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/member/group?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("MemberGroup", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberGroupDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberGroupDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _memberGroupService.AsQueryable()
          .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.keyword))
          .OrderBy(searchParam.OrderBy ?? "AddTime,Id")
          .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        //var list = await _memberGroupService.QueryPageAsync<MemberGroup>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => !searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy ?? "AddTime,Id");

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
        //var resultDto = _mapper.Adapt<IEnumerable<MemberGroupDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<MemberGroupDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/member/group
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("MemberGroup", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] MemberGroupEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<MemberGroup>();
        //获取当前用户名
        model.AddBy = await _userService.GetUserNameAsync();
        model.AddTime = DateTime.Now;
        //写入数据库
        await _memberGroupService.InsertReturnEntityAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<MemberGroupDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/member/group/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberGroup", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] MemberGroupEditDto modelDto)
    {
        //查找记录
        var model = await _memberGroupService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        _memberGroupService.Context.Tracking(model);
        //获取当前用户名
        model.UpdateBy = await _userService.GetUserNameAsync();
        model.UpdateTime = DateTime.Now;

        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelDto.Adapt(model);
        var result = await _memberGroupService.AutoUpdateAsync(model);
        return NoContent();
    }

    // <summary>
    /// 局部更新一条记录
    /// 示例：/admin/member/group/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberGroup", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] JsonPatchDocument<MemberGroupEditDto> patchDocument)
    {
        //检查记录是否存在
        var model = await _memberGroupService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<MemberGroupEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _memberGroupService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _memberGroupService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/member/group/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberGroup", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        //检查参数是否正确
        if (!await _memberGroupService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        var result = await _memberGroupService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/admin/member/group?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("MemberGroup", ActionType.Delete)]
    public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
    {
        //检查参数是否为空
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var listIds = Ids.ToIEnumerable<long>();
        if (listIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        await _memberGroupService.DeleteAsync(x => listIds.Contains(x.Id));

        return NoContent();
    }
    #endregion
}