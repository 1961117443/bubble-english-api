using QT.CMS.Emum;
using QT.CMS.Entitys.Dto.Member;
using QT.Systems.Entitys.Permission;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 会员收货地址
/// </summary>
[Route("api/cms/admin/member/address")]
[ApiController]
public class MemberAddressController : ControllerBase
{
    private readonly ISqlSugarRepository<MemberAddress> _memberAddressService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public MemberAddressController(ISqlSugarRepository<MemberAddress> memberAddressService, IUserService userService)
    {
        _memberAddressService = memberAddressService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/member/address/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberAddress", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<MemberAddressDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var resultFrom = await _memberAddressService.AsQueryable()
     .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
     .Where((a, u) => a.Id == id)
     .Select((a, u) => new MemberAddressDto
     {
         id = a.Id,
         userId = a.UserId,
         userName = u.Account,
         acceptName = a.AcceptName,
         province = a.Province,
         city = a.City,
         area = a.Area,
         address = a.Address,
         telphone = a.Telphone,
         mobile = a.Mobile,
         zip = a.Zip,
         isDefault = a.IsDefault,
         addTime = a.AddTime
     })
     .FirstAsync();

        //var resultFrom = await _memberAddressService.QueryAsync(x => x.Id == id, WriteRoRead.Write);
        if (resultFrom == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //根据字段进行塑形
        var result = resultFrom.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/admin/member/address/view/0
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    [AuthorizeFilter("MemberAddress", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberAddressDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberAddressDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据库列表
        var result = await _memberAddressService.AsQueryable()
     .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
     .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), (a, u) => u.Account.Contains(searchParam.keyword))
     .Select((a, u) => new MemberAddressDto
     {
         id = a.Id,
         userId = a.UserId,
         userName = u.Account,
         acceptName = a.AcceptName,
         province = a.Province,
         city = a.City,
         area = a.Area,
         address = a.Address,
         telphone = a.Telphone,
         mobile = a.Mobile,
         zip = a.Zip,
         isDefault = a.IsDefault,
         addTime = a.AddTime
     })
     .AutoTake(top)
     .ToListAsync();

        //var resultFrom = await _memberAddressService.QueryListAsync(top,
        //    x => !searchParam.Keyword.IsNotNullOrEmpty() || (x.AcceptName != null && x.AcceptName.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy ?? "AddTime");
        ////根据字段进行塑形
        //var result = resultFrom.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(result);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/member/address?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("MemberAddress", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberAddressDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberAddressDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _memberAddressService.AsQueryable()
  .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
  .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), (a, u) => a.AcceptName.Contains(searchParam.keyword))
  .Select((a, u) => new MemberAddressDto
  {
      id = a.Id,
      userId = a.UserId,
      userName = u.Account,
      acceptName = a.AcceptName,
      province = a.Province,
      city = a.City,
      area = a.Area,
      address = a.Address,
      telphone = a.Telphone,
      mobile = a.Mobile,
      zip = a.Zip,
      isDefault = a.IsDefault,
      addTime = a.AddTime
  })
     .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
       .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        return PageResult<MemberAddressDto>.SqlSugarPageResult(list);

