using QT.CMS.Entitys.Dto.Member;
using QT.CMS.Entitys.Dto.Order;

namespace QT.CMS;


/// <summary>
/// 统计报表
/// </summary>
[Route("api/cms/admin/report")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 获取会员注册统计
    /// 示例：/admin/report/member/register/count
    /// </summary>
    [HttpGet("member/register/count")]
    [Authorize]
    [AuthorizeFilter("Report", ActionType.View, "MemberRegister")]
    public async Task<IActionResult> GetMemberRegister([FromQuery] ReportParameter searchParam)
    {
        if (searchParam.StartTime == null)
        {
            searchParam.StartTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1);
        }
        if (searchParam.EndTime == null)
        {
            searchParam.EndTime = DateTime.Now;
        }
        var result = await _reportService.GetMemberRegisterCountAsync(searchParam.Top.GetValueOrDefault(),
            x => (DateTime.Compare(x.RegTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
            && (DateTime.Compare(x.RegTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        );
        return Ok(result);
    }

    /// <summary>
    /// 获取会员注册分页
    /// 示例：/admin/report/member/register/list
    /// </summary>
    [HttpGet("member/register/list")]
    [Authorize]
    [AuthorizeFilter("Report", ActionType.View, "MemberRegister")]
    public async Task<dynamic> GetRegisterList([FromQuery] ReportParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        if (searchParam.StartTime == null)
        {
            searchParam.StartTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1);
        }
        if (searchParam.EndTime == null)
        {
            searchParam.EndTime = DateTime.Now;
        }

        //获取数据列表
        var list = await _reportService.GetMemberRegisterPageAsync(
            pageParam.PageSize,
            pageParam.PageIndex,
            x => (DateTime.Compare(x.RegTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
            && (DateTime.Compare(x.RegTime, searchParam.EndTime.GetValueOrDefault()) <= 0),
            searchParam.OrderBy ?? "Id,RegTime");

        return PageResult<MembersDto>.SqlSugarPageResult(list); ;

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
        //return Ok(resultDto);
    }


    /// <summary>
    /// 获取会员消费统计
    /// 示例：/admin/report/member/amount/count
    /// </summary>
    [HttpGet("member/amount/count")]
    [Authorize]
    [AuthorizeFilter("Report", ActionType.View, "MemberAmount")]
    public async Task<IActionResult> GetMemberAmount([FromQuery] ReportParameter searchParam)
    {
        if (searchParam.StartTime == null)
        {
            searchParam.StartTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1);
        }
        if (searchParam.EndTime == null)
        {
            searchParam.EndTime = DateTime.Now;
        }
        var result = await _reportService.GetMemberAmountCountAsync(searchParam.Top.GetValueOrDefault(),
            x => (DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
            && (DateTime.Compare(x.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        );
        return Ok(result);
    }

    /// <summary>
    /// 获取会员消费分页
    /// 示例：/admin/report/member/amount/list
    /// </summary>
    [HttpGet("member/amount/list")]
    [Authorize]
    [AuthorizeFilter("Report", ActionType.View, "MemberAmount")]
    public async Task<IActionResult> GetAmountList([FromQuery] ReportParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        if (searchParam.StartTime == null)
        {
            searchParam.StartTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1);
        }
        if (searchParam.EndTime == null)
        {
            searchParam.EndTime = DateTime.Now;
        }

        //获取数据列表
        var list = await _reportService.GetMemberAmountPageAsync(
            pageParam.PageSize,
            pageParam.PageIndex,
            x => (DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
            && (DateTime.Compare(x.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0),
            searchParam.OrderBy ?? "Id,AddTime");

        return PageResult<MemberAmountLogDto>.SqlSugarPageResult(list);

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
        //var resultDto = _mapper.Map<IEnumerable<MemberAmountLogDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 获取商品销售统计
    /// 示例：/admin/report/order/goods/count
    /// </summary>
    [HttpGet("order/goods/count")]
    [Authorize]
    [AuthorizeFilter("Report", ActionType.View, "GoodsCount")]
    public async Task<IActionResult> GetGoodsCount([FromQuery] ReportParameter searchParam)
    {
        if (searchParam.StartTime == null)
        {
            searchParam.StartTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1);
        }
        if (searchParam.EndTime == null)
        {
            searchParam.EndTime = DateTime.Now;
        }
        var result = await _reportService.GetGoodsSaleCountAsync(searchParam.Top.GetValueOrDefault(),
            x => (DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
            && (DateTime.Compare(x.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        );
        return Ok(result);
    }

    /// <summary>
    /// 获取商品销售分页
    /// 示例：/admin/report/order/goods/list
    /// </summary>
    [HttpGet("order/goods/list")]
    [Authorize]
    [AuthorizeFilter("Report", ActionType.View, "GoodsCount")]
    public async Task<IActionResult> GetGoodsList([FromQuery] ReportParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        if (searchParam.StartTime == null)
        {
            searchParam.StartTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1);
        }
        if (searchParam.EndTime == null)
        {
            searchParam.EndTime = DateTime.Now;
        }

        //获取数据列表
        var list = await _reportService.GetGoodsSalePageAsync(
            pageParam.PageSize,
            pageParam.PageIndex,
            x => x.Order != null
            && (DateTime.Compare(x.Order.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
            && (DateTime.Compare(x.Order.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0),
            searchParam.OrderBy ?? "Id");

        return PageResult<OrderGoodsDto>.SqlSugarPageResult(list);

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
        //var resultDto = _mapper.Map<IEnumerable<OrderGoodsDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 获取订单金额统计
    /// 示例：/admin/report/order/amount/count
    /// </summary>
    [HttpGet("order/amount/count")]
    [Authorize]
    [AuthorizeFilter("Report", ActionType.View, "OrderAmount")]
    public async Task<IActionResult> GetOrderCount([FromQuery] ReportParameter searchParam)
    {
        if (searchParam.StartTime == null)
        {
            searchParam.StartTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1);
        }
        if (searchParam.EndTime == null)
        {
            searchParam.EndTime = DateTime.Now;
        }
        var result = await _reportService.GetOrderAmountCountAsync(searchParam.Top.GetValueOrDefault(),
            x => (DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
            && (DateTime.Compare(x.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0)
        );
        return Ok(result);
    }

    /// <summary>
    /// 获取订单金额分页
    /// 示例：/admin/report/order/amount/list
    /// </summary>
    [HttpGet("order/amount/list")]
    [Authorize]
    [AuthorizeFilter("Report", ActionType.View, "OrderAmount")]
    public async Task<IActionResult> GetOrderList([FromQuery] ReportParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        if (searchParam.StartTime == null)
        {
            searchParam.StartTime = DateTime.Now.AddDays(-DateTime.Now.Day + 1);
        }
        if (searchParam.EndTime == null)
        {
            searchParam.EndTime = DateTime.Now;
        }

        //获取数据列表
        var list = await _reportService.GetOrderAmountPageAsync(
            pageParam.PageSize,
            pageParam.PageIndex,
            x => (DateTime.Compare(x.AddTime, searchParam.StartTime.GetValueOrDefault()) >= 0)
            && (DateTime.Compare(x.AddTime, searchParam.EndTime.GetValueOrDefault()) <= 0),
            searchParam.OrderBy ?? "AddTime,Id");

        return PageResult<OrdersDto>.SqlSugarPageResult(list);

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
        //var resultDto = _mapper.Map<IEnumerable<OrdersDto>>(list).ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }
    #endregion
}