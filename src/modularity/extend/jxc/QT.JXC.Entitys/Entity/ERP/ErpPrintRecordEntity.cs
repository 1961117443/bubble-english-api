//using QT.Common.Const;
//using QT.Common.Contracts;
//using SqlSugar;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace QT.Extend.Entitys;

///// <summary>
///// 订单打印记录表实体.
///// </summary>
//[SugarTable("erp_printrecord")]
//[Tenant(ClaimConst.TENANTID)]
//public class ErpPrintRecordEntity: CEntityBase
//{
//    /// <summary>
//    /// 菜单ID.
//    /// </summary>
//    [SugarColumn(ColumnName = "F_MenuId")]
//    public string MenuId { get; set; }

//    /// <summary>
//    /// 订单ID.
//    /// </summary>
//    [SugarColumn(ColumnName = "F_Fid")]
//    public string Fid { get; set; }
//}
