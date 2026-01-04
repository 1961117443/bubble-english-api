using QT.Common;
using QT.Common.Contracts;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Iot.Application.Entity;

/// <summary>
/// 物资类别
/// </summary>
[SugarTable("iot_material_category")]
[EntityUniqueProperty("Code")]
public class MaterialCategoryEntity: CUDEntityBase
{
    [Description("类别编码")]
    public string Code { get; set; }

    public string Name { get; set; }

    public string ParentId { get; set; }

    public string Remark { get; set; }
}
