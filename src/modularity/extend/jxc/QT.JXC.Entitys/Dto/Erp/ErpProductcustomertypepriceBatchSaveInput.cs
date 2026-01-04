using QT.JXC.Entitys.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.JXC.Entitys.Dto.Erp;

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
