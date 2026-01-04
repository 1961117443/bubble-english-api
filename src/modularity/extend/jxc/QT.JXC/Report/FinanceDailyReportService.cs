using NPOI.OpenXmlFormats.Dml;
using QT.Common.Extension;
using QT.JXC.Entitys.Dto.Report.FinanceDailyReport;
using QT.Systems.Entitys.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Report;

/// <summary>
/// 业务实现：报表统计-财务日报表.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "报表统计-财务日报表", Name = "FinanceDailyReport", Order = 200)]
[Route("api/Erp/Report/FinanceDailyReport")]
public class FinanceDailyReportService : IDynamicApiController
{
    private readonly ISqlSugarClient _db;

    public FinanceDailyReportService(ISqlSugarClient db)
    {
        _db = db;
        _db.QueryFilter.Clear<ICompanyEntity>();
    }

    [HttpGet("")]
    public async Task<dynamic> GetFinanceDailyReportAsync([FromQuery] FinanceDailyReportQueryInput input)
    {
        List<DateTime> dateRange = input.dateRange?.Split(',').ToObject<List<DateTime>>();
        DateTime? startDate = dateRange?.First();
        DateTime? endDate = dateRange?.Last();
        if (!startDate.HasValue || !endDate.HasValue)
        {
            throw Oops.Oh("请输入正确的日期！");
        }
        var report = new DailyReportDto
        {
             //Date = startDate.Value,
             StartDate = startDate.Value,
             EndDate = endDate.Value
        };
        var oids = input.oidRange?.Split(",").ToArray();
        report.Companys = await _db.Queryable<OrganizeEntity>()
            .WhereIF(oids.IsAny(), x=> oids.Contains(x.Id))
            .Select(x => new CompanyDto
            {
                 Oid = x.Id,
                 CompanyName = x.FullName
            }).ToListAsync();

        report.YesterdayCash = await this.GetYesterdayCash(report);
        report.Inbound = await this.GetTodayInbound(report);
        report.Sales = await this.GetTodaySales(report);
        report.Income = await this.GetTodayIncome(report);
        report.Payments = await this.GetTodayPayments(report);


        /*
       var cashCompany = report.YesterdayCash.Select(x => x.Oid);
        var businessCompany = report.Inbound.Select(x => x.Oid)
             .Concat(report.Sales.Select(x => x.Oid))
             .Concat(report.Income.Select(x => x.Oid))
             .Concat(report.Payments.Select(x => x.Oid))
             .Except(cashCompany)
             .ToList();

        if (businessCompany.IsAny())
        {
            var list = await _db.Queryable<OrganizeEntity>().Where(x => businessCompany.Contains(x.Id)).Select(x => new YesterdayCashDto
            {
                AccountNumber = "",
                BankName = "",
                AccountName = "",
                CompanyName = x.FullName,
                YesterdayBalance = 0,
                Oid = x.Id
            }).ToListAsync();

            report.YesterdayCash.AddRange(list);
        }

        report.Companys = report.YesterdayCash.GroupBy(x => new { Oid = x.Oid, CompanyName = x.CompanyName }).Select(x=> new CompanyDto { Oid = x.Key.Oid, CompanyName = x.Key.CompanyName }).ToList();
       */

        //var list = CreateFinancialDailyReportItem(report);

        return report;
    }

