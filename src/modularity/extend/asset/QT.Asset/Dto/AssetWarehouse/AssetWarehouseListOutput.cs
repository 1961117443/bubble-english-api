using QT.Common.Security;
using QT.DependencyInjection;
using System.Security.AccessControl;

namespace QT.Asset.Dto.AssetWarehouse;

/// <summary>
/// 仓库信息输入参数.
/// </summary>
public class AssetWarehouseListOutput
{
    /// <summary>
    /// 自然主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 地址.
    /// </summary>
    public string address { get; set; }

    /// <summary>
    /// 简介.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    public string adminId { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    public string adminTel { get; set; }

    /// <summary>
    /// 面积.
    /// </summary>
    public decimal area { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    public int status { get; set; }

    /// <summary>
    /// 上级仓库.
    /// </summary>
    public string pidName { get; set; }

    /// <summary>
    /// 类型.
    /// </summary>
    public int category { get; set; }



    /// <summary>
    /// 条码
    /// </summary>
    public string barcode { get; set; }

}

/// <summary>
/// 树形下拉模型
/// </summary>
[SuppressSniffer]
public class AssetWarehouseTreeListOutput : TreeModel
{
    /// <summary>
    /// 类型
    /// </summary>
    public int category { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string fullName { get; set; }

    /// <summary>
    /// 条码
    /// </summary>
    public string barcode { get; set; }
}