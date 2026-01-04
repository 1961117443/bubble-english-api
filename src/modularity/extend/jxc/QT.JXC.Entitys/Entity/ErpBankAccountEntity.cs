using QT.Common;
using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.JXC.Entitys.Entity;

/// <summary>
/// 银行账户表
/// </summary>
[SugarTable("erp_bankaccount")]
[EntityUniqueProperty("AccountNumber")]
public class ErpBankAccountEntity : CUDEntityBase,ICompanyEntity
{
    /// <summary>
    /// 公司ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Oid")]
    public string Oid { get; set; }

    /// <summary>
    /// 银行名称
    /// </summary>
    [SugarColumn(ColumnName = "BankName")]
    public string BankName { get; set; }


    /// <summary>
    /// 账户类型（如：对公账户，对私账户等）
    /// </summary>
    [SugarColumn(ColumnName = "AccountType")]
    public string AccountType { get; set; }

    /// <summary>
    /// 银行账号
    /// </summary>
    [SugarColumn(ColumnName = "AccountNumber")]
    public string AccountNumber { get; set; }

    /// <summary>
    /// 账户名称
    /// </summary>
    [SugarColumn(ColumnName = "AccountName")]
    public string AccountName { get; set; }



    /// <summary>
    /// 账户余额
    /// </summary>
    [SugarColumn(ColumnName = "Balance")]
    public decimal Balance { get; set; }


    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnName = "Remark")]
    public string Remark { get; set; }
}