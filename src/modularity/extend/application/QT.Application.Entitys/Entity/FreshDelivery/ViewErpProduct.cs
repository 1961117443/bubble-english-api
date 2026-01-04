using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.FreshDelivery;

[SqlSugar.SugarTable("view_erpproduct")]
public class ViewErpProduct
{
    [SqlSugar.SugarColumn(ColumnName ="F_Id")]
    public string Id { get; set; }
    [SqlSugar.SugarColumn(ColumnName = "F_Name")]
    public string Name { get; set; }
    [SqlSugar.SugarColumn(ColumnName = "TypeName")]
    public string TypeName { get; set; }  
}


[SqlSugar.SugarTable("vw_erp_producttype_ex")]
public class ViewErpProducttypeEx
{
    [SqlSugar.SugarColumn(ColumnName = "F_Id")]
    public string Id { get; set; }
    [SqlSugar.SugarColumn(ColumnName = "F_Name")]
    public string Name { get; set; }


    [SqlSugar.SugarColumn(ColumnName = "F_Fid")]
    public string Fid { get; set; }

    [SqlSugar.SugarColumn(ColumnName = "Rid")]
    public string RootId { get; set; }

    [SqlSugar.SugarColumn(ColumnName = "R_Name")]
    public string RootName { get; set; }

    /// <summary>
    /// 一级分类排序
    /// </summary>
    [SqlSugar.SugarColumn(ColumnName = "R_Order")]
    public string ROrder { get; set; }

    /// <summary>
    /// 商品分类排序
    /// </summary>
    [SqlSugar.SugarColumn(ColumnName = "F_Order")]
    public string Order { get; set; }
}
