using QT.Common.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Entity.ERP;

/// <summary>
/// 采购入库关联特殊入库记录
/// </summary>
[SugarTable("erp_inrecord_ts")]
[Tenant(ClaimConst.TENANTID)]
public class ErpInrecordTsEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 特殊入库明细ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_TsId")]
    public string TsId { get; set; }

    /// <summary>
    /// 采购入库明细ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_InId")]
    public string InId { get; set; }


    /// <summary>
    /// 关联特殊数量.
    /// </summary>
    [SugarColumn(ColumnName = "Num")]
    public decimal Num { get; set; }
}
