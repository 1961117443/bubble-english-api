using QT.Common.Security;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpStoreroom;

/// <summary>
/// 仓库信息输入参数.
/// </summary>
public class ErpStoreroomListOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 父级仓库ID.
    /// </summary>
    public string fid { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    public string firstChar { get; set; }

    /// <summary>
    /// 仓库地址.
    /// </summary>
    public string address { get; set; }

    /// <summary>
    /// 联系电话.
    /// </summary>
    public string phone { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    public string admin { get; set; }

    /// <summary>
    /// 负责人电话.
    /// </summary>
    public string admintel { get; set; }

    /// <summary>
    /// 父级仓库
    /// </summary>
    public string fidName { get; set; }

    /// <summary>
    /// 所属公司.
    /// </summary>
    public string[] oid { get; set; }

}


public class ErpStoreroomListTreeOutput : TreeModel
{
    /// <summary>
    /// 父级仓库ID.
    /// </summary>
    public string fid { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    public string firstChar { get; set; }

    /// <summary>
    /// 仓库地址.
    /// </summary>
    public string address { get; set; }

    /// <summary>
    /// 联系电话.
    /// </summary>
    public string phone { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    public string admin { get; set; }

    /// <summary>
    /// 负责人电话.
    /// </summary>
    public string admintel { get; set; }

    /// <summary>
    /// 父级仓库
    /// </summary>
    public string fidName { get; set; }
}