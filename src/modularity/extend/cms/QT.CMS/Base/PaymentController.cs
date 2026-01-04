using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using QT.CMS.Entitys;
using QT.CMS.Entitys.Dto.Base;
using QT.CMS.Entitys.Dto.Order;
using QT.CMS.Entitys.Dto.Parameter;
using QT.Common.Extension;
using QT.FriendlyException;
using QT.JsonSerialization;
using SqlSugar;

namespace QT.CMS;

/// <summary>
/// 支付方式接口
/// </summary>
[Route("api/cms/admin/payment")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly ISqlSugarRepository<Payments> _paymentService;

    /// <summary>
    /// 依赖注入
    /// </summary>
    public PaymentController(ISqlSugarRepository<Payments> paymentService)
    {
        _paymentService = paymentService;
    }

    #region 管理员调用接口==========================
    /// <summary>
    /// 获取列表
    /// 示例：/admin/payment/view/10
    /// </summary>
    [HttpGet("view/{top}")]
    [Authorize]
    //[AuthorizeFilter("Payment", ActionType.View)]
    public async Task<IActionResult> GetList([FromRoute] int top, [FromQuery] BaseParameter searchParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.Replace("-", "").IsPropertyExists<PaymentsDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<PaymentsDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //获取数据库列表
        var resultFrom = await _paymentService.Context.Queryable<Payments>()
            .Where(x => x.Status == 0)
            .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
            .OrderBy(searchParam.OrderBy ?? "SortId,Id")
            .AutoTake(top)
            .ToListAsync();
        //var resultFrom = await _paymentService.QueryListAsync<Payments>(top,
        //    x => x.Status == 0
        //    && (!searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword))),
        //    searchParam.OrderBy ?? "SortId,Id");
        //使用AutoMapper转换成ViewModel，根据字段进行塑形
        var resultDto = resultFrom.Adapt<IEnumerable<PaymentsDto>>();//.ShapeData(searchParam.Fields);
        //返回成功200
        return Ok(resultDto);
    }

    /// <summary>
    /// 获取分页列表
    /// 示例：/admin/payment?pageSize=10&pageIndex=1
    /// </summary>
    [HttpGet]
    [Authorize]
    //[AuthorizeFilter("Payment", ActionType.View)]
    public async Task<IActionResult> GetList([FromQuery] BaseParameter searchParam, [FromQuery] PageParamater pageParam)
    {
        //检测参数是否合法
        if (searchParam.OrderBy != null
            && !searchParam.OrderBy.TrimStart('-').IsPropertyExists<PaymentsDto>())
        {
            throw Oops.Oh("请输入正确的排序参数");
        }
        if (!searchParam.Fields.IsPropertyExists<PaymentsDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }

        //获取数据列表
        var list = await _paymentService.Context.Queryable<Payments>()
           .WhereIF(searchParam.Keyword.IsNotEmptyOrNull(), x => x.Title.Contains(searchParam.Keyword))
           .OrderBy(searchParam.OrderBy ?? "Id")
           .ToPagedListAsync(pageParam.PageIndex, pageParam.PageSize);

        return Ok(PageResult<PaymentsDto>.SqlSugarPageResult(list));
        //var list = await _paymentService.QueryPageAsync<Payments>(
        //    pageParam.PageSize,
        //    pageParam.PageIndex,
        //    x => !searchParam.Keyword.IsNotNullOrEmpty() || (x.Title != null && x.Title.Contains(searchParam.Keyword)),
        //    searchParam.OrderBy ?? "Id");

        //x-pagination
        //var paginationMetadata = new
        //{
        //    totalCount = list.pagination.Total,
        //    pageSize = list.pagination.PageSize,
        //    pageIndex = list.pagination.PageIndex,
        //    totalPages = 0
        //};
        //Response.Headers.Add("x-pagination", JSON.Serialize(paginationMetadata));

        ////映射成DTO
        //var resultDto = list.Adapt<IEnumerable<PaymentsDto>>().ShapeData(searchParam.Fields);
        //return Ok(resultDto);
    }

    /// <summary>
    /// 根据ID获取数据
    /// 示例：/admin/payment/1
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    //[AuthorizeFilter("Payment", ActionType.View)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] BaseParameter param)
    {
        //检测参数是否合法
        if (!param.Fields.IsPropertyExists<PaymentsDto>())
        {
            throw Oops.Oh("请输入正确的属性参数");
        }
        //查询数据库获取实体
        var model = await _paymentService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        //使用AutoMapper转换成ViewModel
        //根据字段进行塑形
        var result = model.Adapt<PaymentsDto>().ShapeData(param.Fields);
        return Ok(result);
    }

    /// <summary>
    /// 添加一条记录
    /// 示例：/admin/payment/
    /// </summary>
    [HttpPost]
    [Authorize]
    //[AuthorizeFilter("Payment", ActionType.Add)]
    public async Task<IActionResult> Add([FromBody] PaymentsEditDto modelDto)
    {
        //映射成实体
        var model = modelDto.Adapt<Payments>();
        //写入数据库
        await _paymentService.InsertAsync(model);
        //映射成DTO再返回，否则出错
        var result = model.Adapt<PaymentsDto>();
        return Ok(result);
    }

    /// <summary>
    /// 修改一条记录
    /// 示例：/admin/payment/1
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    //[AuthorizeFilter("Payment", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PaymentsEditDto modelDto)
    {
        //查找记录
        var model = await _paymentService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }
        _paymentService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelDto.Adapt( model);
        var result = await _paymentService.UpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 局部更新一条记录
    /// 示例：/admin/payment/1
    /// Body：[{"op":"replace","path":"/title","value":"new title"}]
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    //[AuthorizeFilter("Payment", ActionType.Edit)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] JsonPatchDocument<PaymentsEditDto> patchDocument)
    {
        var model = await _paymentService.SingleAsync(x => x.Id == id);
        if (model == null)
        {
            throw Oops.Oh($"数据{id}不存在或已删除");
        }

        var modelToPatch = model.Adapt<PaymentsEditDto>();
        patchDocument.ApplyTo(modelToPatch, ModelState);
        //验证数据是否合法
        if (!TryValidateModel(modelToPatch))
        {
            return ValidationProblem(ModelState);
        }
        _paymentService.Context.Tracking(model);
        //更新操作AutoMapper替我们完成，只需要调用保存即可
        modelToPatch.Adapt(model);
        await _paymentService.UpdateAsync(model);
        return NoContent();
    }

    /// <summary>
    /// 删除一条记录
    /// 示例：/admin/payment/1
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    //[AuthorizeFilter("Payment", ActionType.Delete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!await _paymentService.AnyAsync(x => x.Id == id))
        {
            throw Oops.Oh($"数据[{id}]不存在或已删除");
        }
        var result = await _paymentService.DeleteAsync(id);

        return NoContent();
    }

    /// <summary>
    /// 批量删除记录(级联数据)
    /// 示例：/admin/payment?ids=1,2,3
    /// </summary>
    [HttpDelete]
    [Authorize]
    //[AuthorizeFilter("Payment", ActionType.Delete)]
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
        await _paymentService.DeleteAsync(listIds);

        return NoContent();
    }
    #endregion
}