using QT.Common.Models.User;
using QT.Systems.Entitys.Permission;
using Mapster;
using QT.Common.Extension;

namespace QT.Common.Core.Mapper;

/// <summary>
/// 对象映射.
/// </summary>
public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<UserEntity, UserInfoModel>()
           .Map(dest => dest.userId, src => src.Id)
           .Map(dest => dest.userAccount, src => src.Account)
           .Map(dest => dest.userName, src => src.RealName)
           .Map(dest => dest.headIcon, src => src.HeadIcon.IsNotEmptyOrNull() && src.HeadIcon.StartsWith("/") ? src.HeadIcon : ("/api/File/Image/userAvatar/" + src.HeadIcon))
           .Map(dest => dest.prevLoginTime, src => src.PrevLogTime)
           .Map(dest => dest.prevLoginIPAddress, src => src.PrevLogIP);
    }
}