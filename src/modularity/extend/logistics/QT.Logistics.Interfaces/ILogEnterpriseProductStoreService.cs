using QT.Logistics.Entitys.Dto.LogEnterpriseProductStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Logistics.Interfaces;

public interface ILogEnterpriseProductStoreService
{
    /// <summary>
    /// 获取库存明细
    /// </summary>
    /// <param name="logEnterpriseProductStoreDetailQueryInput"></param>
    /// <returns></returns>
    Task<List<LogEnterpriseProductStoreDetailListOutput>> GetLogEnterpriseProductStoreDetailListAsync(LogEnterpriseProductStoreDetailQueryInput logEnterpriseProductStoreDetailQueryInput);

    /// <summary>
    /// 获取库存汇总
    /// </summary>
    /// <param name="logEnterpriseProductStoreDetailQueryInput"></param>
    /// <returns></returns>
    Task<List<LogEnterpriseProductStoreSumOutput>> GetLogEnterpriseProductStoreSumAsync(LogEnterpriseProductStoreDetailQueryInput logEnterpriseProductStoreDetailQueryInput);
}
