using QT.Common;
using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 仓库管理
/// </summary>
[SugarTable("iot_maintenance_material")]
[EntityUniqueProperty("Code")]
public class MaintenanceMaterialEntity : CUDEntityBase
{
    [Description("物品编码")]
    public string Code { get; set; }

    public string Name { get; set; }
    public string ShortName { get; set; }

    public string BarCode { get; set; }

    public string CategoryId { get; set; }
    public string Unit { get; set; }
    public string Spec { get; set; }
    public string Color { get; set; }
    public string Brand { get; set; }
    public decimal TaxRate { get; set; }
    public string Size { get; set; }
    public decimal Weight { get; set; }
    public string ImageUrl { get; set; }

    public string Remark { get; set; }

    public string ProjectId { get; set; }
}
