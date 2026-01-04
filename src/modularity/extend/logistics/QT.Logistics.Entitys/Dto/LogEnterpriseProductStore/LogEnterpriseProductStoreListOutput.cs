using QT.DependencyInjection;

namespace QT.Logistics.Entitys.Dto.LogEnterpriseProductStore;

/// <summary>
/// 商家商品信息输入参数.
/// </summary>
public class LogEnterpriseProductStoreListOutput
{
    #region 商品信息

    /// <summary>
    /// 分类ID.
    /// </summary>
    public string tid { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string productName { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    public string firstChar { get; set; }

    /// <summary>
    /// 产地.
    /// </summary>
    public string producer { get; set; }

    ///// <summary>
    ///// 介绍.
    ///// </summary>
    //public string remark { get; set; }

    /// <summary>
    /// 存储条件.
    /// </summary>
    public string storage { get; set; }

    /// <summary>
    /// 保质期.
    /// </summary>
    public string retention { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    public int state { get; set; }


    /// <summary>
    /// 分类名称.
    /// </summary>
    public string tidName { get; set; }


    /// <summary>
    /// 商家.
    /// </summary>
    public string eIdName { get; set; }
    #endregion



    #region 规格信息
    /// <summary>
    /// 规格主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 规格.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 单位.
    /// </summary>
    public string unit { get; set; }

    ///// <summary>
    ///// 成本价.
    ///// </summary>
    //public decimal costPrice { get; set; }

    ///// <summary>
    ///// 销售价.
    ///// </summary>
    //public decimal salePrice { get; set; }

    /// <summary>
    /// 条形码.
    /// </summary>
    public string barCode { get; set; }

    ///// <summary>
    ///// 起售数.
    ///// </summary>
    //public decimal minNum { get; set; }

    ///// <summary>
    ///// 限购数.
    ///// </summary>
    //public decimal maxNum { get; set; } 

    /// <summary>
    /// 库存数.
    /// </summary>
    public decimal storeNum { get; set; }
    #endregion
}

/// <summary>
/// 库存明细记录
/// </summary>
public class LogEnterpriseProductStoreDetailListOutput
{
    /// <summary>
    /// 规格主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 单据日期
    /// </summary>
    public DateTime? billDate { get; set; }


    /// <summary>
    /// 单据类型
    /// </summary>
    public string billType { get; set; }

    /// <summary>
    /// 仓库id.
    /// </summary>
    public string storeRoomId { get; set; }

    /// <summary>
    /// 入库数量
    /// </summary>
    public decimal? inNum { get; set; }

    /// <summary>
    /// 出库数量
    /// </summary>
    public decimal? outNum { get; set;}

    /// <summary>
    /// 类型，（1:入库，-1：出库）
    /// </summary>
    public int category { get; set; }
}

/// <summary>
/// 库存明细记录
/// </summary>
public class LogEnterpriseProductStoreSumOutput
{
    /// <summary>
    /// 规格主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    public decimal? storeNum { get; set; }
}