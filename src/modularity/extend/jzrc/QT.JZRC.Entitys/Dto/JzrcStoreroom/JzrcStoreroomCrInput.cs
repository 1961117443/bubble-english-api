
namespace QT.JZRC.Entitys.Dto.JzrcStoreroom;

/// <summary>
/// 档案室管理修改输入参数.
/// </summary>
public class JzrcStoreroomCrInput
{
    /// <summary>
    /// 名称.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 简介.
    /// </summary>
    public string remark { get; set; }

    /// <summary>
    /// 负责人.
    /// </summary>
    public string adminId { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    public string adminTel { get; set; }

    /// <summary>
    /// 编号.
    /// </summary>
    public string code { get; set; }

    /// <summary>
    /// 上级id.
    /// </summary>
    public string pId { get; set; }

}