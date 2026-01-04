using QT.CMS.Entitys.Dto.Parameter;
using QT.DataValidation;
using System.ComponentModel.DataAnnotations;

namespace QT.CMS.Entitys.Dto.Base;

///// <summary>
///// 站点(显示)
///// </summary>
//public class SitesDto : SitesEditDto
//{
//    /// <summary>
//    /// 自增ID
//    /// </summary>
//    [Display(Name = "自增ID")]
//    public int id { get; set; }

//    /// <summary>
//    /// 创建人
//    /// </summary>
//    [Display(Name = "创建人")]
//    [StringLength(128)]
//    public string? addBy { get; set; }

//    /// <summary>
//    /// 创建时间
//    /// </summary>
//    [Display(Name = "创建时间")]
//    public DateTime addTime { get; set; } = DateTime.Now;
//}

/// <summary>
/// 联系客服(编辑)
/// </summary>
public class ContactsEditDto
{

    /// <summary>
    /// 主题
    /// </summary>
    [Display(Name = "主题")]
    //[Required(ErrorMessage = "{0}不可为空")]
    public string? subject { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    [Display(Name = "姓名")]
    //[Required(ErrorMessage = "{0}不可为空")]
    public string? addBy { get; set; }

    

    /// <summary>
    /// 电话
    /// </summary>
    [Display(Name = "电话")]
    //[DataValidation(ValidationTypes.PhoneNumber,ErrorMessage ="手机号码格式错误")]
    public string? phone { get; set; }

    /// <summary>
    /// 站点
    /// </summary>
    public int siteId { get; set; }

    /// <summary>
    /// 短信验证码
    /// </summary>
    [Display(Name = "短信验证码")]
    //[Required(ErrorMessage = "{0}不可为空")]
    public string? code { get; set; }

    /// <summary>
    /// 留言
    /// </summary>
    [Display(Name = "留言")]
    public string? remark { get; set; }



    /// <summary>
    /// 扩展信息
    /// </summary>
    [Display(Name = "扩展信息")]
    public string? extend { get; set; }
}


/// <summary>
/// 站点(显示)
/// </summary>
public class ContactsDto : ContactsEditDto
{
    /// <summary>
    /// 自增ID
    /// </summary>
    [Display(Name = "自增ID")]
    public int id { get; set; }


    /// <summary>
    /// IP地址
    /// </summary>
    [Display(Name = "IP")]
    public string? ip { get; set; }


    /// <summary>
    /// 是否已发送短信
    /// </summary>
    [Display(Name = "是否已发送短信")]
    public byte isSend { get; set; } = 0;



    /// <summary>
    /// 创建时间
    /// </summary>
    [Display(Name = "创建时间")]
    public DateTime? addTime { get; set; }




}

public class ContactParameter : BaseParameter
{
    public int? siteId { get; set; }
}