using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.SDMS.Entitys.Dto.SysConfig;

public class SdmsConfigOutput
{
    /// <summary>
    /// 分润基数比例：决定从订单金额中抽取多少比例作为分润基数。
    /// </summary>
    public decimal? BaseCommissionRate { get; set; }

    /// <summary>
    /// 一级销售提点：直接推广者获得的分成比例
    /// </summary>
    public decimal? Level1CommissionRate { get; set; }

    /// <summary>
    /// 二级销售提点：由一级销售发展的下级产生销售时，一级获得的分成比例
    /// </summary>
    public decimal? Level2CommissionRate { get; set; }


    /// <summary>
    /// 三级销售提点：二级销售发展的下级产生销售时，一、二级可获得的分成比例（按系统设计）
    /// </summary>
    public decimal? Level3CommissionRate { get; set; }
}
