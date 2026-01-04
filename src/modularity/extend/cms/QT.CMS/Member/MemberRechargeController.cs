using QT.CMS.Emum;
using QT.CMS.Entitys.Dto.Member;
using QT.CMS.Interfaces;
using QT.Common.Core.Security;
using QT.Systems.Entitys.Permission;

namespace QT.CMS;

/// <summary>
/// 会员充值记录
/// </summary>
[Route("api/cms/admin/member/recharge")]
[ApiController]
public class MemberRechargeController : ControllerBase
{
    private readonly ISqlSugarRepository<MemberRecharge> _memberRechargeService;
    private readonly IPaymentCollectionService _paymentCollectionService;
    private readonly IUserService _userService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public MemberRechargeController(ISqlSugarRepository<MemberRecharge> memberRechargeService, IPaymentCollectionService paymentCollectionService,
        IUserService userService)
    {
        _memberRechargeService = memberRechargeService;
        _paymentCollectionService = paymentCollectionService;
        _userService = userService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/member/recharge/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberRecharge", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] long id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<MemberRechargeDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _memberRechargeService.AsQueryable()
            .Includes(x => x.Collection)
            .SingleAsync(x => x.Id == id);

        //var model = await _memberRechargeService.QueryAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //根据字段进行塑形
        var result = model.Adapt<MemberRechargeDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/member/recharge?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    [AuthorizeFilter("MemberRecharge", ActionType.View)]
    public async Task<dynamic> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberRechargeDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberRechargeDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _memberRechargeService.AsQueryable()
            .Includes(x => x.Collection)
            .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.UserName.Contains(searchParam.keyword) || x.Collection.TradeNo.Contains(searchParam.keyword))
            .OrderBy(searchParam.OrderBy ?? "-Id")
            .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);
        //var list = await _memberRechargeService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => (!searchParam.Keyword.IsNotNullOrEmpty() || (x.UserName != null && x.UserName.Contains(searchParam.Keyword))),
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
        //var resultDto = _mapper.Adapt<IEnumerable<MemberRechargeDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult<MemberRechargeDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/member/recharge
    /// </summary>
    [HttpPost]
    [Authorize]
    [AuthorizeFilter("MemberRecharge", ActionType.Add)]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> Add([FromBody] MemberRechargeEditDto modelDto)
    {
        //保存充值记录
        var model = await this.AddAsync(modelDto);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<PaymentCollectionDto>();
        return Ok(result);
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/member/recharge/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [AuthorizeFilter("MemberRecharge", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        //检查记录是否存在
        if (!await _memberRechargeService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _memberRechargeService.DeleteAsync(x => x.Id == id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/admin/member/recharge?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    [AuthorizeFilter("MemberRecharge", ActionType.Delete)]
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
        await _memberRechargeService.DeleteAsync(x => listIds.Contains(x.Id));
        return NoContent();
    }

    /// <summary>
    /// 完成充值订单
    /// 示例：/admin/member/recharge?ids=1,2,3
    /// </summary>
    [HttpPut]
    [Authorize]
    [AuthorizeFilter("MemberRecharge", ActionType.Complete)]
    [SqlSugarUnitOfWork]
    public async Task<IActionResult> Complete([FromQuery] string Ids)
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
        //将符合条件的一次查询出来
        var list = await _memberRechargeService.AsQueryable()
            .Includes(x => x.Collection)
            .Where(x => x.Collection.Status == 1 && listIds.Contains(x.Id))
            .OrderBy("-Id")
            .ToListAsync();
        //var list = await _memberRechargeService.QueryListAsync(0, x => x.Collection != null && x.Collection.Status == 1 && listIds.Contains(x.Id), "-Id");
        //执行批量删除操作
        foreach (var modelt in list)
        {
            if (modelt.Collection != null && modelt.Collection.TradeNo != null)
            {
                await _paymentCollectionService.ConfirmAsync(modelt.Collection.TradeNo);
            }
        }
        return NoContent();
    }
    #endregion

    #region 当前用户调用接口========================
    /// <summary>
    /// 获取指定数量列表
    /// 示例：/account/member/recharge/view/10
    /// </summary>
    [HttpGet("/account/member/recharge/view/{top}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberRechargeDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberRechargeDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        var userId = await  _userService.GetUserIdAsync();

        //获取数据库列表
        var list = await _memberRechargeService.AsQueryable()
           .Includes(x => x.Collection)
           .Where(x => x.UserId == userId)
           .WhereIF(searchParam.Status > 0, x => x.Collection.Status == searchParam.Status)
           .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.UserName.Contains(searchParam.keyword))
           .OrderBy(searchParam.OrderBy ?? "-Id")
           .AutoTake(top)
           .ToListAsync();

        //var list = await _memberRechargeService.QueryListAsync(top,
        //    x => x.UserId == userId
        //    && (searchParam.Status <= 0 || (x.Collection != null && x.Collection.Status == searchParam.Status))
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.UserName != null && x.UserName.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "-Id");

        //映射成DTO，根据字段进行塑形
        var resultDto = list.Adapt<IEnumerable<MemberRechargeDto>>(); //.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/account/member/recharge?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet("/account/member/recharge")]
    [Authorize]
    [NonUnify]
    public async Task<dynamic> AccountList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<MemberRechargeDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<MemberRechargeDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取登录用户ID
        var userId = await  _userService.GetUserIdAsync();

        //获取数据列表，如果ID大于0则查询该用户下所有的列表
        var list = await _memberRechargeService.AsQueryable()
          .Includes(x => x.Collection)
          .Where(x => x.UserId == userId)
          .WhereIF(searchParam.Status > 0, x => x.Collection.Status == searchParam.Status)
          .WhereIF(searchParam.keyword.IsNotEmptyOrNull(), x => x.UserName.Contains(searchParam.keyword))
          .OrderBy(searchParam.OrderBy ?? "-Id")
          .ToPagedListAsync(searchParam.currentPage, searchParam.pageSize);

        //var list = await _memberRechargeService.QueryPageAsync(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => x.UserId == userId
        //    && (searchParam.Status <= 0 || (x.Collection != null && x.Collection.Status == searchParam.Status))
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.UserName != null && x.UserName.Contains(searchParam.Keyword))),
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
        //var resultDto = _mapper.Adapt<IEnumerable<MemberRechargeDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);

        return PageResult< MemberRechargeDto>.SqlSugarPageResult(list);
    }

    /// <summary>
    /// 账户充值
    /// 示例：/account/member/recharge
    /// </summary>
    [HttpPost("/account/member/recharge")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountAdd([FromBody] MemberRechargeEditDto modelDto)
    {
        //获取登录用户ID
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }
        modelDto.userId = userId;
        //保存数据
        var model = await this.AddAsync(modelDto);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<PaymentCollectionDto>();
        return Ok(result);
    }

    /// <summary>
    /// 批量删除记录
    /// 示例：/account/member/recharge?ids=1,2,3
    /// </summary>
    [HttpDelete("/account/member/recharge")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountDeleteByIds([FromQuery] string Ids)
    {
        //检查参数是否为空
        if (Ids == null)
        {
            throw Oops.Oh("传输参数不可为空");
        }
        //获取登录用户ID
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }
        //将ID列表转换成IEnumerable
        var listIds = Ids.ToIEnumerable<long>();
        if (listIds == null)
        {
            throw Oops.Oh("传输参数不符合规范");
        }
        //执行批量删除操作
        await _memberRechargeService.DeleteAsync(x => listIds.Contains(x.Id) && x.UserId == userId);
        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/account/member/recharge/1
    /// </summary>
    [HttpDelete("/account/member/recharge/{id}")]
    [Authorize]
    [NonUnify]
    public async Task<IActionResult> AccountDelete([FromRoute] long id)
    {
        //获取登录用户ID
        var userId = await  _userService.GetUserIdAsync();
        if (userId.IsNullOrEmpty())
        {
            throw Oops.Oh("用户尚未登录");
        }
        //检查记录是否存在
        if (!await _memberRechargeService.AnyAsync(x => x.Id == id && x.UserId == userId))
        {
            throw Oops.Oh($"数据不存在或已删除");
        }
        var result = await _memberRechargeService.DeleteAsync(x => x.Id == id && x.UserId == userId);

        return NoContent();
    }
    #endregion


    #region Private Method
    /// <summary>
    /// 新增一条记录
    /// </summary>
    private async Task<PaymentCollection> AddAsync(MemberRechargeEditDto modelDto)
    {
                                                               //检查会员是否存在
        var memberModel = await _memberRechargeService.Context.Queryable<Members>()
            .Includes(x => x.User)
            .SingleAsync(x => x.UserId == modelDto.userId);
        if (memberModel == null)
        {
            throw Oops.Oh("会员账户不存在或已删除");
        }
        //检查支付方式是否存在
        var paymentModel = await _memberRechargeService.Context.Queryable<SitePayment>().Includes(x => x.Payment).SingleAsync(x => x.Id == modelDto.paymentId);
        if (paymentModel == null || paymentModel.Payment == null)
        {
            throw Oops.Oh("支付方式不存在或已删除");
        }

        //新增一条充值记录
        List<MemberRecharge> rechargeList = new()
        {
            new()
            {
                UserId = memberModel.UserId,
                UserName = memberModel.User?.RealName,
                Amount = modelDto.amount
            }
        };
        //新增一条收款单
        PaymentCollection model = new()
        {
            UserId = modelDto.userId ?? "",
            TradeNo = $"RN{SnowflakeIdHelper.GetGuidToNumber()}",
            TradeType = 1,
            PaymentId = paymentModel.Id,
            PaymentType = paymentModel.Payment.Type == 0 ? (byte)1 : (byte)0,
            PaymentTitle = paymentModel.Title,
            PaymentAmount = modelDto.amount,
            AddTime = DateTime.Now,
            Recharges = rechargeList
        };
        //保存到数据库
        await _memberRechargeService.Context.Insertable<PaymentCollection>(model).ExecuteReturnEntityAsync();

        rechargeList.ForEach(x => x.CollectionId = model.Id);

        await _memberRechargeService.InsertAsync(rechargeList);
        return model;
    } 
    #endregion
}
