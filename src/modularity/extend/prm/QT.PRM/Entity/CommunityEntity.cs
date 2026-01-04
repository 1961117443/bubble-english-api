using QT.Common.Const;
using QT.Common.Contracts;
using SqlSugar;

namespace QT.PRM.Entitys;


/// <summary>
/// 苑区表
/// </summary>
[SugarTable("prm_community")]
[Tenant(ClaimConst.TENANTID)]
public class CommunityEntity : CLDEntityBase
{
    /// <summary>
    /// 苑区名称
    /// </summary>
    [SugarColumn(Length = 100, IsNullable = false)]
    public string Name { get; set; }

    /// <summary>
    /// 省份
    /// </summary>
    [SugarColumn(Length = 50)]
    public string Province { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    [SugarColumn(Length = 50)]
    public string City { get; set; }

    /// <summary>
    /// 区县
    /// </summary>
    [SugarColumn(Length = 50)]
    public string District { get; set; }

    /// <summary>
    /// 详细地址
    /// </summary>
    [SugarColumn(Length = 255, ColumnName = "address_detail")]
    public string AddressDetail { get; set; }

    /// <summary>
    /// 经度
    /// </summary>
    [SugarColumn(DecimalDigits = 6)]
    public decimal Longitude { get; set; }

    /// <summary>
    /// 纬度
    /// </summary>
    [SugarColumn(DecimalDigits = 6)]
    public decimal Latitude { get; set; }

    /// <summary>
    /// 附件JSON
    /// </summary>
    [SugarColumn]
    public string AttachmentJson { get; set; }
}
