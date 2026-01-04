using Newtonsoft.Json;

namespace QT.JXC.Entitys.Dto.Erp.gz_cycs;

public class CycsSupplierAddInfo
{
    public int cycsId { get; set; } // 所属门店id

    public string supplyName { get; set; } // 供应商名称

    public string gType { get; set; } // 供应商类型

    public string papersId { get; set; } // 当供应商类型为0市场主体时填写社会统一信用代码，当类型为1其他时填写身份证号

    public string divisionCode { get; set; } // 所属地区编码

    public string address { get; set; } // 详细地址

    public string contactPhone { get; set; } // 联系电话
    
    public string url { get; set; } // 身份证件正(反)照片(当类型为1其他时填写) 照片base64字符串,多张图片用,分割
}



public class CycsSupplierAddInfoInput : CycsSupplierAddInfo
{
    public string Sid { get; set; }

    public int SupplierId { get;set; }
}


public class ErpCycsSupplierAddInfoInput
{
    public int cycsId { get; set; }
    public string sId { get; set; }

    public int supplierId { get; set; }

    public string divisionCode { get; set; }

    public string gType { get; set; }

    public string papersId { get; set; }

    public string address { get; set; }

    public string contactPhone { get; set; }

    public string url { get; set; }

    public string supplyName { get; set; }

}


public class ErpCycsGoodsAddInfoInput
{
    public string pId { get; set; }

    public int goodsId { get; set; }

    public int cycsId { get; set; }

    public string code { get; set; }

    public string localName { get; set; }

    public string barCode { get; set; }

    public string manufacturer { get; set; }
}