    /// <summary>
    /// 获取上日库存现金集合
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="today"></param>
    /// <returns></returns>
    private async Task<List<YesterdayCashDto>> GetYesterdayCash(DailyReportDto reportDto)
    {
        DateTime today = reportDto.StartDate;
        // 期初
        var list1 = await _db.Queryable<ErpBankAccountEntity>()
            .InnerJoin<OrganizeEntity>((x, a) => x.Oid == a.Id)
            .Select((x, a) => new YesterdayCashDto
            {
                AccountNumber = x.AccountNumber,
                BankName = x.BankName,
                AccountName = x.AccountName,
                CompanyName = a.FullName,
                YesterdayBalance = x.Balance,
                Oid = x.Oid
            })
            .ToListAsync();

        // 上日收入

        var list2 = await _db.Queryable<CwReceiptEntity>()
           .InnerJoin<ErpCustomerEntity>((a, b) => b.Id == a.Cid)
           .InnerJoin<ErpBankAccountEntity>((a, b, c) => a.BankAccountId == c.Id)
           .InnerJoin<OrganizeEntity>((a, b, c, d) => b.Oid == d.Id)
           .Where((a, b, c, d) => SqlFunc.LessThan(a.ReceiptDate, today))
           .GroupBy((a, b, c, d) => new { b.Oid, CompanyName = d.FullName, BankName = c.BankName, AccountName = c.AccountName, AccountNumber = c.AccountNumber })
           .Select((a, b, c, d) => new YesterdayCashDto
           {
               AccountNumber = c.AccountNumber,
               CompanyName = d.FullName,
               BankName = c.BankName,
               AccountName = c.AccountName,
               YesterdayBalance = SqlFunc.AggregateSum(a.Amount),
               Oid = b.Oid
           })
           .ToListAsync();


        // 上日支出

        var list3 = await _db.Queryable<CwPaymentEntity>()
          .InnerJoin<ErpSupplierEntity>((a, b) => b.Id == a.Sid)
          .InnerJoin<ErpBankAccountEntity>((a, b, c) => a.BankAccountId == c.Id)
          .InnerJoin<OrganizeEntity>((a, b, c, d) => b.Oid == d.Id)
          .Where((a, b, c, d) => SqlFunc.LessThan(a.PaymentDate, today))
          .GroupBy((a, b, c, d) => new { b.Oid, CompanyName = d.FullName, BankName = c.BankName, AccountName = c.AccountName, AccountNumber = c.AccountNumber })
          .Select((a, b, c, d) => new YesterdayCashDto
          {
              AccountNumber = c.AccountNumber,
              CompanyName = d.FullName,
              BankName = c.BankName,
              AccountName = c.AccountName,
              YesterdayBalance = SqlFunc.AggregateSum(a.Amount * -1),
              Oid = b.Oid
          })
          .ToListAsync();


        var list = list1.Concat(list2).Concat(list3);

        var result = list.GroupBy(x => new { x.Oid, x.AccountNumber, x.CompanyName, x.AccountName, x.BankName })
            .Select(x => new YesterdayCashDto
            {
                Oid = x.Key.Oid,
                AccountNumber = x.Key.AccountNumber,
                CompanyName = x.Key.CompanyName,
                BankName = x.Key.BankName,
                AccountName = x.Key.AccountName,
                YesterdayBalance = x.Sum(z => z.YesterdayBalance)
            })
            .ToList();
        return result;
    }

    /// <summary>
    /// 获取当日入库集合
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    private async Task<List<InboundDto>> GetTodayInbound(DailyReportDto reportDto)
    {

        var list = await _db.Queryable<ErpBuyorderEntity>()
            .InnerJoin<ErpBuyorderdetailEntity>((a, b) => a.Id == b.Fid)
            .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Gid == c.Id)
            .InnerJoin<ErpProductEntity>((a, b, c, d) => c.Pid == d.Id)
            .InnerJoin<ErpSupplierEntity>((a, b, c, d, e) => e.Id == b.Supplier)
            .WhereIF(reportDto.Companys.IsAny(), (a,b,c,d)=> reportDto.Companys.Any(z=>z.Oid == a.Oid))
            .Where((a, b, c, d) => SqlFunc.Between(a.TaskBuyTime, reportDto.StartDate.ToString("yyyy-MM-dd 00:00:00"), reportDto.EndDate.ToString("yyyy-MM-dd 23:59:59")) && b.BuyState == "2")
            .GroupBy((a, b, c, d, e) => new { a.Oid, SupplierName = e.Name, ProductName = d.Name, UnitPrice = b.Price })
            .Select((a, b, c, d, e) => new InboundDto
            {
                Amount = SqlFunc.AggregateSum(b.Amount),
                ProductName = d.Name,
                Quantity = SqlFunc.AggregateSum(b.TsNum + (b.InNum ?? 0)),
                SupplierName = e.Name,
                UnitPrice = b.Price,
                Oid = a.Oid,
            })
            .ToListAsync();

        // 合并同一个供应商的
        foreach (var g in list.GroupBy(x => new {x.Oid,x.SupplierName}))
        {
            var totalAmount = g.Sum(x => x.Amount);
            foreach (var item in g)
            {
                item.TotalAmount = totalAmount;
            }
        }

