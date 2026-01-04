namespace QT.JXC.Entitys.Dto.Erp;

/// <summary>
/// 分拣员修改输入参数.
/// </summary>
public class ErpSorterCrInput
{

    /// <summary>
    /// 姓名.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 电话.
    /// </summary>
    public string mobile { get; set; }

    /// <summary>
    /// 登录帐号，角色叫分拣员.
    /// </summary>
    public string loginId { get; set; }

    /// <summary>
    /// 登录密码.
    /// </summary>
    public string loginPwd { get; set; }

    /// <summary>
    /// 线路分拣员（中间表）.
    /// </summary>
    public List<ErpDeliveryrouteSorterCrInput> erpDeliveryrouteSorterList { get; set; }

    /// <summary>
    /// 公司ID.
    /// </summary>
    public string oid { get; set; }
}