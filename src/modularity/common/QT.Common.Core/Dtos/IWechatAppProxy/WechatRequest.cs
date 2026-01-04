namespace QT.Common.Core.Dto.IWechatAppProxy;



public class WxacodeUnlimitRequest
{
    public string page { get; set; }

    public string scene { get; set; }

    public bool check_path { get; set; }

    //public string env_version { get; set; }
}


public class WxGetUserPhonenumberRequest
{
    public string code { get; set; }
}