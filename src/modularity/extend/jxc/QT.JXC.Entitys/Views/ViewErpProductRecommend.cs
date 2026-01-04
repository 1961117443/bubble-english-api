using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Views;

[SugarTable("view_erpProduct_recommend")]
public class ViewErpProductRecommend
{
    /// <summary>
    /// 公司id.
    /// </summary>
    [SugarColumn(ColumnName = "F_Oid")]
    public string Oid { get; set; }

    /// <summary>
    /// 规格id.
    /// </summary>
    [SugarColumn(ColumnName = "F_Mid")]
    public string Mid { get; set; }

    /// <summary>
    /// 建议采购数量
    /// </summary>
    [SugarColumn(ColumnName = "F_Num")]
    public decimal Num { get; set; }

    /// <summary>
    /// 库存数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_StoreNum")]
    public decimal StoreNum { get; set; }

    /// <summary>
    /// 订单数量.
    /// </summary>
    [SugarColumn(ColumnName = "F_OrderNum")]
    public string OrderNum { get; set; }
}