        //var list = await _memberAddressService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => !searchParam.Keyword.IsNotNullOrEmpty() || (x.AcceptName != null && x.AcceptName.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

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
        //var resultDto = list.AsEnumerable<MemberAddressDto>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }


    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/member/address
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("MemberAddress", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] MemberAddressEditDto modelDto)
    {
        //检查会员是否存在
        if (!await _memberAddressService.Context.Queryable<Members>().AnyAsync(x => x.UserId == modelDto.userId))
        {
            throw Oops.Oh($"会员ID[{modelDto.userId}]不存在");
        }
        modelDto.address = $"{modelDto.province},{modelDto.city},{modelDto.area},{modelDto.address}";
        //映射成实体
        var model = modelDto.Adapt<MemberAddress>();
        //写入数据库
        await this.AddAsync(model);
        //重新联合查询
        var result = await _memberAddressService.AsQueryable()
   .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
   .Where((a, u) => a.Id == model.Id)
   .Select((a, u) => new MemberAddressDto
   {
       id = a.Id,
       userId = a.UserId,
       userName = u.Account,
       acceptName = a.AcceptName,
       province = a.Province,
       city = a.City,
       area = a.Area,
       address = a.Address,
       telphone = a.Telphone,
       mobile = a.Mobile,
       zip = a.Zip,
       isDefault = a.IsDefault,
       addTime = a.AddTime
   })
   .FirstAsync();

        //var result = await _memberAddressService.QueryAsync(x => x.Id == model.Id, WriteRoRead.Write);
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/member/address/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberAddress", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] MemberAddressEditDto modelDto)
    {
        //查找记录
        var model = await _memberAddressService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _memberAddressService.Context.Tracking(model);
        //保存到数据库
        if (modelDto.address != null)
        {
            var addressArr = modelDto.address.Split(',');//将详细地址拆分，重新组合
            modelDto.address = $"{modelDto.province},{modelDto.city},{modelDto.area},{addressArr[^1]}";
        }
        modelDto.Adapt(model);
        var result = await _memberAddressService.AutoUpdateAsync(model);

        //如果当前设置为默认，则取消该用户下所有默认数据
        if (model.IsDefault == 1)
        {
            var list = await _memberAddressService.Context.Updateable<MemberAddress>()
                .Where(x => x.UserId == model.UserId && x.Id != model.Id && x.IsDefault == 1)
                .SetColumns(x => x.IsDefault, 0)
                .ExecuteCommandAsync();
        }
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/member/address/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberAddress", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] JsonPatchDocument<MemberAddressEditDto> patchDocument)
    {
        //检查记录是否存在
        var model = await _memberAddressService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var modelToPatch = model.Adapt<MemberAddressEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        _memberAddressService.Context.Tracking(model);
        modelToPatch.Adapt(model);
        await _memberAddressService.AutoUpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/member/address/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberAddress", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        //检查参数是否正确
        if (!await _memberAddressService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _memberAddressService.DeleteAsync(x => x.Id == id);
        return NoContent();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/admin/member/address?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("MemberAddress", ActionType.Delete)]
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
        await _memberAddressService.DeleteAsync(x => listIds.Contains(x.Id));
        return NoContent();
    }
    #endregion

    #region 当前用户调用接口========================
    /// <summary>
    /// 获取用户一条默认数据
    /// 示例：/account/member/address
    /// </summary>
    [HttpGet("/account/member/address")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetByUserId([FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<MemberAddressDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        //查询数据库获取实体
        var resultFrom = await _memberAddressService.AsQueryable()
.InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
.Where((a, u) => a.UserId == userId && a.IsDefault == 1)
.Select((a, u) => new MemberAddressDto
{
   id = a.Id,
   userId = a.UserId,
   userName = u.Account,
   acceptName = a.AcceptName,
   province = a.Province,
   city = a.City,
   area = a.Area,
   address = a.Address,
   telphone = a.Telphone,
   mobile = a.Mobile,
   zip = a.Zip,
   isDefault = a.IsDefault,
   addTime = a.AddTime
})
.FirstAsync();
        //var resultFrom = await _memberAddressService.QueryAsync(x => x.UserId == userId && x.IsDefault == 1);
        if (resultFrom == null)
        {
            throw Oops.Oh("暂无默认收货地址");
        }
        //根据字段进行塑形
        var result = resultFrom.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 根据ID获取用户一条数据
    /// 示例：/account/member/address/1
    /// </summary>
    [HttpGet("/account/member/address/{id}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<MemberAddressDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        //查询数据库获取实体
        var resultFrom = await _memberAddressService.AsQueryable()
.InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
.Where((a, u) => a.UserId == userId && a.Id == id)
.Select((a, u) => new MemberAddressDto
{
id = a.Id,
userId = a.UserId,
userName = u.Account,
acceptName = a.AcceptName,
province = a.Province,
city = a.City,
area = a.Area,
address = a.Address,
telphone = a.Telphone,
mobile = a.Mobile,
zip = a.Zip,
isDefault = a.IsDefault,
addTime = a.AddTime
})
.FirstAsync();

        //var resultFrom = await _memberAddressService.QueryAsync(x => x.UserId == userId && x.Id == id);
        if (resultFrom == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //根据字段进行塑形
        var result = resultFrom.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定数量列表
    /// 示例：/account/member/address/view/0
    /// </summary>
    [HttpGet("/account/member/address/view/{top}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberAddressDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberAddressDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        //获取数据库列表
        var resultFrom = await _memberAddressService.AsQueryable()
.InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
.Where((a, u) => a.UserId == userId)
.WhereIF(searchParam.keyword.IsNotEmptyOrNull(), (a, u) => a.AcceptName.Contains(searchParam.Keyword))
.Select((a, u) => new MemberAddressDto
{
    id = a.Id,
    userId = a.UserId,
    userName = u.Account,
    acceptName = a.AcceptName,
    province = a.Province,
    city = a.City,
    area = a.Area,
    address = a.Address,
    telphone = a.Telphone,
    mobile = a.Mobile,
    zip = a.Zip,
    isDefault = a.IsDefault,
    addTime = a.AddTime
})
.OrderBy(searchParam.OrderBy ?? "-IsDefault,-AddTime")
.AutoTake(top)
.ToListAsync();

        //var resultFrom = await _memberAddressService.QueryListAsync(top,
        //    x => x.UserId == userId
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.AcceptName != null && x.AcceptName.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-IsDefault,-AddTime");
        //根据字段进行塑形
        //var result = resultFrom.ShapeData(searchParam.Fields);
        ////返回成功200
        return Ok(resultFrom);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/account/member/address/list?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/account/member/address/list")]
    [Authorize]
    [NonUnify]
    public async Task<dynamic> AccountGetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberAddressDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberAddressDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _memberAddressService.AsQueryable()
.InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
.Where((a, u) => a.UserId == userId)
.WhereIF(searchParam.keyword.IsNotEmptyOrNull(), (a, u) => a.AcceptName.Contains(searchParam.Keyword))
.Select((a, u) => new MemberAddressDto
{
 id = a.Id,
 userId = a.UserId,
 userName = u.Account,
 acceptName = a.AcceptName,
 province = a.Province,
 city = a.City,
 area = a.Area,
 address = a.Address,
 telphone = a.Telphone,
 mobile = a.Mobile,
 zip = a.Zip,
 isDefault = a.IsDefault,
 addTime = a.AddTime
})
.OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
 .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);
        return PageResult<MemberAddressDto>.SqlSugarPageResult(list);

        //var list = await _memberAddressService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.UserId == userId
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.AcceptName != null && x.AcceptName.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

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
        //var resultDto = list.AsEnumerable<MemberAddressDto>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/account/member/address
    /// </summary>
    [HttpPost("/account/member/address")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountAdd([FromBody] MemberAddressEditDto modelDto)
    {
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        modelDto.userId = userId; //赋值当前用户
        modelDto.address = $"{modelDto.province},{modelDto.city},{modelDto.area},{modelDto.address}";
        //映射成实体
        var model = modelDto.Adapt<MemberAddress>();
        //写入数据库
        await this.AddAsync(model);
        //重新联合查询
        var result = await _memberAddressService.AsQueryable()
 .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
 .Where((a, u) => a.Id == model.Id)
 .Select((a, u) => new MemberAddressDto
 {
     id = a.Id,
     userId = a.UserId,
     userName = u.Account,
     acceptName = a.AcceptName,
     province = a.Province,
     city = a.City,
     area = a.Area,
     address = a.Address,
     telphone = a.Telphone,
     mobile = a.Mobile,
     zip = a.Zip,
     isDefault = a.IsDefault,
     addTime = a.AddTime
 })
 .FirstAsync();

        //var result = await _memberAddressService.QueryAsync(x => x.Id == model.Id, WriteRoRead.Write);
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/account/member/address/1
    /// </summary>
    [HttpPut("/account/member/address/{id}")]
    [Authorize]
    [NonUnify]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> AccountUpdate([FromRoute] long id, [FromBody] MemberAddressEditDto modelDto)
    {
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }
        //查找记录
        var model = await _memberAddressService.SingleAsync(x => x.UserId == userId && x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        _memberAddressService.Context.Tracking(model);
        //赋值保存到数据库
        modelDto.userId = userId;
        if (modelDto.address != null)
        {
            var addressArr = modelDto.address.Split(',');//将详细地址拆分，重新组合
            modelDto.address = $"{modelDto.province},{modelDto.city},{modelDto.area},{addressArr[^1]}";
        }
        modelDto.Adapt(model);
        var result = await _memberAddressService.AutoUpdateAsync(model);

        //如果当前设置为默认，则取消该用户下所有默认数据
        if (model.IsDefault == 1)
        {
            var list = await _memberAddressService.Context.Updateable<MemberAddress>()
                .Where(x => x.UserId == model.UserId && x.Id != model.Id && x.IsDefault == 1)
                .SetColumns(x => x.IsDefault, 0)
                .ExecuteCommandAsync();
        }
        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/account/member/address/1
    /// </summary>
    [HttpDelete("/account/member/address/{id}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountDelete([FromRoute] long id)
    {
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }
        //检查参数是否正确
        if (!await _memberAddressService.AnyAsync(x => x.UserId == userId && x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _memberAddressService.DeleteAsync(x => x.UserId == userId && x.Id == id);
        return NoContent();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// 新增一条数据
    /// </summary>
    private async Task<bool> AddAsync(MemberAddress model, WriteRoRead writeAndRead = WriteRoRead.Write)
    { 
        model.AddTime = DateTime.Now;
        //如果当前设置为默认，则取消该用户下所有默认数据
        if (model.IsDefault == 1)
        {
            var list = await _memberAddressService.AsQueryable().Where(x => x.UserId == model.UserId && x.IsDefault == 1).ToListAsync();
            foreach (var item in list)
            {
                item.IsDefault = 0;
            }
            await _memberAddressService.Context.Updateable<MemberAddress>(list).UpdateColumns(x => x.IsDefault).ExecuteCommandAsync();
        }
        //添加新地址
        var result  =await _memberAddressService.InsertReturnEntityAsync(model);
        //保存到数据库
        //var result = await this.SaveAsync();
        return result != null;
    } 
    #endregion
}