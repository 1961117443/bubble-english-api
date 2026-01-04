using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Emum;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Config;
using QT.CMS.Entitys.Dto.Login;
using QT.CMS.Entitys.Dto.Member;
using QT.CMS.Entitys.Dto.Parameter;
using QT.CMS.Interfaces;
using QT.Common.Cache;
using QT.Common.Const;
using QT.Common.Core.Manager;
using QT.Common.Core.Security;
using QT.Common.Extension;
using QT.Common.Filter;
using QT.DataEncryption;
using QT.FriendlyException;
using QT.JsonSerialization;
using QT.Systems.Entitys.Permission;
using SqlSugar;
using System.Reactive;
using System.Security.Claims;

namespace QT.CMS;

/// <summary>
/// 会员信息
/// </summary>
[Route("api/cms/admin/member")]
[ApiController]
[ApiDescriptionSettings(ModuleConst.CMS)]
public class MemberController : ControllerBase
{
    private readonly ISqlSugarRepository<Members> _memberService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public MemberController(ISqlSugarRepository<Members> memberService, IUserService userService)
    {
        _memberService = memberService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取一条记录
    /// 示例：/admin/member/1
    /// </summary>
    [HttpGet("{userId}")]
    [Authorize]
    //[AuthorizeFilter("Member", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] string userId, [FromQuery] MemberParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<MembersDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _memberService.AsQueryable()
            .Includes(x => x.User)
            .Includes(x => x.Group)
            .SingleAsync(x => x.UserId == userId);
        if (model == null)
        {
            throw Oops.Oh($"会员[{userId}]不存在或已删除");
        }
        //根据字段进行塑形
        var result = model.Adapt<MembersDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取总记录数量
    /// 示例：/admin/member/view/count
    /// </summary>
    [HttpGet("view/count")]
    [Authorize]
    //[AuthorizeFilter("Member", ActionType.View)]
    public async Task<IActionResult> GetCount([FromQuery] MemberParameter searchParam)
    {
        //var result = await _memberService.QueryCountAsync(x => searchParam.Status < 0 || (x.User != null && x.User.Status == searchParam.Status));
        var result = await _memberService.CountAsync(x => searchParam.Status < 0 || (x.User != null && x.User.EnabledMark == 1));
        //返回成功200
        return Ok(result);
    }

    /// <summary>
    /// 获取会员注册统计
    /// 示例：/admin/member/view/report
    /// </summary>
    [HttpGet("view/report")]
    [Authorize]
    //[AuthorizeFilter("Member", ActionType.View)]
    public async Task<IActionResult> GetMemberRegister([FromQuery] MemberParameter searchParam)
    {
        throw new NotImplementedException();
        //if (searchParam.StartTime == null)
        //{
        //    searchParam.StartTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1);
        //}
        //if (searchParam.EndTime == null)
        //{
        //    searchParam.EndTime = DateTime.Now;
        //}
        //var result = await _memberService.QueryCountListAsync(0,
        //    x => (DateTime.Compare(x.RegTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
        //    && (DateTime.Compare(x.RegTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        //);
        //return Ok(result);
    }

    // <summary>
    /// 获取分页列表
    /// 示例：/admin/member?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    //[AuthorizeFilter("Member", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] MemberParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MembersDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MembersDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表 status =-1全部 0正常 1待验证 2待审核 3黑名单
        var list = await _memberService.AsQueryable()
            .Includes(x=>x.User)
            .Includes(x => x.Group)
            .Where(x =>!SqlFunc.IsNullOrEmpty(x.UserId))
            .WhereIF(searchParam.SiteId > 0, x => x.SiteId == searchParam.SiteId)
            .WhereIF(searchParam.Status == 0, x=>x.User.EnabledMark ==1)
            .WhereIF(searchParam.Status == 3, x => x.User.EnabledMark == 0)
            .WhereIF(searchParam.Status == 2, x => x.User.EnabledMark == 3)
            //.WhereIF(searchParam.Status > 0, x => x.User.EnabledMark == 1)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.RealName.Contains(searchParam.Keyword) || x.User.RealName.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "-Id,-RegTime")
            .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        //var list = await _memberService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.User!=null
        //    && (searchParam.SiteId <= 0 || x.SiteId == searchParam.SiteId)
        //    && (searchParam.Status < 0 || x.User.Status == searchParam.Status)
        //    && (!searchParam.Keyword.IsNotNullOrEmpty()
        //        || (x.RealName!=null&&x.RealName.Contains(searchParam.Keyword))
        //        || x.User.UserName.Contains(searchParam.Keyword) 
        //        || x.User.Email.Contains(searchParam.Keyword)
        //        || x.User.PhoneNumber.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy??"-Id,-RegTime");

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
        //var resultDto = _mapper.Map<IEnumerable<MembersDto>>(list).ShapeData(searchParam.Fields);
        return Ok(PageResult<MembersDto>.SqlSugarPageResult(list));
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/member
    /// </summary>
    [HttpPost]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("Member", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] MembersEditDto modelDto)
    {
        var result = await this.AddAsync(modelDto);
        //{
        //    //判断用户名邮箱手机是否为空
        //    if (!modelDto.UserName.IsNotEmptyOrNull()
        //        && !modelDto.Email.IsNotEmptyOrNull() && !modelDto.Phone.IsNotEmptyOrNull())
        //    {
        //        throw Oops.Oh("用户名、邮箱、手机至少填写一项");
        //    }
        //    //判断密码是否为空
        //    if (!modelDto.Password.IsNotEmptyOrNull())
        //    {
        //        throw Oops.Oh("请输入登录密码");
        //    }
        //    //如果用户名为空，则自动生成用户名
        //    if (!modelDto.UserName.IsNotEmptyOrNull())
        //    {
        //        modelDto.UserName = snow.GetGuidToString();
        //    }
        //    //判断用户名是否重复
        //    if (await ExistsAsync<ApplicationUser>(x => x.UserName == modelDto.UserName))
        //    {
        //        throw new ResponseException("用户名已重复，请更换");
        //    }
        //    //判断邮箱是否重复
        //    if (modelDto.Email.IsNotEmptyOrNull()
        //        && await ExistsAsync<ApplicationUser>(x => x.Email == modelDto.Email))
        //    {
        //        throw new ResponseException("邮箱地址已重复，请更换");
        //    }
        //    //判断手机号是否重复
        //    if (modelDto.Phone.IsNotEmptyOrNull()
        //        && await ExistsAsync<ApplicationUser>(x => x.PhoneNumber == modelDto.Phone))
        //    {
        //        throw new ResponseException("手机号码已重复，请更换");
        //    }
        //    //检查会员组是否存在
        //    if (modelDto.GroupId <= 0)
        //    {
        //        var group = await QueryAsync<MemberGroup>(x => x.IsDefault == 1);
        //        if (group == null)
        //        {
        //            throw new ResponseException("没有找到默认会员组");
        //        }
        //        modelDto.GroupId = group.Id;
        //    }
        //    //获取会员角色
        //    var role = await QueryAsync<ApplicationRole>(x => x.RoleType == (byte)RoleType.Member);
        //    if (role == null)
        //    {
        //        throw new ResponseException("会员角色不存在");
        //    }
        //    //创建用户对象
        //    var user = new ApplicationUser()
        //    {
        //        UserName = modelDto.UserName,
        //        Email = modelDto.Email,
        //        PhoneNumber = modelDto.Phone,
        //        Status = modelDto.Status
        //    };
        //    //将用户与角色关联
        //    user.UserRoles = new List<ApplicationUserRole>()
        //    {
        //        new ApplicationUserRole()
        //        {
        //            RoleId=role.Id,
        //            UserId=user.Id
        //        }
        //    };
        //    //将用户会员信息关联
        //    user.Member = new Members()
        //    {
        //        SiteId = modelDto.SiteId,
        //        UserId = user.Id,
        //        GroupId = modelDto.GroupId,
        //        Avatar = modelDto.Avatar,
        //        RealName = modelDto.RealName,
        //        Sex = modelDto.Sex,
        //        Birthday = modelDto.Birthday,
        //        RegIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress.ToString(),
        //        RegTime = DateTime.Now
        //    };
        //    //HASH密码，保存用户
        //    var result = await _userManager.CreateAsync(user, modelDto.Password);
        //    if (!result.Succeeded)
        //    {
        //        throw new ResponseException($"{result.Errors.FirstOrDefault()?.Description}");
        //    }

        //    var model = await QueryAsync(x => x.UserId == user.Id, WriteRoRead.Write);
        //    var resultModel = _mapper.Map<MembersDto>(model);

        //    return resultModel;
        //}
        //var result = await _memberService.AddAsync(modelDto);
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/member/1
    /// </summary>
    [HttpPut("{userId}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("Member", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] string userId, [FromBody] MembersEditDto modelDto)
    {
        var result = await this.UpdateAsync(userId, modelDto);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/member/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{userId}")]
    [Authorize]
    //[AuthorizeFilter("Member", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] string userId, [FromBody] JsonPatchDocument<MembersEditDto> patchDocument)
    {
        var model = await _memberService.SingleAsync(x => x.UserId == userId);
        if (model == null)
        {
            throw Oops.Oh($"数据[{userId}]不存在或已删除");
        }

        var modelToPatch = model.Adapt<MembersEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _memberService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _memberService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/member/1
    /// </summary>
    [HttpDelete("{userId}")]
    [Authorize]
    [SqlSugarUnitOfWork]
    //[AuthorizeFilter("Member", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] string userId)
    {
        //查找记录是否存在
        if (!await _memberService.AnyAsync(x => x.UserId == userId))
        {
            throw Oops.Oh($"数据{userId}不存在或已删除");
        }
        var result = await _memberService.DeleteAsync(userId);
        await _memberService.Context.Deleteable<UserEntity>(x => x.Id == userId).ExecuteCommandAsync();
        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/member?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    //[AuthorizeFilter("Member", ActionType.Delete)]
    public async Task<IActionResult> DeleteByIds([FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var listIds = Ids.ToIEnumerable<string>();
        if (listIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //将符合条件的会员ID一次查询出来
        var list = await _memberService.Where(x => listIds.Contains(x.UserId)).ToListAsync();
        //执行批量删除操作
        foreach (var modelt in list)
        {
            await _memberService.DeleteAsync(modelt.UserId);
        }
        return NoContent();
    }

    /// <summary>
    /// 审核一条记录
    /// 示例：/admin/member
    /// </summary>
    [HttpPut("audit")]
    [Authorize]
    //[AuthorizeFilter("Member", ActionType.Audit)]
    public async Task<IActionResult> Audit([FromQuery] string Ids)
    {
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //将ID列表转换成IEnumerable
        var listIds = Ids.ToIEnumerable<string>();
        if (listIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //将符合条件的会员ID一次查询出来
        var list = await _memberService.Context.Queryable<UserEntity>()
            .Where(x => listIds.Contains(x.Id) && x.EnabledMark == 3)
            .ToListAsync();

        if (list.IsAny())
        {
            //var list = await _memberService.QueryListAsync(0,
            //    x => x.User != null && x.User.Status == 2 && listIds.Contains(x.UserId), "-RegTime,-Id", WriteRoRead.Write);
            //执行批量删除操作
            foreach (var modelt in list)
            {
                //if (modelt.User != null)
                //{
                //    modelt.User.Status = 0;
                //}

                modelt.EnabledMark = 1; //将会员状态改为启用
            }
            await _memberService.Context.Updateable<UserEntity>(list).UpdateColumns(x => x.EnabledMark).ExecuteCommandAsync();
        }
       
        //var result = await _memberService.SaveAsync();
        return NoContent();
    }
    #endregion

    #region 当前用户调用接口========================
    /// <summary>
    /// 会员注册
    /// 示例：/account/member/register
    /// </summary>
    [HttpPost("/account/member/register")]
    [AllowAnonymous, NonUnify]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> AccountAdd([FromBody] RegisterDto registerDto, [FromServices] ICacheManager cache)
    {
        string cacheKey = string.Format("{0}{1}", CommonConst.CACHEKEYCODE, registerDto.CodeKey);

        var code = await cache.GetAsync<string>(cacheKey);
        //if (code.IsNullOrEmpty())
        //{
        //    code = await App.GetService<ICacheManager>().GetAsync<string>(cacheKey);
        //}
        //else
        //{
        //    await cache.DelAsync(cacheKey);
        //}

        //检查验证码是否正确 
        if (code.IsNullOrEmpty())
        {
            throw Oops.Oh("验证码已过期，请重新获取");
        }
        //var cacheValue = cacheObj.ToString();
        var codeSecret = string.Empty;
        if (registerDto.Method == 1)
        {
            if (!registerDto.Phone.IsNotEmptyOrNull())
            {
                throw Oops.Oh("请填写手机号码");
            }
            codeSecret = registerDto.CodeValue; // MD5Encryption.Encrypt(registerDto.Phone + registerDto.CodeValue); // MD5Helper.MD5Encrypt32(registerDto.Phone + registerDto.CodeValue);
        }
        else if (registerDto.Method == 2)
        {
            if (!registerDto.Email.IsNotEmptyOrNull())
            {
                throw Oops.Oh("请填写邮箱地址");
            }
            codeSecret = MD5Encryption.Encrypt(registerDto.Email + registerDto.CodeValue); // MD5Helper.MD5Encrypt32(registerDto.Email + registerDto.CodeValue);
        }
        else
        {
            if (!registerDto.UserName.IsNotEmptyOrNull())
            {
                throw Oops.Oh("请填写用户名");
            }
            codeSecret = registerDto.CodeValue;
        }
        if (code.ToLower() != codeSecret?.ToLower())
        {
            throw Oops.Oh("验证码有误，请重新获取");
        }
        //检查站点是否正确
        if (!await _memberService.Context.Queryable<Sites>().AnyAsync(x => x.Id == registerDto.SiteId))
        {
            throw Oops.Oh("所属站点不存在或已删除");
        }
        //取得会员参数设置
        var jsonData = await _memberService.Context.Queryable<SysConfig>().FirstAsync(x => x.Type == ConfigType.MemberConfig.ToString());
        if (jsonData == null)
        {
            throw Oops.Oh("会员设置参数有误，请联系管理员");
        }
        var memberConfig = JSON.Deserialize<MemberConfigDto>(jsonData.JsonData);
        if (memberConfig == null)
        {
            throw Oops.Oh("会员设置参数格式有误，请联系管理员");
        }
        var modelDto = registerDto.Adapt<MembersEditDto>(); //将注册DTO映射成修改DTO
        //检查系统是否开放注册
        if (memberConfig.regStatus == 1)
        {
            throw Oops.Oh("系统暂停开放注册，请稍候再试");
        }
        //检查保留用户名关健字
        if (memberConfig.regKeywords != null && modelDto.userName != null)
        {
            var keywords = memberConfig.regKeywords.Split(',');
            if (keywords.Any(x => x.ToLower().Equals(modelDto.userName.ToLower())))
            {
                throw Oops.Oh("用户名被系统保留，请更换");
            }
        }
        //检查同一IP注册的时间间隔
        if (memberConfig.regCtrl > 0)
        {
            //获取客户IP地址
            var userIp = App.HttpContext?.Connection.RemoteIpAddress?.ToString();
            //查找同一IP下最后的注册用户
            if (await _memberService.AnyAsync(x => x.RegIp != null && x.RegIp.Equals(userIp)
                && DateTime.Compare(x.RegTime.AddHours(memberConfig.regCtrl), DateTime.Now) > 0))
            {
                throw Oops.Oh($"同IP注册过于频繁，请稍候再试");
            }
        }
        //新用户是否开启人工审核
        if (memberConfig.regVerify == 1)
        {
            modelDto.status = 2;
        }

        var result = await this.AddAsync(modelDto);
        //MemoryCacheHelper.Remove(registerDto.CodeKey); //删除验证码缓存
        await cache.DelAsync(cacheKey);
        return Ok(result);
    }

    /// <summary>
    /// 根据当前会员信息
    /// 示例：/account/member/info
    /// </summary>
    [HttpGet("/account/member/info")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetById([FromQuery] MemberParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<MembersDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录").StatusCode(StatusCodes.Status401Unauthorized);
        }
        //查询数据库获取实体
        var model = await _memberService.AsQueryable()
            .Includes(x => x.User)
            .Includes(x => x.Group)
            .Where(x => x.UserId == userId)
            .FirstAsync();
        //var model = await _memberService.QueryAsync(x => x.UserId == userId, WriteRoRead.Write);
        if (model == null)
        {
            throw Oops.Oh($"会员{userId}不存在或已删除");
        }
        //根据字段进行塑形
        var result = model.Adapt<MembersDto>(); //.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/account/member/info
    /// </summary>
    [HttpPut("/account/member/info")]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] MembersModifyDto modelDto)
    {
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录").StatusCode(StatusCodes.Status401Unauthorized);
        }
        var model = await _memberService.AsQueryable()
            .Where(x => x.UserId == userId)
            .FirstAsync();

        _memberService.Context.Tracking(model);

        modelDto.Adapt(model);
        await _memberService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 修改当前会员密码
    /// 示例：/account/member/password
    /// </summary>
    [HttpPut("/account/member/password")]
    [Authorize]
    public async Task<IActionResult> AccountPassword([FromBody] PasswordDto modelDto)
    {
        await this.UpdatePasswordAsync(modelDto);
        return NoContent();
    }

    /// <summary>
    /// 修改当前用户密码
    /// </summary>
    public async Task<bool> UpdatePasswordAsync(PasswordDto modelDto)
    {
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("尚未登录或已超时，请登录后操作").StatusCode(StatusCodes.Status401Unauthorized);
        }
        var model = await _memberService.Context.Queryable<UserEntity>().Where(x => x.Id == userId).FirstAsync();
        if (modelDto.NewPassword != modelDto.ConfirmPassword)
        {
            throw Oops.Oh("两次输入的密码不一至，请重试");
        }
        //if (!await _userManager.CheckPasswordAsync(model, modelDto.Password))
        //{
        //    throw Oops.Oh("旧密码不正确，请重试");
        //}

        //var result = await _userManager.ChangePasswordAsync(model, modelDto.Password, modelDto.NewPassword);
        //if (!result.Succeeded)
        //{
        //    throw Oops.Oh($"错误代码：{result.Errors.FirstOrDefault()?.Code}");
        //}
        return true;
    }
    #endregion

    #region 前台调用接口============================
    /// <summary>
    /// 获取当前会员信息(公共)
    /// 示例：/client/member
    /// </summary>
    [HttpGet("/client/member")]
    [AllowAnonymous, NonUnify]
    public async Task<IActionResult> ClientGetById([FromQuery] MemberParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<MembersDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录").StatusCode(StatusCodes.Status404NotFound);
        }
        //查询数据库获取实体
        var model = await _memberService.SingleAsync(x => x.UserId == userId);
        if (model == null)
        {
            throw Oops.Oh($"会员{userId}不存在或已删除");
        }
        //根据字段进行塑形
        var result = model.Adapt<MembersDto>().ShapeData(param.Fields);
        return Ok(result);
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 添加会员
    /// </summary>
    [SqlSugarUnitOfWork]
    private async Task<MembersDto> AddAsync(MembersEditDto modelDto)
    {
        //判断用户名邮箱手机是否为空
        if (!modelDto.userName.IsNotEmptyOrNull()
            && !modelDto.email.IsNotEmptyOrNull() && !modelDto.phone.IsNotEmptyOrNull())
        {
            throw Oops.Oh("用户名、邮箱、手机至少填写一项");
        }
        //判断密码是否为空
        if (!modelDto.password.IsNotEmptyOrNull())
        {
            throw Oops.Oh("请输入登录密码");
        }
        //如果用户名为空，则自动生成用户名
        if (!modelDto.userName.IsNotEmptyOrNull())
        {
            modelDto.userName = SnowflakeIdHelper.GetGuidToNumber();
        }
        //判断用户名是否重复
        if (await _memberService.Context.Queryable<UserEntity>().AnyAsync(x => x.Account == modelDto.userName))
        {
            throw Oops.Oh("用户名已重复，请更换");
        }
        //判断邮箱是否重复
        if (modelDto.email.IsNotEmptyOrNull()
            && await _memberService.Context.Queryable<UserEntity>().AnyAsync(x => x.Email == modelDto.email))
        {
            throw Oops.Oh("邮箱地址已重复，请更换");
        }
        //判断手机号是否重复
        if (modelDto.phone.IsNotEmptyOrNull()
            && await _memberService.Context.Queryable<UserEntity>().AnyAsync(x => x.MobilePhone == modelDto.phone))
        {
            throw Oops.Oh("手机号码已重复，请更换");
        }
        //检查会员组是否存在
        if (modelDto.groupId <= 0)
        {
            var group = await _memberService.Context.Queryable<MemberGroup>().FirstAsync(x => x.IsDefault == 1);
            if (group == null)
            {
                throw Oops.Oh("没有找到默认会员组");
            }
            modelDto.groupId = (int)group.Id;
        }
        //获取会员角色
        var role = await _memberService.Context.Queryable<RoleEntity>().FirstAsync(x => x.EnCode == RoleType.Member.ToString());
        //var role = await QueryAsync<ApplicationRole>(x => x.RoleType == (byte)RoleType.Member);
        if (role == null)
        {
            throw Oops.Oh("会员角色[Member]不存在");
        }
        //创建用户对象
        var user = new UserEntity()
        {
            Id = SnowflakeIdHelper.NextId(),
            Account = modelDto.userName,
            Email = modelDto.email,
            MobilePhone = modelDto.phone,
            EnabledMark = modelDto.status,
            RealName = modelDto.realName
        };
        if (modelDto.status == 3)
        {
            user.LockMark = 1;
        }
        else if (modelDto.status == 2)
        {
            user.EnabledMark = 3;
        }
        if (modelDto.status == 1)
        {
            user.LockMark = 0;
        }
        if (modelDto.status == 0)
        {
            user.EnabledMark = 1;
        }

        //将用户与角色关联
        UserRelationEntity relation = new UserRelationEntity()
        {
            Id = SnowflakeIdHelper.NextId(),
            UserId = user.Id,
            ObjectType = "Role",
            ObjectId = role.Id
        };

        //user.UserRoles = new List<ApplicationUserRole>()
        //    {
        //        new ApplicationUserRole()
        //        {
        //            RoleId=role.Id,
        //            UserId=user.Id
        //        }
        //    };
        //将用户会员信息关联
        var member = new Members()
        {
            SiteId = modelDto.siteId,
            UserId = user.Id,
            GroupId = modelDto.groupId,
            Avatar = modelDto.avatar,
            RealName = modelDto.realName,
            Sex = modelDto.sex,
            Birthday = modelDto.birthday,
            RegIp = App.HttpContext?.Connection.RemoteIpAddress.ToString(),
            RegTime = DateTime.Now
        };
        //HASH密码，保存用户
        user.Secretkey = Guid.NewGuid().ToString();
        user.Password = MD5Encryption.Encrypt(MD5Encryption.Encrypt(modelDto.password) + user.Secretkey);

        await _memberService.Context.Insertable<UserEntity>(user).ExecuteCommandAsync();
        await _memberService.Context.Insertable<UserRelationEntity>(relation).ExecuteCommandAsync();
        await _memberService.Context.Insertable<Members>(member).ExecuteCommandAsync();

        //var result = await _userManager.CreateAsync(user, modelDto.Password);
        //if (!result.Succeeded)
        //{
        //    throw Oops.Oh($"{result.Errors.FirstOrDefault()?.Description}");
        //}

        var model = await _memberService.SingleAsync(x => x.UserId == user.Id);
        var resultModel = model.Adapt<MembersDto>();

        return resultModel;
    }


    /// <summary>
    /// 修改会员
    /// </summary>
    private async Task<bool> UpdateAsync(string userId, MembersEditDto modelDto)
    {
        //查找记录会员及用户信息
        var model = await _memberService.SingleAsync(x => x.UserId == userId);
        if (model == null)
        {
            throw Oops.Oh("会员不存在或已删除");
        }
        //检查用户信息是否存在
        var user = await _memberService.Context.Queryable<UserEntity>().InSingleAsync(userId.ToString());
        if (user == null)
        {
            throw Oops.Oh("会员不存在或已删除");
        }
        //如果用户名发生改变，则检查重复
        if (modelDto.userName.IsNotEmptyOrNull() && user.RealName != modelDto.userName
            && await _memberService.Context.Queryable<UserEntity>().AnyAsync(x => x.Id != userId && x.RealName == modelDto.userName))
        {
            throw Oops.Oh("用户名已重复，请更换");
        }
        //如果邮箱发生改变，则检查重复
        if (modelDto.email.IsNotEmptyOrNull() && user.Email != modelDto.email
            && await _memberService.Context.Queryable<UserEntity>().AnyAsync(x => x.Id != userId && x.Email == modelDto.email))
        {
            throw Oops.Oh("邮箱地址已重复，请更换");
        }
        //如果手机号发生改变，则检查重复
        if (modelDto.phone.IsNotEmptyOrNull() && user.MobilePhone != modelDto.phone
            && await _memberService.Context.Queryable<UserEntity>().AnyAsync(x => x.Id != userId && x.MobilePhone == modelDto.phone))
        {
            throw Oops.Oh("手机号码已重复，请更换");
        }

        _memberService.Context.Tracking(model);
        //用户信息
        if (modelDto.userName.IsNotEmptyOrNull())
        {
            user.RealName = modelDto.userName;
        }
        user.Email = modelDto.email;
        user.MobilePhone = modelDto.phone;
        if (modelDto.status == 3)
        {
            user.LockMark = 1;
        }
        else if(modelDto.status == 2)
        {
            user.EnabledMark = 3;
        }
        if (modelDto.status == 1)
        {
            user.LockMark = 0;
        }
        if (modelDto.status == 0)
        {
            user.EnabledMark = 1;
        }

        //会员信息
        var member = new Members()
        {
            Id = model.Id,
            UserId = model.UserId,
            Amount = model.Amount,
            Point = model.Point,
            Exp = model.Exp,
            RegIp = model.RegIp,
            RegTime = model.RegTime,
            LastIp = model.LastIp,
            LastTime = model.LastTime,

            SiteId = modelDto.siteId,
            GroupId = modelDto.groupId,
            Avatar = modelDto.avatar,
            RealName = modelDto.realName,
            Sex = modelDto.sex,
            Birthday = modelDto.birthday
        };

        await _memberService.Context.AutoUpdateAsync(user);
        var ok = await _memberService.UpdateAsync(member);

        return ok > 0;

        //var result = await _userManager.UpdateAsync(user);
        ////异常错误提示
        //if (!result.Succeeded)
        //{
        //    throw Oops.Oh($"{result.Errors.FirstOrDefault()?.Description}");
        //}
        ////如果密码不为空，则重置密码
        //if (modelDto.Password.IsNotEmptyOrNull())
        //{
        //    //生成token，用于重置密码
        //    string token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //    //重置密码
        //    await _userManager.ResetPasswordAsync(user, token, modelDto.Password);
        //}
        //return true;
    }
    #endregion
}
