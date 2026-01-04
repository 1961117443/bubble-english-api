using QT.Common.Core;
using QT.JXC.Entitys.Dto.ErpBankAccount;

namespace QT.JXC;

/// <summary>
/// 业务实现：银行账户表.
/// </summary>
[ApiDescriptionSettings(ModuleConst.ERP, Tag = "银行账户表", Name = "ErpBankAccount", Order = 300)]
[Route("api/erp/BankAccount")]
public class ErpBankAccountService : QTBaseService<ErpBankAccountEntity, ErpBankAccountCrInput, ErpBankAccountUpInput, ErpBankAccountOutput, ErpBankAccountListQueryInput, ErpBankAccountListOutput>, IDynamicApiController
{

    public ErpBankAccountService(ISqlSugarRepository<ErpBankAccountEntity> repository, ISqlSugarClient context, IUserManager userManager) : base(repository, context, userManager)
    {
    }

    protected override async Task<SqlSugarPagedList<ErpBankAccountListOutput>> GetPageList([FromQuery] ErpBankAccountListQueryInput input)
    { 
        return await _repository.Context.Queryable<ErpBankAccountEntity>()
            .WhereIF(!string.IsNullOrEmpty(input.accountName), it => it.AccountName.Contains(input.accountName))
            .WhereIF(!string.IsNullOrEmpty(input.accountNumber), it => it.AccountNumber.Contains(input.accountNumber))
            .WhereIF(!string.IsNullOrEmpty(input.accountType), it => it.AccountType == input.accountType)
            .WhereIF(!string.IsNullOrEmpty(input.bankName), it => it.BankName.Contains(input.bankName))
            .Select<ErpBankAccountListOutput>(it => new ErpBankAccountListOutput
            {
            }, true)
            .ToPagedListAsync(input.currentPage, input.pageSize);
    }
}