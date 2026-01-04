using Microsoft.AspNetCore.Http;
using QT.CMS.Emum;
using QT.CMS.Entitys.Dto.Member;
using QT.DataValidation;
using QT.Systems.Entitys.Permission;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 会员积分记录
/// </summary>
[Route("api/cms/admin/member/point")]
[ApiController]
public class MemberPointController : ControllerBase
{
    private readonly ISqlSugarRepository<MemberPointLog> _memberPointLogService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public MemberPointController(ISqlSugarRepository<MemberPointLog> memberPointLogService, IUserService userService)
    {
        _memberPointLogService = memberPointLogService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/member/point/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberPoint", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<MemberPointLogDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var resultFrom = await _memberPointLogService.AsQueryable()
        .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
        .Where((a, u) => a.Id == id)
        .Select((a, u) => new MemberPointLogDto
        {
            id = a.Id,
            userId = a.UserId,
            userName = u.Account,
            value = a.Value,
            remark = a.Remark,
            addTime = a.AddTime
        })
        .FirstAsync();
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
    /// 示例：/admin/member/point?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("MemberPoint", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberPointLogDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberPointLogDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _memberPointLogService.AsQueryable()
       .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
       .WhereIF(searchParam.keyword.IsNotEmptyOrNull(),(a,u)=> u.Account.Contains(searchParam.keyword))
       .Select((a, u) => new MemberPointLogDto
       {
           id = a.Id,
           userId = a.UserId,
           userName = u.Account,
           value = a.Value,
           remark = a.Remark,
           addTime = a.AddTime
       })
       .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
         .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        return PageResult<MemberPointLogDto>.SqlSugarPageResult(list);

        ////根据字段进行塑形，注意因为没有使用AotoMapper，所以要转换成Enumerable
        //var resultDto = list.AsEnumerable().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/member/point
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("MemberPoint", ActionType.Add)]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> Add([FromBody] MemberPointLogEditDto modelDto)
    {
        //检查会员是否存在
        if (!await _memberPointLogService.Context.Queryable<Members>().AnyAsync(x => x.UserId == modelDto.userId))
        {
            throw Oops.Oh($"会员ID[{modelDto.userId}]不存在");
        }
        //映射成实体
        var model = modelDto.Adapt<MemberPointLog>();
        //写入数据库
        await this.AddAsync(model);
        //查询刚添加的记录
        var result = await _memberPointLogService.AsQueryable()
        .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
        .Where((a, u) => a.Id == model.Id)
        .Select((a, u) => new MemberPointLogDto
        {
            id = a.Id,
            userId = a.UserId,
            userName = u.Account,
            value = a.Value,
            remark = a.Remark,
            addTime = a.AddTime
        })
        .FirstAsync();
        return Ok(result);
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/member/point/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberPoint", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        //检查记录是否存在
        if (!await _memberPointLogService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _memberPointLogService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/admin/member/point?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("MemberPoint", ActionType.Delete)]
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
        await _memberPointLogService.DeleteAsync(x => listIds.Contains(x.Id));

        return NoContent();
    }
    #endregion

    #region 当前用户调用接口========================
    /// <summary>
    /// 获取指定数量列表
    /// 示例：/account/member/point/view/10
    /// </summary>
    [HttpGet("/account/member/point/view/{top}")]
    [Authorize]
    [NonUnify]
    public async Task<IEnumerable<MemberPointLogDto>> AccountGetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberPointLogDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberPointLogDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        var userId = await _userService.GetUserIdAsync();

        //获取数据库列表
        var list = await _memberPointLogService.AsQueryable()
        .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
        .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), (a, u) => u.Account.Contains(searchParam.Keyword))
        .Select((a, u) => new MemberPointLogDto
        {
            id = a.Id,
            userId = a.UserId,
            userName = u.Account,
            value = a.Value,
            remark = a.Remark,
            addTime = a.AddTime
        })
        .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
        .AutoTake(top)
        .ToListAsync();

        //根据字段进行塑形，注意因为没有使用AotoMapper，所以要转换成Enumerable
        var resultDto = list.AsEnumerable<MemberPointLogDto>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return resultDto;
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/account/member/point?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/account/member/point")]
    [Authorize]
    [NonUnify]
    public async Task<dynamic> AccountGetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberPointLogDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberPointLogDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        string userId = await _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }
        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _memberPointLogService.AsQueryable()
        .InnerJoin<UserEntity>((a, u) => a.UserId == u.Id)
        .Where( (a, u) => a.UserId == userId)
        .Select((a, u) => new MemberPointLogDto
        {
            id = a.Id,
            userId = a.UserId,
            userName = u.Account,
            value = a.Value,
            remark = a.Remark,
            addTime = a.AddTime
        })
        .OrderBy(searchParam.OrderBy ?? "-AddTime,-Id")
        .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);


        return PageResult<MemberPointLogDto>.SqlSugarPageResult(list);

        //var list = await _memberPointLogService.QueryPageAsync(
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
        //var resultDto = list.AsEnumerable().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }
    #endregion


    #region Private Methods
    /// <summary>
    /// 新增一条数据，须检查是否正数，正数须同时增加会员积分及经验值，检查升级
    /// </summary>
    private async Task<MemberPointLog> AddAsync(MemberPointLog model)
    {

        var userModel = await _memberPointLogService.Context.Queryable<Members>().FirstAsync(x => x.UserId == model.UserId);
        if (userModel == null)
        {
            throw Oops.Oh("会员账户不存在或已删除");
        }
        _memberPointLogService.Context.Tracking(userModel);
        //如果是负数，检查会员积分是否够扣减
        if (model.Value < 0 && userModel.Point < (model.Value * -1))
        {
            throw Oops.Oh("会员账户积分不足本次扣减");
        }
        userModel.Point += model.Value;//添加积分
        //如果是正数则增加经验值
        if (model.Value > 0)
        {
            userModel.Exp += model.Value;
            //查询当前会员组
            var currGroupModel = await _memberPointLogService.Context.Queryable<MemberGroup>().FirstAsync(x => x.Id == userModel.GroupId);
            if (currGroupModel == null)
            {
                throw Oops.Oh("会员组不存在或已删除");
            }
            //检查有无可升级的会员组
            var upgradeGroupModel = await _memberPointLogService.Context.Queryable<MemberGroup>()
                .Where(x => x.Id != currGroupModel.Id && x.IsUpgrade == 1 && x.MinExp >= currGroupModel.MaxExp
                && x.MinExp <= userModel.Exp
                && x.MaxExp >= userModel.Exp)
                .FirstAsync();
            if (upgradeGroupModel != null && upgradeGroupModel.Amount >= currGroupModel.Amount)
            {
                userModel.GroupId = (int)upgradeGroupModel.Id;
            }
        }
        //新增积分记录
        await _memberPointLogService.InsertReturnEntityAsync(model);
        await _memberPointLogService.Context.AutoUpdateAsync(userModel);

        return model;
    } 
    #endregion
}