namespace QT.Common.Core.Dto.IWechatAppProxy;



public class WxResponseBase
{

    public int errcode { get; set; }

    public string errmsg { get; set; }
}

public class WxGetUserPhonenumberResponse : WxResponseBase
{
    public WxGetUserPhonenumberResponseContent phone_info { get; set; }
}

public class WxGetUserPhonenumberResponseContent
{
    public string phoneNumber { get; set; }

    public string purePhoneNumber { get; set; }

    public string countryCode { get; set; }

}