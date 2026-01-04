using QT.Common.Const;
using QT.Common.Contracts;
using QT.JXC.Entitys.Dto.Erp.gz_cycs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Entity.ERP.gz_cycs;

[SugarTable("erp_cycs_goods")]
[Tenant(ClaimConst.TENANTID)]
public class ErpCycsGoodsEntity
{

    [SugarColumn(ColumnName = "PId", ColumnDescription = "erp商品id" ,IsPrimaryKey =true)]
    public string PId { get; set; }

    [SugarColumn(ColumnName = "GoodsId", ColumnDescription = "商品id")]
    public int GoodsId { get; set; }

    [SugarColumn(ColumnName = "CycsId", ColumnDescription = "所在门店id")]
    public int CycsId { get; set; }

    [SugarColumn(ColumnName = "Code", ColumnDescription = "商品类别编码")]
    public string Code { get; set; }

    [SugarColumn(ColumnName = "LocalName", ColumnDescription = "商品名称")]
    public string LocalName { get; set; }

    [SugarColumn(ColumnName = "BarCode", ColumnDescription = "商品条形码")]
    public string BarCode { get; set; }

    [SugarColumn(ColumnName = "Manufacturer", ColumnDescription = "生产厂商名称")]
    public string Manufacturer { get; set; }
}

[SugarTable("erp_cycs_supplier")]
[Tenant(ClaimConst.TENANTID)]
public class ErpCycsSupplierEntity 
{
    [SugarColumn(ColumnName = "SId", ColumnDescription = "erp供应商id",IsPrimaryKey =true)]
    public string SId { get; set; }

    [SugarColumn(ColumnName = "SupplierId", ColumnDescription = "供应商id")]
    public int SupplierId { get; set; }

    [SugarColumn(ColumnName = "DivisionCode", ColumnDescription = "所属地区编码")]
    public string DivisionCode { get; set; }

    [SugarColumn(ColumnName = "GType", ColumnDescription = "供应商类型")]
    public string GType { get; set; }

    [SugarColumn(ColumnName = "PapersId", ColumnDescription = "当供应商类型为0市场主体时填写社会统一信用代码，当类型为1其他时填写身份证号")]
    public string PapersId { get; set; }

    [SugarColumn(ColumnName = "Address", ColumnDescription = "详细地址")]
    public string Address { get; set; }

    [SugarColumn(ColumnName = "ContactPhone", ColumnDescription = "联系电话")]
    public string ContactPhone { get; set; }

    [SugarColumn(ColumnName = "Url", ColumnDescription = "身份证件正(反)照片(当类型为1其他时填写)  照片base64字符串,多张图片用,分割")]
    public string Url { get; set; }

}


[SugarTable("erp_cycs_organize")]
[Tenant(ClaimConst.TENANTID)]
public class ErpCycsOrganizeEntity : IDeleteTime
{
    [SugarColumn(ColumnName = "Id", ColumnDescription = "主键", IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(ColumnName = "OId", ColumnDescription = "erp公司id")]
    public string OId { get; set; }

    [SugarColumn(ColumnName = "CycsId", ColumnDescription = "门店id")]
    public int CycsId { get; set; }

    [SugarColumn(ColumnName = "F_DeleteTime", ColumnDescription = "删除时间")]
    public DateTime? DeleteTime { get; set; }
}

[SugarTable("erp_cycs_in")]
public class ErpCycsInEntity
{
    [SugarColumn(ColumnName = "Id", ColumnDescription = "主键", IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 采购明细
    /// </summary>
    [SugarColumn(ColumnName = "buyId", ColumnDescription = "所属门店id")]
    public string buyId { get; set; }

    [SugarColumn(ColumnName = "cycsId", ColumnDescription = "所属门店id")]
    public int cycsId { get; set; } // 所属门店id

    [SugarColumn(ColumnName = "supplyId", ColumnDescription = "供应商id")]
    public int supplyId { get; set; } // 供应商id


    [SugarColumn(ColumnName = "goodsCode", ColumnDescription = "商品id")]
    public int goodsCode { get; set; } // 商品id


    [SugarColumn(ColumnName = "amount", ColumnDescription = "商品重量(kg)")]
    public decimal amount { get; set; } // 商品重量(kg)


    [SugarColumn(ColumnName = "ticketDate", ColumnDescription = "进货日期")]
    public DateTime? ticketDate { get; set; } // 进货日期（yyyy-MM-dd）


    [SugarColumn(ColumnName = "tickets", ColumnDescription = "票证信息",IsJson =true)]
    public List<Ticket> tickets { get; set; } // 票证信息


    [SugarColumn(ColumnName = "divisionCode", ColumnDescription = "产地编号")]
    public string divisionCode { get; set; } // 产地编号


    [SugarColumn(ColumnName = "manufactureDate", ColumnDescription = "生产日期")]
    public DateTime? manufactureDate { get; set; } // 生产日期（yyyy-MM-dd）


    [SugarColumn(ColumnName = "expiryDate", ColumnDescription = "保质期")]
    public DateTime? expiryDate { get; set; } // 保质期（yyyy-MM-dd）


    [SugarColumn(ColumnName = "batchNumber", ColumnDescription = "生产批次")]
    public string batchNumber { get; set; } // 生产批次

}