namespace QT.JXC.Entitys.Dto.ErpBankAccount;

public class ErpBankAccountCrInput
{
    /// <summary>
    /// 银行名称
    /// </summary>
    public string bankName { get; set; }


    /// <summary>
    /// 账户类型（如：对公账户，对私账户等）
    /// </summary>
    public string accountType { get; set; }

    /// <summary>
    /// 银行账号
    /// </summary>
    public string accountNumber { get; set; }

    /// <summary>
    /// 账户名称
    /// </summary>
    public string accountName { get; set; }



    /// <summary>
    /// 账户余额
    /// </summary>
    public decimal balance { get; set; }


    /// <summary>
    /// 备注
    /// </summary>
    public string remark { get; set; }
}
