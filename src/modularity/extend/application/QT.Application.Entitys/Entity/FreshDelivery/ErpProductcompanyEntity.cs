using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.FreshDelivery;

/// <summary>
/// 商品公司关联表.
/// </summary>
[SugarTable("erp_productcompany")]
[Tenant(ClaimConst.TENANTID)]
public class ErpProductcompanyEntity : CLEntityBase
{
    ///// <summary>
    ///// 主键.
    ///// </summary>
    //[SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    //public string Id { get; set; }


    /// <summary>
    /// 商品ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Pid")]
    public string Pid { get; set; }


    /// <summary>
    /// 公司ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Oid")]
    public string Oid { get; set; }
}
