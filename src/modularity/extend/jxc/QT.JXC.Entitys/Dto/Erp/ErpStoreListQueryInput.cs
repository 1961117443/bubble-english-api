using QT.Common.Filter;

namespace QT.JXC.Entitys.Dto.Erp;

public class ErpStoreListQueryInput
{
    /// <summary>
    /// 过滤公司id.
    /// </summary>
    public string oid { get; set; }



    /// <summary>
    /// 是否只显示其他规格
    /// </summary>
    public bool otherSpec { get; set; }


    /// <summary>
    /// 客户类型
    /// </summary>
    public string cidType { get; set; }
}

public class ErpStorePageListQueryInput : PageInputBase
{
    /// <summary>
    /// 过滤公司id.
    /// </summary>
    public string oid { get; set; }

    /// <summary>
    /// 分类id
    /// </summary>
    public string tid { get; set; }


    /// <summary>
    /// 规格id
    /// </summary>
    public string gid { get; set; }

    /// <summary>
    /// 是否带出关联产品
    /// </summary>
    public bool whetherRelation { get; set; }


    /// <summary>
    /// 入库时间 范围查询
    /// </summary>
    public string intimeRange { get; set; }

    /// <summary>
    /// 全部公司
    /// </summary>
    public bool? allOid { get; set; }

    /// <summary>
    /// 截止时间
    /// </summary>
    public DateTime? cutDate { get; set; }

    /// <summary>
    /// 一级分类
    /// </summary>
    public string rootTypeId { get; set; }

}


public class ErpInrecordTransferInput
{
    public string id { get; set; }

    public List<string> value { get; set; }
}

public class ErpInrecordTransferInputV2
{
    public string id { get; set; }

    public List<ErpInrecordTransferInputV2Item> items { get; set; }

    /// <summary>
    /// 当前的入库数量（有可能是前台未保存的数据）
    /// </summary>
    public decimal? inNum { get; set; }
}

public class ErpInrecordTransferInputV2Item
{
    /// <summary>
    /// 特殊入库id
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 本次关联数量
    /// </summary>
    public decimal num { get; set; }
}