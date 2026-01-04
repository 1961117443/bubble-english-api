using QT.Common.Const;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.FreshDelivery;

/// <summary>
/// 订单关联表
/// </summary>
[SugarTable("erp_order_relation")]
[Tenant(ClaimConst.TENANTID)]
public class ErpOrderRelationEntity
{
    /// <summary>
    /// 主键.
    /// </summary>
    [SugarColumn(ColumnName = "F_Id", IsPrimaryKey = true)]
    public string Id { get; set; }

    /// <summary>
    /// 订单主键ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Oid")]
    public string Oid { get; set; }

    /// <summary>
    /// 子单主键ID.
    /// </summary>
    [SugarColumn(ColumnName = "F_Cid")]
    public string Cid { get; set; }

    /// <summary>
    /// 类型（主表或者从表）.
    /// </summary>
    [SugarColumn(ColumnName = "F_Type")]
    public string Type { get; set; }
}
