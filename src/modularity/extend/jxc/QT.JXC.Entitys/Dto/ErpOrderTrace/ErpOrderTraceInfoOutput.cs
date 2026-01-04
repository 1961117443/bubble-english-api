using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Dto.ErpOrderTrace;

public class ErpOrderTraceInfoOutput: ErpOrderTraceInfoBase
{
    public TraceInfo traceInfo { get; set; }
}


public class ErpOrderTraceInfoBase
{
    public OrderInfo orderInfo {  get; set; }

    public ProductInfo productInfo { get; set; }

    public OriginInfo originInfo { get; set; }
}



public class TraceInfo
{
    public string id { get; set; }
    public string code { get; set; }
    public int num { get; set; }
    public string firstQueryTime { get; set; }
}


public class OrderInfo
{
    /// <summary>
    /// 订单明细id
    /// </summary>
    public string id { get; set; }
    public string fid { get; set; }
    public string cid { get; set; }
    public string cidName { get; set; }
    public string oid { get; set; }
    public string oidName { get; set; }
    public string fjTime { get; set; }

    /// <summary>
    /// 配送时间提取订单中约定配送时间的数据
    /// </summary>
    public string psTime { get; set; }
    public string gid { get; set; }


    public string no { get; set; }

    /// <summary>
    /// 客户类型
    /// </summary>
    public string cidType { get; set; }


    /// <summary>
    /// 生产日期
    /// </summary>
    public string productDate { get; set; }

    /// <summary>
    /// 保质期
    /// </summary>
    public string retention { get; set; }
}

public class ProductInfo
{
    public string id { get; set; }
    public string productName { get; set; }
    public string productType { get; set; }
    public string typeId { get; set; }
    public string unit { get; set; }
    public string unitCn { get; set; }
    public string gid { get; set; }
    public string gidName { get; set; }
    public string brand { get; set; }
    public string origin { get; set; }

    public List<string> imageList { get; set; }
}
public class OriginInfo
{
    //public List<string> detectionList { get; set; }

    /// <summary>
    /// 检测报告
    /// </summary>
    public List<string> qualityReport { get; set; }

    /// <summary>
    /// 自检报告
    /// </summary>
    public List<string> selfReport { get; set; }

    public List<OriginInrecordInfo> inRecordList { get; set; }
}

public class OriginInrecordInfo
{
    public string id { get; set; }
    public string pid { get; set; }

    public string num { get; set; }

    public string supplierId { get; set; }

    public string supplierName { get; set; }
    public string rkNo { get; set; }

    public string cgNo { get; set; }

    /// <summary>
    /// 采购单id
    /// </summary>
    public string cgId { get; set; }

    /// <summary>
    /// 采购明细id
    /// </summary>
    public string cgdId { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    public string productDate { get; set; }

    /// <summary>
    /// 保质期
    /// </summary>
    public string retention { get; set; }

    /// <summary>
    /// 质检报告
    /// </summary>
    public string qualityReportProof { get; set; }

    /// <summary>
    /// 本次使用
    /// </summary>
    public string useNum { get; set; }
}