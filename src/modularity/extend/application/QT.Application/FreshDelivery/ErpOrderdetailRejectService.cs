using QT.Application.Entitys.Dto.FreshDelivery.ErpOrderdetailReject;
using QT.Application.Entitys.Enum.FreshDelivery;
using QT.Application.Entitys.FreshDelivery;
using QT.Common.Core.Security;
using QT.Common.Enum;
using QT.DependencyInjection;
using QT.DynamicApiController;
using QT.Systems.Interfaces.System;

namespace QT.Application.FreshDelivery;

/// <summary>
/// 业务实现：订单退货.
/// </summary>
[ApiDescriptionSettings("生鲜配送",Tag = "订单退货", Name = "ErpOrderdetailReject", Order = 200)]
[Route("api/apply/FreshDelivery/[controller]")]
public class ErpOrderdetailRejectService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<ErpOrderdetailRejectEntity> _repository;

    /// <summary>
    /// 用户管理.
    /// </summary>
    private readonly IUserManager _userManager;
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="ErpOrderdetailRejectService"/>类型的新实例.
    /// </summary>
    public ErpOrderdetailRejectService(
        ISqlSugarRepository<ErpOrderdetailRejectEntity> erpOrderdetailRejectRepository,
        ISqlSugarClient context,
        IUserManager userManager)
    {
        _repository = erpOrderdetailRejectRepository;
        _userManager = userManager;
        _db = context.AsTenant();
    }

    /// <summary>
    /// 获取订单退货.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<dynamic> GetInfo(string id)
    {
        var data = await _repository.Context.Queryable<ErpOrderdetailRejectEntity>()
            .InnerJoin<ErpOrderdetailEntity>((a, b) => a.OrderDetailId == b.Id)
             .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Mid == c.Id)
          .InnerJoin<ErpProductEntity>((a, b, c, d) => c.Pid == d.Id)
          .InnerJoin<ErpOrderEntity>((a, b, c, d, e) => b.Fid == e.Id)
          .InnerJoin<ErpCustomerEntity>((a, b, c, d, e, f) => e.Cid == f.Id)
          .Where((a) => a.Id == id)
            .Select((a, b, c, d, e, f) => new ErpOrderdetailRejectListOutput
            {
                id = a.Id,
                orderDetailId = a.OrderDetailId,
                num = a.Num,
                amount = a.Amount,
                remark = a.Remark,
                customerName = f.Name,
                orderNo = e.No,
                midName = c.Name,
                midUnit = c.Unit,
                productName = d.Name,
                salePrice = b.SalePrice,
                posttime = e.Posttime,
                num1 = b.Num1,
            })
            .FirstAsync();
        return data;
    }

    /// <summary>
    /// 获取订单退货列表.
    /// </summary>
    /// <param name="input">请求参数.</param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] ErpOrderdetailRejectListQueryInput input)
    {

        var data = await _repository.Context.Queryable<ErpOrderdetailRejectEntity>()
            .InnerJoin<ErpOrderdetailEntity>((a, b) => a.OrderDetailId == b.Id)
             .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Mid == c.Id)
          .InnerJoin<ErpProductEntity>((a, b, c, d) => c.Pid == d.Id)
          .InnerJoin<ErpOrderEntity>((a, b, c, d, e) => b.Fid == e.Id)
          .InnerJoin<ErpCustomerEntity>((a, b, c, d, e, f) => e.Cid == f.Id)
            .WhereIF(input.orderNo.IsNotEmptyOrNull(), (a, b, c, d, e, f) => e.No == input.orderNo)
            .WhereIF(input.cidName.IsNotEmptyOrNull(), (a, b, c, d, e, f) => f.Name.Contains(input.cidName))
            .WhereIF(input.posttime.HasValue, (a, b, c, d, e, f) => e.Posttime >= input.posttime && e.Posttime < input.posttime.Value.AddDays(1))
            .OrderByIF(string.IsNullOrEmpty(input.sidx), a => a.Id)
            .Select((a, b, c, d, e, f) => new ErpOrderdetailRejectListOutput
            {
                id = a.Id,
                orderDetailId = a.OrderDetailId,
                num = a.Num,
                amount = a.Amount,
                remark = a.Remark,
                customerName = f.Name,
                orderNo = e.No,
                midName = c.Name,
                midUnit = c.Unit,
                productName = d.Name,
                salePrice = b.SalePrice,
                posttime = e.Posttime,
                num1=b.Num1,
                auditTime = a.AuditTime
            }).OrderByIF(!string.IsNullOrEmpty(input.sidx), it => input.sidx + " " + input.sort).ToPagedListAsync(input.currentPage, input.pageSize);
        return PageResult<ErpOrderdetailRejectListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 新建订单退货.
    /// </summary>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task Create([FromBody] ErpOrderdetailRejectCrInput input)
    {
        var entity = input.Adapt<ErpOrderdetailRejectEntity>();
        entity.Id = SnowflakeIdHelper.NextId();
        var isOk = await _repository.Context.Insertable(entity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1000);
    }

    /// <summary>
    /// 更新订单退货.
    /// </summary>
    /// <param name="id">主键值.</param>
    /// <param name="input">参数.</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ErpOrderdetailRejectUpInput input)
    {
        var entity = input.Adapt<ErpOrderdetailRejectEntity>();
        var isOk = await _repository.Context.Updateable(entity).Where(it=> it.Id == entity.Id && it.AuditTime == null).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1001);
    }

    /// <summary>
    /// 删除订单退货.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var isOk = await _repository.Context.Deleteable<ErpOrderdetailRejectEntity>().Where(it => it.Id.Equals(id) && it.AuditTime == null).ExecuteCommandAsync();
        if (!(isOk > 0)) throw Oops.Oh(ErrorCode.COM1002);
    }


    /// <summary>
    /// 查询订单
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/queryOrder")]
    public async Task<dynamic> QueryOrderList([FromQuery] ErpOrderdetailRejectQueryOrderListQueryInput input)
    {
        var data = await _repository.Context.Queryable<ErpOrderdetailEntity>()
          .InnerJoin<ErpProductmodelEntity>((w, x) => w.Mid == x.Id)
          .InnerJoin<ErpProductEntity>((w, x, b) => x.Pid == b.Id)
          .InnerJoin<ErpOrderEntity>((w, x, b, a) => w.Fid == a.Id)
          .InnerJoin<ErpCustomerEntity>((w, x, b, a, c) => a.Cid == c.Id)
          .Where((w, x, b, a, c) => a.State >  OrderStateEnum.Picked  && a.State !=  OrderStateEnum.Split)
          .Where((w,x,b,a,c)=> SqlFunc.Subqueryable<ErpOrderdetailRejectEntity>().Where(xxx=>xxx.OrderDetailId ==w.Id).NotAny() )
          .WhereIF(input.customerName.IsNotEmptyOrNull(), (w, x, b, a, c) => c.Name.Contains(input.customerName))
          .WhereIF(input.no.IsNotEmptyOrNull(), (w, x, b, a, c) => a.No.Contains(input.no))
          .WhereIF(input.keyword.IsNotEmptyOrNull(), (w, x, b, a, c) => a.No.Contains(input.keyword) || c.Name.Contains(input.keyword) || b.Name.Contains(input.keyword))
          .Select((w, x, b, a, c) => new ErpOrderdetailRejectQueryOrderListOutput()
          {
              midName = x.Name,
              productName = b.Name,
              //rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
              customerName = c.Name,
              orderNo = a.No,
              num = w.Num,
              num1 = w.Num1,
              midUnit = x.Unit
          }, true)
          .ToPagedListAsync(input.currentPage, input.pageSize);

        return PageResult<ErpOrderdetailRejectQueryOrderListOutput>.SqlSugarPageResult(data);
    }

    /// <summary>
    /// 查询订单
    /// </summary>
    /// <returns></returns>
    [HttpGet("Actions/queryOrder/{id}")]
    public async Task<dynamic> QueryOrderListById(string id)
    {
        var data = await _repository.Context.Queryable<ErpOrderdetailEntity>()
          .InnerJoin<ErpProductmodelEntity>((w, x) => w.Mid == x.Id)
          .InnerJoin<ErpProductEntity>((w, x, b) => x.Pid == b.Id)
          .InnerJoin<ErpOrderEntity>((w, x, b, a) => w.Fid == a.Id)
          .InnerJoin<ErpCustomerEntity>((w, x, b, a, c) => a.Cid == c.Id)
          .Where((w, x, b, a, c) => w.Id == id)
          .Select((w, x, b, a, c) => new ErpOrderdetailRejectQueryOrderListOutput()
          {
              midName = x.Name,
              productName = b.Name,
              //rootProducttype = SqlFunc.Subqueryable<ViewErpProducttypeEx>().Where(ddd => ddd.Id == b.Tid).Select(ddd => ddd.RootName)
              customerName = c.Name,
              orderNo = a.No,
              num = w.Num,
              num1 = w.Num,
              midUnit = x.Unit
          }, true)
          .FirstAsync();

        return data;
    }

    /// <summary>
    /// 审核订单退货.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Audit/{id}")]
    public async Task Audit(string id, [FromServices] IBillRullService billRullService)
    {
        // 1、生成退货入库记录，（销售出库的成本价作为入库单价）
        // 2、更新订单的退回数量，计算订单金额
        // 3、更新退回的审核时间
        await _db.TranExecute(async () =>
        {
            var entity = await _repository.AsQueryable().InSingleAsync(id) ?? throw Oops.Oh(ErrorCode.COM1005);
            if (entity.AuditTime.HasValue)
            {
                throw Oops.Oh("单据已审核！");
            }
            var orderDetail = await _repository.Context.Queryable<ErpOrderdetailEntity>().InSingleAsync(entity.OrderDetailId) ?? throw Oops.Oh(ErrorCode.COM1005);
            var order = await _repository.Context.Queryable<ErpOrderEntity>().InSingleAsync(orderDetail.Fid) ?? throw Oops.Oh(ErrorCode.COM1005);
            #region 1、生成退货入库记录，（销售出库的成本价作为入库单价）
            //获取出库记录的出库成本
            var costAmount = await _repository.Context.Queryable<ErpOutrecordEntity>().Where(x => x.OrderId == entity.OrderDetailId).SumAsync(x=>x.CostAmount);

            ErpInorderEntity erpInorderEntity = new ErpInorderEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                Oid = order.Oid,
                InType = "6",
                No = await billRullService.GetBillNumber("QTErpInOrder"),
                InTime = order.Posttime?.ToString("yyyy-MM-DD") ?? "",
                Amount = costAmount
            };
            ErpInrecordEntity erpInrecordEntity = new ErpInrecordEntity
            {
                Id = SnowflakeIdHelper.NextId(),
                Amount = costAmount,
                Price = orderDetail.Num1 > 0 ? Math.Round(costAmount / orderDetail.Num1, 2) : 0,
                Gid = orderDetail.Mid,
                InNum = entity.Num,
                Num = entity.Num,
                Remark = orderDetail.Remark,
                InId = erpInorderEntity.Id
            };
            await _repository.Context.Insertable<ErpInorderEntity>(erpInorderEntity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
            await _repository.Context.Insertable<ErpInrecordEntity>(erpInrecordEntity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
            #endregion

            #region 2、更新订单的退回数量
            // 2、更新订单的退回数量
            _repository.Context.Tracking(orderDetail);
            orderDetail.RejectNum = entity.Num;
            // 订单金额 (配送数量-退回数量)*订单单价
            var newAmount = (orderDetail.Num1 - orderDetail.RejectNum.Value) * orderDetail.SalePrice;
            var diffAmount = newAmount - orderDetail.Amount; // 当前订单明细的差异金额，用来更新主表的订单金额
            orderDetail.Amount = newAmount;

            _repository.Context.Tracking(order);
            order.Amount += diffAmount;
            await  _repository.Context.Updateable<ErpOrderEntity>(order).ExecuteCommandAsync();
            await _repository.Context.Updateable<ErpOrderdetailEntity>(orderDetail).ExecuteCommandAsync();
            #endregion

            #region 3、更新退回的审核时间
            _repository.Context.Tracking(entity);
            entity.AuditTime = DateTime.Now;
            entity.AuditUserId = _userManager.UserId;

            var isOk = await _repository.Context.Updateable<ErpOrderdetailRejectEntity>(entity).Where(it => it.Id == entity.Id && it.AuditTime == null).ExecuteCommandAsync();
            if (!(isOk > 0)) throw Oops.Oh("审核失败！"); 
            #endregion
        });
    }
}