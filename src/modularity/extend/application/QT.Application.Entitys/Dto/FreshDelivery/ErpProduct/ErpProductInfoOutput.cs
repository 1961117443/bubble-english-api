using QT.Application.Entitys.Dto.FreshDelivery.ErpProductcompany;
using QT.Application.Entitys.Dto.FreshDelivery.ErpProductmodel;
using QT.Application.Entitys.Dto.FreshDelivery.ErpProductpic;
using QT.Application.Entitys.Dto.FreshDelivery.ErpProductvideo;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProduct;

/// <summary>
/// 商品信息输出参数.
/// </summary>
public class ErpProductInfoOutput
{
    /// <summary>
    /// 主键.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 分类ID.
    /// </summary>
    public string tid { get; set; }

    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 别名.
    /// </summary>
    public string nickname { get; set; }

    /// <summary>
    /// 拼音首字母.
    /// </summary>
    public string firstChar { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    public string no { get; set; }

    /// <summary>
    /// 计量单位.
    /// </summary>
    public string unit { get; set; }

    /// <summary>
    /// 产地.
    /// </summary>
    public string producer { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public int? sort { get; set; }

    /// <summary>
    /// 存储条件.
    /// </summary>
    public string storage { get; set; }

    /// <summary>
    /// 保质期.
    /// </summary>
    public string retention { get; set; }

    /// <summary>
    /// 供货商.
    /// </summary>
    public string supplier { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    public int state { get; set; }

    /// <summary>
    /// 销售类型.
    /// </summary>
    public string saletype { get; set; }

    /// <summary>
    /// 库存.
    /// </summary>
    public decimal num { get; set; }

    /// <summary>
    /// 商品规格.
    /// </summary>
    public List<ErpProductmodelInfoOutput> erpProductmodelList { get; set; }

    /// <summary>
    /// 商品图片.
    /// </summary>
    public List<ErpProductpicInfoOutput> erpProductpicList { get; set; }

    /// <summary>
    /// 商品视频.
    /// </summary>
    public List<ErpProductvideoInfoOutput> erpProductvideoList { get; set; }

    /// <summary>
    /// 商品关联公司.
    /// </summary>
    public List<ErpProductcompanyInfoOutput> erpProductcompanyList { get; set; }

}