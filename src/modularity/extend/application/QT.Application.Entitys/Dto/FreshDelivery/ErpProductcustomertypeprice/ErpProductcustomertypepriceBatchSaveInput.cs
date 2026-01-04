using QT.Application.Entitys.Dto.FreshDelivery.ErpProductprice;
using System.ComponentModel.DataAnnotations;

namespace QT.Application.Entitys.Dto.FreshDelivery.ErpProductcustomertypeprice;

public class ErpProductcustomertypepriceBatchSaveInput
{
    /// <summary>
    /// 删除的集合
    /// </summary>
    public string[] delList { get; set; }

    /// <summary>
    /// 当前的集合
    /// </summary>
    [Required]
    public List<ErpProductcustomertypepriceUpInput> list { get; set; }
}

public class ErpProductCustomerpriceBatchSaveInput
{
    /// <summary>
    /// 删除的集合
    /// </summary>
    public string[] delList { get; set; }

    /// <summary>
    /// 当前的集合
    /// </summary>
    [Required]
    public List<ErpProductpriceCrInput> list { get; set; }
    
}
