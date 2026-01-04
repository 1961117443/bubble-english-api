using QT.Common;
using QT.Common.Contracts;
using SqlSugar;
using System.ComponentModel;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 仓库管理
/// </summary>
[SugarTable("iot_material_warehouse")]
[EntityUniqueProperty("Code")]
public class MaterialWareHouseEntity : CUDEntityBase
{

    [Description("类别编码")]
    public string Code { get; set; }

    public string Name { get; set; }

    public string ParentId { get; set; }

    public string Remark { get; set; }
}
