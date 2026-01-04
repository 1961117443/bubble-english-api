using QT.CMS.Entitys.Dto.Member;
using QT.Systems.Entitys.Permission;

namespace QT.CMS;

/// <summary>
/// 会员附件下载记录
/// </summary>
[Route("api/cms/member/attach")]
[ApiController]
public class MemberAttachController : ControllerBase
{
    private readonly ISqlSugarRepository<MemberAttachLog> _memberAttachLogService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public MemberAttachController(ISqlSugarRepository<MemberAttachLog> memberAttachLogService)
    {
        _memberAttachLogService = memberAttachLogService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/member/attach/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberAttach", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<MemberAttachLogDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var resultFrom = await _memberAttachLogService.AsQueryable()
  .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
  .Where((a, u) => a.Id == id)
  .Select((a, u) => new MemberAttachLogDto
  {
      id = SqlFunc.ToString(a.Id),
      userId = a.UserId,
      userName = u.Account,
      attachId = a.AttachId,
      fileName = a.FileName,
      addTime = a.AddTime
  })
  .FirstAsync();
        //var resultFrom = await _memberAttachLogService.QueryAsync(x => x.Id == id, WriteRoRead.Write);
        if (resultFrom == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        //根据字段进行塑形
        var result = resultFrom.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/member/attach?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("MemberAttach", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberAttachLogDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberAttachLogDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _memberAttachLogService.AsQueryable()
 .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
  .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), (a, u) => u.Account.Contains(searchParam.Keyword))
 .Select((a, u) => new MemberAttachLogDto
 {
     id = SqlFunc.ToString(a.Id),
     userId = a.UserId,
     userName = u.Account,
     attachId = a.AttachId,
     fileName = a.FileName,
     addTime = a.AddTime
 }).OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
        .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);
        return PageResult<MemberAttachLogDto>.SqlSugarPageResult(list); 

        //var list = await _memberAttachLogService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.UserName != null && x.UserName.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");
        //if (list.Count() <= 0)
        //{
        //    throw Oops.Oh("暂无查询到记录");
        //}

        ////x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.TotalCount,
        //    pageSize = list.PageSize,
        //    pageIndex = list.PageIndex,
        //    totalPages = list.TotalPages
        //};
        //Response.Headers.Add("x-pagination", SerializeHelper.SerializeObject(paginationMetadata));

        ////根据字段进行塑形，注意因为没有使用AotoMapper，所以要转换成Enumerable
        //var resultDto = list.AsEnumerable().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }


    /// <summary>
    /// 添加一条记录
    /// 示例：/member/attach
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("MemberAttach", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] MemberAttachLogEditDto modelDto)
    {
        //检查会员是否存在
        if (!await _memberAttachLogService.Context.Queryable<Members>().AnyAsync(x => x.UserId == modelDto.userId))
        {
            throw Oops.Oh($"会员[{modelDto.userId}]不存在");
        }
        //获取文章附件信息
        var attachModel = await _memberAttachLogService.Context.Queryable<ArticleAttach>().FirstAsync(x => x.Id == modelDto.attachId);
        if (attachModel == null)
        {
            throw Oops.Oh($"附件[{modelDto.attachId}]不存在或已删除");
        }
        modelDto.fileName = attachModel.FileName;

        //映射成实体
        var model = modelDto.Adapt<MemberAttachLog>();
        //写入数据库
        await _memberAttachLogService.InsertReturnEntityAsync(model);
        //重新联合查询
        var result = await _memberAttachLogService.AsQueryable()
.InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
.Where((a, u) => a.Id == model.Id)
.Select((a, u) => new MemberAttachLogDto
{
  id = SqlFunc.ToString(a.Id),
  userId = a.UserId,
  userName = u.Account,
  attachId = a.AttachId,
  fileName = a.FileName,
  addTime = a.AddTime
})
.FirstAsync();
        return Ok(result);
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/member/attach/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberAttach", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        //检查记录是否存在
        if (!await _memberAttachLogService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _memberAttachLogService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/member/attach?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("MemberAttach", ActionType.Delete)]
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
        await _memberAttachLogService.DeleteAsync(x => listIds.Contains(x.Id));

        return NoContent();
    }
    #endregion
}