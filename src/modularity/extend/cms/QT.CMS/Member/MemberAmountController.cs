using Microsoft.AspNetCore.Http;
using NPOI.XWPF.UserModel;
using QT.CMS.Emum;
using QT.CMS.Entitys.Dto.Member;
using QT.Systems.Entitys.Permission;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 会员余额记录
/// </summary>
[Route("api/cms/admin/member/amount")]
[ApiController]
public class MemberAmountController : ControllerBase
{
    private readonly ISqlSugarRepository<MemberAmountLog> _memberAmountLogService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public MemberAmountController(ISqlSugarRepository<MemberAmountLog> memberAmountLogService, IUserService userService)
    {
        _memberAmountLogService = memberAmountLogService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/member/amount/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberAmount", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<MemberAmountLogDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var resultFrom = await _memberAmountLogService.AsQueryable()
      .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
      .Where((a, u) => a.Id == id)
      .Select((a, u) => new MemberAmountLogDto
      {
          id = SqlFunc.ToString(a.Id),
          userId = a.UserId,
          userName = u.Account,
          value = a.Value,
          remark = a.Remark,
          addTime = a.AddTime
      })
      .FirstAsync(); 
        if (resultFrom == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //根据字段进行塑形
        var result = resultFrom.ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/member/amount?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("MemberAmount", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberAmountLogDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberAmountLogDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表

        var list = await _memberAmountLogService.AsQueryable()
     .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
     .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), (a, u) => u.Account.Contains(searchParam.keyword))
     .Select((a, u) => new MemberAmountLogDto
     {
         id = SqlFunc.ToString(a.Id),
         userId = a.UserId,
         userName = u.Account,
         value = a.Value,
         remark = a.Remark,
         addTime = a.AddTime
     })
     .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
       .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        return PageResult<MemberAmountLogDto>.SqlSugarPageResult(list);


        //var list = await _memberAmountLogService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => !searchParam.Keyword.IsNotNullOrEmpty() || (x.UserName != null && x.UserName.Contains(searchParam.Keyword)),
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
        //var resultDto = list.AsEnumerable<MemberAmountLogDto>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }


    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/member/amount
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("MemberAmount", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] MemberAmountLogEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<MemberAmountLog>();
        //写入数据库
        await this.AddAsync(model);
        //重新联合查询
        var result = await _memberAmountLogService.AsQueryable()
     .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
     .Where((a, u) => a.Id == model.Id)
     .Select((a, u) => new MemberAmountLogDto
     {
         id = SqlFunc.ToString(a.Id),
         userId = a.UserId,
         userName = u.Account,
         value = a.Value,
         remark = a.Remark,
         addTime = a.AddTime
     })
     .FirstAsync();

        //var result = await _memberAmountLogService.QueryAsync(x => x.Id == model.Id);
        return Ok(result);
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/member/amount/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberAmount", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        //检查记录是否存在
        if (!await _memberAmountLogService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _memberAmountLogService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/admin/member/amount?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("MemberAmount", ActionType.Delete)]
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
        await _memberAmountLogService.DeleteAsync(x => listIds.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 当前用户调用接口========================
    /// <summary>
    /// 获取指定数量列表
    /// 示例：/account/member/amount/view/10
    /// </summary>
    [HttpGet("/account/member/amount/view/{top}")]
    [Authorize]
    [NonUnify]
    public async Task<IEnumerable<MemberAmountLogDto>> AccountGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberAmountLogDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberAmountLogDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();

        //获取数据库列表
        var list = await _memberAmountLogService.AsQueryable()
        .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
        .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), (a, u) => u.Account.Contains(searchParam.Keyword))
        .Select((a, u) => new MemberAmountLogDto
        {
            id = SqlFunc.ToString(a.Id),
            userId = a.UserId,
            userName = u.Account,
            value = a.Value,
            remark = a.Remark,
            addTime = a.AddTime
        })
        .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
        .AutoTake(top)
        .ToListAsync();

        //var list = await _memberAmountLogService.QueryListAsync(top,
        //    x => x.UserId == userId
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.UserName != null && x.UserName.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-AddTime,-Id");

        //根据字段进行塑形，注意因为没有使用AotoMapper，所以要转换成Enumerable
        var resultDto = list.AsEnumerable<MemberAmountLogDto>();//.ShapeData(searchParam.Fields);
        //返回成功200
        return (resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/account/member/amount?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/account/member/amount")]
    [Authorize]
    [NonUnify]
    public async Task<dynamic> AccountGetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberAmountLogDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberAmountLogDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }
        //获取数据列表
        var list = await _memberAmountLogService.AsQueryable()
      .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
      .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), (a, u) => u.Account.Contains(searchParam.Keyword))
      .Select((a, u) => new MemberAmountLogDto
      {
          id = SqlFunc.ToString(a.Id),
          userId = a.UserId,
          userName = u.Account,
          value = a.Value,
          remark = a.Remark,
          addTime = a.AddTime
      })
      .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
        .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);
        return PageResult<MemberAmountLogDto>.SqlSugarPageResult(list);

        //var list = await _memberAmountLogService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.UserId == userId,
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
        //var resultDto = list.AsEnumerable<MemberAmountLogDto>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 新增余额记录，须同时增加会员余额，检查升级
    /// </summary>
    private async Task<MemberAmountLog> AddAsync(MemberAmountLog model)
    {
        var userModel = await _memberAmountLogService.Context.Queryable<Members>().FirstAsync(x => x.UserId == model.UserId);
        if (userModel == null)
        {
            throw Oops.Oh("会员账户不存在或已删除");
        }
        //如果是负数，检查会员余额是否够扣减
        if (model.Value < 0 && userModel.Amount < (model.Value * -1))
        {
            throw Oops.Oh("会员账户余额不足本次扣减");
        }
        userModel.Amount += model.Value;//添减余额
        //如果是正数则检查是否需要升级
        if (model.Value > 0)
        {
            //查询当前会员组
            var currGroupModel = await _memberAmountLogService.Context.Queryable<MemberGroup>().FirstAsync(x => x.Id == userModel.GroupId);
            if (currGroupModel == null)
            {
                throw Oops.Oh("会员组不存在或已删除");
            }
            //检查有无可升级的会员组
            var upgradeGroupModel = await _memberAmountLogService.Context.Queryable<MemberGroup>()
                .Where(x => x.Id != currGroupModel.Id && x.IsUpgrade == 1 && x.Amount <= model.Value)
                .OrderByDescending(x=>x.Amount)
                .FirstAsync();
            //var upgradeGroupModel = await _memberGroupService.QueryAsync(x =>
            //x.Id != currGroupModel.Id && x.IsUpgrade == 1 && x.Amount <= model.Value, "-Amount");
            if (upgradeGroupModel != null && upgradeGroupModel.Amount >= currGroupModel.Amount)
            {
                userModel.GroupId = (int)upgradeGroupModel.Id;
            }
        }
        //新增余额记录，更新用户余额
        await _memberAmountLogService.InsertReturnEntityAsync(model);
        await _memberAmountLogService.Context.AutoUpdateAsync(userModel);

        return model;
    } 
    #endregion
}