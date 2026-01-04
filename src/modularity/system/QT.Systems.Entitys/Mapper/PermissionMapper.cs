using QT.Common.Models.User;
using QT.Systems.Entitys.Dto.Department;
using QT.Systems.Entitys.Dto.OrganizeAdministrator;
using QT.Systems.Entitys.Dto.User;
using QT.Systems.Entitys.Permission;
using Mapster;

namespace QT.Systems.Entitys.Mapper;

/// <summary>
/// 权限模块对象映射.
/// </summary>
public class PermissionMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<UserEntity, UserInfoModel>()
               .Map(dest => dest.userId, src => src.Id)
               .Map(dest => dest.userAccount, src => src.Account)
               .Map(dest => dest.userName, src => src.RealName)
               .Map(dest => dest.headIcon, src => "/api/File/Image/userAvatar/" + src.HeadIcon)
               .Map(dest => dest.prevLoginTime, src => src.PrevLogTime)
               .Map(dest => dest.prevLoginIPAddress, src => src.PrevLogIP);
        config.ForType<UserEntity, UserInfoOutput>()
             .Map(dest => dest.headIcon, src => "/api/File/Image/userAvatar/" + src.HeadIcon);
        config.ForType<UserEntity, UserSelectorOutput>()
            .Map(dest => dest.fullName, src => src.RealName + "/" + src.Account)
            .Map(dest => dest.type, src => "user")
            .Map(dest => dest.parentId, src => src.OrganizeId);
        config.ForType<OrganizeEntity, UserSelectorOutput>()
            .Map(dest => dest.type, src => src.Category)
            .Map(dest => dest.icon, src => "icon-qt icon-qt-tree-organization3");
        config.ForType<OrganizeEntity, DepartmentSelectorOutput>()
             .Map(dest => dest.type, src => src.Category);
        config.ForType<OrganizeAdminIsTratorCrInput, OrganizeAdministratorEntity>()
            .Ignore(dest => dest.UserId);
    }
}