        return list;
    }

    /// <summary>
    /// 获取当日销货集合
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    private async Task<List<SalesDto>> GetTodaySales(DailyReportDto reportDto)
    {
        var list = await _db.Queryable<ErpOrderEntity>()
            .InnerJoin<ErpOrderdetailEntity>((a, b) => a.Id == b.Fid)
            .InnerJoin<ErpProductmodelEntity>((a, b, c) => b.Mid == c.Id)
            .InnerJoin<ErpProductEntity>((a, b, c, d) => c.Pid == d.Id)
            .InnerJoin<ErpCustomerEntity>((a, b, c, d, e) => e.Id == a.Cid)
            .Where((a, b, c, d) => SqlFunc.Between(a.Posttime, reportDto.StartDate.ToString("yyyy-MM-dd 00:00:00"), reportDto.EndDate.ToString("yyyy-MM-dd 23:59:59")))
            .WhereIF(reportDto.Companys.IsAny() ,(a, b, c, d) => reportDto.Companys.Any(zz=>zz.Oid == a.Oid))
            .GroupBy((a, b, c, d, e) => new { a.Oid, CustomerName = e.Name, ProductName = d.Name, UnitPrice = b.SalePrice })
            .Select((a, b, c, d, e) => new SalesDto
            {
                Amount = SqlFunc.AggregateSum(b.Amount),
                ProductName = d.Name,
                Quantity = SqlFunc.AggregateSum(b.Num),
                CustomerName = e.Name,
                UnitPrice = b.SalePrice,
                Oid = a.Oid
            })
            .ToListAsync();

        // 合并同一个客户的
        foreach (var g in list.GroupBy(x =>new {x.Oid, x.CustomerName }))
        {
            var totalAmount = g.Sum(x => x.Amount);
            foreach (var item in g)
            {
                item.TotalAmount = totalAmount;
            }
        }

        return list;
    }

    /// <summary>
    /// 获取当日收款信息
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    private async Task<List<IncomeDto>> GetTodayIncome(DailyReportDto reportDto)
    {
        var list = await _db.Queryable<CwReceiptEntity>()
           .InnerJoin<ErpCustomerEntity>((a, b) => b.Id == a.Cid)
           .LeftJoin<ErpBankAccountEntity>((a, b, c) => a.BankAccountId == c.Id)
           .InnerJoin<OrganizeEntity>((a, b, c, d) => b.Oid == d.Id)
           .Where((a, b, c, d) => SqlFunc.Between(a.ReceiptDate, reportDto.StartDate.ToString("yyyy-MM-dd 00:00:00"), reportDto.EndDate.ToString("yyyy-MM-dd 23:59:59")))
           .WhereIF(reportDto.Companys.IsAny(), (a, b, c, d) => reportDto.Companys.Any(zz => zz.Oid == b.Oid))
           .GroupBy((a, b, c, d) => new { b.Oid, CompanyName = d.FullName, CustomerName = b.Name, AccountNumber = c.AccountNumber })
           .Select((a, b, c, d) => new IncomeDto
           {
               AccountNumber = c.AccountNumber,
               CompanyName = d.FullName,
               Amount = SqlFunc.AggregateSum(a.Amount),
               CustomerName = b.Name,
               Oid = b.Oid
           })
           .ToListAsync();

        // 合并同一个收款公司的
        foreach (var g in list.GroupBy(x => new { x.Oid, x.CompanyName }))
        {
            var totalAmount = g.Sum(x => x.Amount);
            foreach (var item in g)
            {
                item.TotalAmount = totalAmount;
            }
        }


        return list;
    }

    /// <summary>
    /// 获取当日付款信息
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    private async Task<List<PaymentDto>> GetTodayPayments(DailyReportDto reportDto)
    {
        var list = await _db.Queryable<CwPaymentEntity>()
           .InnerJoin<ErpSupplierEntity>((a, b) => b.Id == a.Sid)
           .LeftJoin<ErpBankAccountEntity>((a, b, c) => a.BankAccountId == c.Id)
           .InnerJoin<OrganizeEntity>((a, b, c, d) => b.Oid == d.Id)
           .Where((a, b, c, d) => SqlFunc.Between(a.PaymentDate, reportDto.StartDate.ToString("yyyy-MM-dd 00:00:00"), reportDto.EndDate.ToString("yyyy-MM-dd 23:59:59")))
           .WhereIF(reportDto.Companys.IsAny(), (a, b, c, d) => reportDto.Companys.Any(zz => zz.Oid == b.Oid))
           .GroupBy((a, b, c, d) => new { b.Oid, PayCompany = d.FullName, Receiver = b.Name, AccountNumber = c.AccountNumber, PayItem = a.Remark })
           .Select((a, b, c, d) => new PaymentDto
           {
               AccountNumber = c.AccountNumber,
               PayCompany = d.FullName,
               PayAmount = SqlFunc.AggregateSum(a.Amount),
               Receiver = b.Name,
               PayItem = a.Remark,
               Oid = b.Oid
           })
           .ToListAsync();

        // 合并同一个付款公司的
        foreach (var g in list.GroupBy(x => new { x.Oid, x.PayCompany }))
        {
            var totalAmount = g.Sum(x => x.PayAmount);
            foreach (var item in g)
            {
                item.TotalAmount = totalAmount;
            }
        }

        return list;
    }

    private List<FinancialDailyReportItemDto> CreateFinancialDailyReportItem(DailyReportDto dailyReportDto)
    {
        var cash = dailyReportDto.YesterdayCash;
        var inList = dailyReportDto.Inbound;
        var salesList = dailyReportDto.Sales;
        var incomeList = dailyReportDto.Income;
        var expenseList = dailyReportDto.Payments;
        var max = new[]
        {
            inList.Count,
            salesList.Count,
            incomeList.Count,
            expenseList.Count,
            cash.Count
        }.Max();

        var items = new List<FinancialDailyReportItemDto>();

        for (int i = 0; i < max; i++)
        {
            items.Add(new FinancialDailyReportItemDto
            {
                // 上日库存现金（只有一条）
                companyName = i < cash.Count ? cash[i].CompanyName : null ,
                bankName = i < cash.Count ? cash[i].BankName : null,
                bankAccount = i < cash.Count ? cash[i].AccountNumber : null,
                lastCashAmount = i < cash.Count ? cash[i].YesterdayBalance : 0,

                // 当日入库（可能存在或为空）
                supplierName = i < inList.Count ? inList[i].SupplierName : null,
                inProductName = i < inList.Count ? inList[i].ProductName : null,
                inPrice = i < inList.Count ? inList[i].UnitPrice : 0,
                inQuantity = i < inList.Count ? inList[i].Quantity : 0,
                inAmount = i < inList.Count ? inList[i].Amount : 0,
                inTotalAmount = i < inList.Count ? inList[i].TotalAmount : 0,

                // 当日销售
                customerName = i < salesList.Count ? salesList[i].CustomerName : null,
                salesProductName = i < salesList.Count ? salesList[i].ProductName : null,
                salesPrice = i < salesList.Count ? salesList[i].UnitPrice : 0,
                salesQuantity = i < salesList.Count ? salesList[i].Quantity : 0,
                salesAmount = i < salesList.Count ? salesList[i].Amount : 0,
                salesTotalAmount = i < salesList.Count ? salesList[i].TotalAmount : 0,

                // 收
                payCustomerName = i < incomeList.Count ? incomeList[i].CustomerName : null,
                payAmount = i < incomeList.Count ? incomeList[i].Amount : 0,
                payCompany = i < incomeList.Count ? incomeList[i].CompanyName : null,
                payAccount = i < incomeList.Count ? incomeList[i].AccountNumber : null,
                incomeTotalAmount = i < incomeList.Count ? incomeList[i].TotalAmount : 0,

                // 支
                expenseItem = i < expenseList.Count ? expenseList[i].PayItem : null,
                expenseCompany = i < expenseList.Count ? expenseList[i].PayCompany : null,
                expenseAccount = i < expenseList.Count ? expenseList[i].AccountNumber : null,
                expenseReceiver = i < expenseList.Count ? expenseList[i].Receiver : null,
                expenseAmount = i < expenseList.Count ? expenseList[i].PayAmount : 0,
                expenseTotalAmount = i < expenseList.Count ? expenseList[i].TotalAmount : 0,
            });
        }

        return items;
    }
}
