//using Mapster;

//namespace QT.CMS.Entitys.Profiles.Manager;


///// <summary>
///// 管理员实体映射
///// </summary>
//public class ManagerProfile : IRegister
//{
//    public void Register(TypeAdapterConfig config)
//    {
//        //管理员,将源数据映射到DTO
//        config.ForType<Manager, ManagerDto>()
//            .ForMember(
//                dest => dest.UserName,
//                opt =>
//                {
//                    opt.MapFrom(src => src.User != null ? src.User.UserName : null);
//                }
//            ).ForMember(
//                dest => dest.Email,
//                opt =>
//                {
//                    opt.MapFrom(src => src.User != null ? src.User.Email : null);
//                }
//            ).ForMember(
//                dest => dest.Phone,
//                opt =>
//                {
//                    opt.MapFrom(src => src.User != null ? src.User.PhoneNumber : null);
//                }
//            ).ForMember(
//                dest => dest.RoleId,
//                opt =>
//                {
//                    opt.MapFrom(src => src.User != null ? src.User!.UserRoles!.FirstOrDefault()!.RoleId : 0);
//                }
//            ).ForMember(
//                dest => dest.Status,
//                opt =>
//                {
//                    opt.MapFrom(src => src.User != null ? src.User.Status : 0);
//                }
//            );
//        //管理员,将DTO映射到源数据
//        config.ForType<ManagerDto, Manager>();
//        config.ForType<ManagerEditDto, Manager>();

//        //管理角色,将源数据映射到DTO
//        config.ForType<ApplicationRole, ManagerRolesDto>();
//        //管理角色,将DTO映射到源数据
//        config.ForType<ManagerRolesDto, ApplicationRole>();
//        config.ForType<ManagerRolesEditDto, ApplicationRole>();

//        //管理日志,将源数据映射到DTO
//        config.ForType<ManagerLog, ManagerLogDto>();
//        //管理日志,将DTO映射到源数据
//        config.ForType<ManagerLogDto, ManagerLog>();

//        //管理菜单,将源数据映射到DTO
//        config.ForType<ManagerMenu, ManagerMenuDto>();
//        config.ForType<ManagerMenu, ManagerMenuEditDto>();
//        //管理菜单,将DTO映射到源数据
//        config.ForType<ManagerMenuEditDto, ManagerMenu>();
//    }
//}
