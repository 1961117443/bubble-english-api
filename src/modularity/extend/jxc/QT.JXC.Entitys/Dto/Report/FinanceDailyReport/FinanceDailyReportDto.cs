using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Dto.Report.FinanceDailyReport;

//public class FinanceDailyReportDto
//{

//    public DateTime ReportDate { get; set; }

//    public List<FinanceDailyReportCompanyDto> Companies { get; set; }
//}


//public class FinanceDailyReportCompanyDto
//{
//    public int CompanyId { get; set; }
//    public string CompanyName { get; set; }

//    public List<FinanceDailyReportBankAccountDto> BankAccounts { get; set; }
//}


//public class FinanceDailyReportBankAccountDto
//{
//    public int AccountId { get; set; }
//    public string BankName { get; set; }
//    public string AccountName { get; set; }
//    public string AccountNumber { get; set; }

//    public decimal Balance { get; set; }

//    public List<FinanceDailyReportTransactionDto> Transactions { get; set; }
//}

//public class FinanceDailyReportTransactionDto
//{
//    public DateTime Time { get; set; }

//    public decimal Income { get; set; }
//    public decimal Expense { get; set; }

//    public string Remark { get; set; }
//}

public class FinanceDailyReportQueryInput
{
    /// <summary>
    /// 日期时间 范围查询
    /// </summary>
    public string dateRange { get; set; }

    /// <summary>
    /// 公司集合
    /// </summary>

    public string oidRange { get; set; }


}

/// <summary>
/// 整体日报表 DTO
/// </summary>
public class DailyReportDto
{
    //public DateTime Date { get; set; }
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }


    public List<YesterdayCashDto> YesterdayCash { get; set; }
    public List<InboundDto> Inbound { get; set; }
    public List<SalesDto> Sales { get; set; }
    public List<IncomeDto> Income { get; set; }
    public List<PaymentDto> Payments { get; set; }
    public List<CompanyDto> Companys { get; set; }

    public decimal TotalInbound { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalPayment { get; set; }

    public decimal TodayBalance { get; set; }
}

/// <summary>
/// 上日库存现金
/// </summary>
public class YesterdayCashDto
{
    public string Oid { get; set; }

    public string CompanyName { get; set; }
    public string BankName { get; set; }
    public string AccountName { get; set; }
    public string AccountNumber { get; set; }
    public decimal YesterdayBalance { get; set; }
}

/// <summary>
/// 当日入库 DTO
/// </summary>
public class InboundDto
{
    public string Oid { get; set; }

    ///// <summary>
    ///// 公司
    ///// </summary>
    //public string CompanyName { get; set; }

    public string SupplierName { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// 当日销售 DTO
/// </summary>
public class SalesDto
{
    public string Oid { get; set; }
    ///// <summary>
    ///// 公司
    ///// </summary>
    //public string CompanyName { get; set; }

    public string CustomerName { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// 当日收款 DTO
/// </summary>
public class IncomeDto
{
    public string Oid { get; set; }

    /// <summary>
    /// 收款客户名称
    /// </summary>
    public string CustomerName { get; set; }

    /// <summary>
    /// 收款金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 收款公司
    /// </summary>
    public string CompanyName { get; set; }

    /// <summary>
    /// 收款账号
    /// </summary>
    public string AccountNumber { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// 当日付款 DTO
/// </summary>
public class PaymentDto
{
    public string Oid { get; set; }

    public string PayItem { get; set; }
    public string PayCompany { get; set; }
    public string AccountNumber { get; set; }
    public string Receiver { get; set; }
    public decimal PayAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// 所属公司 DTO
/// </summary>
public class CompanyDto
{
    public string Oid { get; set; }

    /// <summary>
    /// 收款公司
    /// </summary>
    public string CompanyName { get; set; }
}


public class FinancialDailyReportItemDto
{
    // 上日库存现金
    public string companyName { get; set; }
    public string bankName { get; set; }
    public string bankAccount { get; set; }
    public decimal lastCashAmount { get; set; }

    // 当日入库
    public string supplierName { get; set; }
    public string inProductName { get; set; }
    public decimal inPrice { get; set; }
    public decimal inQuantity { get; set; }
    public decimal inAmount { get; set; }
    public decimal inTotalAmount { get; set; }

    // 当日销售
    public string customerName { get; set; }
    public string salesProductName { get; set; }
    public decimal salesPrice { get; set; }
    public decimal salesQuantity { get; set; }
    public decimal salesAmount { get; set; }
    public decimal salesTotalAmount { get; set; }

    // 收
    public string payCustomerName { get; set; }
    public decimal payAmount { get; set; }
    public string payCompany { get; set; }
    public string payAccount { get; set; }
    public decimal incomeTotalAmount { get; set; }

    // 支
    public string expenseItem { get; set; }
    public string expenseCompany { get; set; }
    public string expenseAccount { get; set; }
    public string expenseReceiver { get; set; }
    public decimal expenseAmount { get; set; }
    public decimal expenseTotalAmount { get; set; }
}