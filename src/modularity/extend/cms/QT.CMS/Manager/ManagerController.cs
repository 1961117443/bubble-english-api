using QT.CMS.Entitys.Dto.Application;

namespace QT.CMS;

/// <summary>
/// 管理员
/// </summary>
[Route("api/cms/admin/manager")]
[ApiController]
public class ManagerController : ControllerBase
{
    private readonly ISqlSugarRepository<Manager> _managerService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ManagerController(ISqlSugarRepository<Manager> managerService, IUserService userService)
    {
        _managerService = managerService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    ///// <summary>
    ///// 获取分页列表
    ///// 示例：/admin/manager?pageSize=10&pageIndex=1
    ///// </summary>
    //[HttpGet]
    //[Authorize]
    //[AuthorizeFilter("Manager", ActionType.View)]
    //public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    //{
    //    //检测参数是否合法
    //    if (searchParam.OrderBy != null
    //        && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<ManagerDto>())
    //    {
    //        throw Oops.Oh("请输入正确的排序参数");
    //    }
    //    if (!searchParam.Fields.IsPropertyExists<ManagerDto>())
    //    {
    //        throw Oops.Oh("请输入正确的属性参数");
    //    }
    //    //获取当前用户是否超管
    //    byte roleType = 1; //系统管理员
    //    if (await _userService.IsSuperAdminAsync())
    //    {
    //        roleType = 2; //超级管理员
    //    }
    //    //获取数据列表
    //    var list = await _managerService.QueryPageAsync(
    //        pageParam.PageSize,
    //        pageParam.PageIndex,
    //        x => x.User != null
    //        && x.User.UserRoles != null
    //        && x.User.UserRoles.Any(x => x.Role!.RoleType <= roleType)
    //        && (!searchParam.Keyword.IsNotNullOrEmpty()
    //            || x.User.UserName.Contains(searchParam.Keyword)
    //            || x.User.Email.Contains(searchParam.Keyword)
    //            || x.User.PhoneNumber.Contains(searchParam.Keyword)
    //            || (x.RealName != null && x.RealName.Contains(searchParam.Keyword))),
    //        searchParam.OrderBy ?? "AddTime,Id");

    //    //x-pagination
    //    var paginationMetadata = new
    //    {
    //        totalCount = list.TotalCount,
    //        pageSize = list.PageSize,
    //        pageIndex = list.PageIndex,
    //        totalPages = list.TotalPages
    //    };
    //    Response.Headers.Add("x-pagination", SerializeHelper.SerializeObject(paginationMetadata));

    //    //映射成DTO
    //    var resultDto = _mapper.Map<IEnumerable<ManagerDto>>(list).ShapeData(searchParam.Fields);
    //    return Ok(resultDto);
    //}

    ///// <summary>
    ///// 根据用户ID获取数据
    ///// 示例：/admin/manager/1
    ///// </summary>
    //[HttpGet("{userId}")]
    //[Authorize]
    //[AuthorizeFilter("Manager", ActionType.View)]
    //public async Task<IActionResult> GetById([FromRoute] int userId, [FromQuery] BaseParameter param)
    //{
    //    //检测参数是否合法
    //    if (!param.Fields.IsPropertyExists<ManagerDto>())
    //    {
    //        return BadRequest(ResponseMessage.Error("请输入正确的属性参数"));
    //    }
    //    //查询数据库获取实体
    //    var model = await _managerService.QueryAsync(x => x.UserId == userId, WriteRoRead.Write);
    //    if (model == null)
    //    {
    //        return NotFound(ResponseMessage.Error($"用户{userId}不存在或已删除"));
    //    }
    //    //使用AutoMapper转换成ViewModel
    //    //根据字段进行塑形
    //    var result = _mapper.Map<ManagerDto>(model).ShapeData(param.Fields);
    //    return Ok(result);
    //}

    ///// <summary>
    ///// 添加一条记录
    ///// 示例：/admin/manager
    ///// </summary>
    //[HttpPost]
    //[Authorize]
    //[AuthorizeFilter("Manager", ActionType.Add)]
    //public async Task<IActionResult> Add([FromBody] ManagerEditDto modelDto)
    //{
    //    //判断用户名邮箱手机是否为空
    //    if (!modelDto.UserName.IsNotNullOrEmpty() 
    //        && !modelDto.Email.IsNotNullOrEmpty() && !modelDto.Phone.IsNotNullOrEmpty())
    //    {
    //        return BadRequest(ResponseMessage.Error("用户名、邮箱、手机至少填写一项"));
    //    }
    //    //判断密码是否为空
    //    if (!modelDto.Password.IsNotNullOrEmpty())
    //    {
    //        return BadRequest(ResponseMessage.Error("请输入登录密码"));
    //    }
    //    //如果用户名为空，则自动生成用户名
    //    if (!modelDto.UserName.IsNotNullOrEmpty())
    //    {
    //        modelDto.UserName = UtilConvert.GetGuidToString();
    //    }
    //    //判断用户名是否重复
    //    if(await _managerService.ExistsAsync<ApplicationUser>(x => x.UserName == modelDto.UserName))
    //    {
    //        return BadRequest(ResponseMessage.Error("用户名已重复，请更换"));
    //    }
    //    //判断邮箱是否重复
    //    if(modelDto.Email.IsNotNullOrEmpty() 
    //        && await _managerService.ExistsAsync<ApplicationUser>(x => x.Email == modelDto.Email))
    //    {
    //        return BadRequest(ResponseMessage.Error("邮箱地址已重复，请更换"));
    //    }
    //    //判断手机号是否重复
    //    if (modelDto.Phone.IsNotNullOrEmpty()
    //        && await _managerService.ExistsAsync<ApplicationUser>(x => x.PhoneNumber == modelDto.Phone))
    //    {
    //        return BadRequest(ResponseMessage.Error("手机号码已重复，请更换"));
    //    }
    //    var roleModel = await _managerService.QueryAsync<ApplicationRole>(x => x.Id == modelDto.RoleId);
    //    if (roleModel == null)
    //    {
    //        return BadRequest(ResponseMessage.Error("管理角色不存在或已删除"));
    //    }
    //    //超管才能添加超级管理员
    //    if (roleModel.RoleType == (byte)RoleType.SuperAdmin && !await _userService.IsSuperAdminAsync())
    //    {
    //        return BadRequest(ResponseMessage.Error("没有权限添加超级管理员"));
    //    }
    //    var result = await _managerService.AddAsync(modelDto);
    //    return Ok(result);
    //}

    ///// <summary>
    ///// 修改一条记录
    ///// 示例：/admin/manager/1
    ///// </summary>
    //[HttpPut("{userId}")]
    //[Authorize]
    //[AuthorizeFilter("Manager", ActionType.Edit)]
    //public async Task<IActionResult> Update([FromRoute] int userId, [FromBody] ManagerEditDto modelDto)
    //{
    //    //查找记录
    //    var model = await _managerService.QueryAsync<Manager>(x => x.UserId == userId);
    //    if (model == null)
    //    {
    //        return BadRequest(ResponseMessage.Error($"管理员{userId}不存在或已删除"));
    //    }
    //    //如果用户名发生改变，则检查重复
    //    if (modelDto.UserName.IsNotNullOrEmpty()
    //        && await _managerService.ExistsAsync<ApplicationUser>(x => x.Id != userId && x.UserName == modelDto.UserName))
    //    {
    //        return BadRequest(ResponseMessage.Error("用户名已重复，请更换"));
    //    }
    //    //如果邮箱发生改变，则检查重复
    //    if (modelDto.Email.IsNotNullOrEmpty()
    //        && await _managerService.ExistsAsync<ApplicationUser>(x => x.Id != userId && x.Email == modelDto.Email))
    //    {
    //        return BadRequest(ResponseMessage.Error("邮箱地址已重复，请更换"));
    //    }
    //    //如果手机号发生改变，刚检查重复
    //    if (modelDto.Phone.IsNotNullOrEmpty()
    //        && await _managerService.ExistsAsync<ApplicationUser>(x => x.Id != userId && x.PhoneNumber == modelDto.Phone))
    //    {
    //        return BadRequest(ResponseMessage.Error("手机号码已重复，请更换"));
    //    }
    //    //如果角色发生改变，则检查是否有权限
    //    var roleModel = await _managerService.QueryAsync<ApplicationRole>(x => x.Id == modelDto.RoleId);
    //    if (roleModel == null)
    //    {
    //        return BadRequest(ResponseMessage.Error("管理角色不存在或已删除"));
    //    }
    //    //超管才能添加超级管理员
    //    if (roleModel.RoleType == (byte)RoleType.SuperAdmin && !await _userService.IsSuperAdminAsync())
    //    {
    //        return BadRequest(ResponseMessage.Error("没有权限添加超级管理员"));
    //    }

    //    var result = await _managerService.UpdateAsync(userId, modelDto);
    //    return NoContent();
    //}

    ///// <summary>
    ///// 局部更新一条记录
    ///// 示例：/admin/manager/1
    ///// Body：[{"op":"replace","path":"/title","value":"new title"}]
    ///// </summary>
    //[HttpPatch("{userId}")]
    //[Authorize]
    //[AuthorizeFilter("Manager", ActionType.Edit)]
    //public async Task<IActionResult> Update([FromRoute] int userId, [FromBody] JsonPatchDocument<ManagerDto> patchDocument)
    //{
    //    var model = await _managerService.QueryAsync<Manager>(x => x.UserId == userId, WriteRoRead.Write);
    //    if (model == null)
    //    {
    //        return BadRequest(ResponseMessage.Error($"数据[{userId}]不存在或已删除"));
    //    }

    //    var modelToPatch = _mapper.Map<ManagerDto>(model);
    //    patchDocument.ApplyTo(modelToPatch, ModelState);
    //    //验证数据是否合法
    //    if (!TryValidateModel(modelToPatch))
    //    {
    //        return ValidationProblem(ModelState);
    //    }
    //    //更新操作AutoMapper替我们完成，只需要调用保存即可
    //    _mapper.Map(modelToPatch, model);
    //    await _managerService.SaveAsync();
    //    return NoContent();
    //}

    ///// <summary>
    ///// 删除一条记录
    ///// 示例：/admin/manager/1
    ///// </summary>
    //[HttpDelete("{userId}")]
    //[Authorize]
    //[AuthorizeFilter("Manager", ActionType.Delete)]
    //public async Task<IActionResult> Delete([FromRoute] int userId)
    //{
    //    //系统默认不允许删除
    //    if (!await _managerService.ExistsAsync<Manager>(x => x.UserId == userId))
    //    {
    //        return BadRequest(ResponseMessage.Error($"数据[{userId}]不存在或无权删除"));
    //    }
    //    var result = await _managerService.DeleteAsync(userId);

    //    return NoContent();
    //}

    ///// <summary>
    ///// 批量删除记录(级联数据)
    ///// 示例：/admin/manager?ids=1,2,3
    ///// </summary>
    //[HttpDelete]
    //[Authorize]
    //[AuthorizeFilter("Manager", ActionType.Delete)]
    //public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
    //{
    //    if (Ids == null)
    //    {
    //        return BadRequest(ResponseMessage.Error("传输参数不可为空"));
    //    }
    //    //将ID列表转换成IEnumerable
    //    var listIds = Ids.ToIEnumerable<int>();
    //    if (listIds == null)
    //    {
    //        return BadRequest(ResponseMessage.Error("传输参数不符合规范"));
    //    }
    //    //执行批量删除操作
    //    foreach (var userId in listIds)
    //    {
    //        var result = await _managerService.DeleteAsync(userId);
    //    }
    //    return NoContent();
    //}
    #endregion

    #region 当前账户调用接口========================
    /// <summary>
    /// 获取当前管理员信息
    /// 示例：/account/manager/info?fields=userName
    /// </summary>
    [HttpGet("/account/manager/info")]
    [Authorize]
    public async Task<IActionResult> GetInfo([FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<ManagerDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var userId = await _userService.GetUserIdAsync();
        var model = await _managerService.AsQueryable().Includes(x => x.User)
            .Where(x => x.UserId == userId)
            .FirstAsync();
        if (model == null)
        {
            throw Oops.Oh("用户不存在或已删除，请重试");
        }
        //根据字段进行塑形
        var result = model.Adapt<ManagerDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    ///// <summary>
    ///// 当前管理员修改密码
    ///// 示例：/account/manager/password
    ///// </summary>
    //[HttpPut("/account/manager/password")]
    //[Authorize]
    //public async Task<IActionResult> UpdatePassword([FromBody] PasswordDto modelDto)
    //{
    //    await _userService.UpdatePasswordAsync(modelDto);
    //    return NoContent();
    //}
    #endregion
}
