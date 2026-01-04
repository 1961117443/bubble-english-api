using Mapster;
using QT.Application.Entitys.Dto.ExtExpenseRecord;
using QT.Application.Entitys.Dto.FreshDelivery.ErpCustomer;
using QT.Common.Extension;
using QT.Common.Models;
using QT.Common.Security;
using QT.Extend.Entitys;
using QT.Extend.Entitys.Dto.ProjectGantt;
using QT.Extend.Entitys.Dto.QuoteCustomer;
using QT.Systems.Entitys.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QT.Application.Entitys.Dto;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<UserEntity, ManagersInfo>()
            .Map(dest => dest.account, src => src.RealName)
            .Map(dest => dest.firstName, src => src.RealName.IsNotEmptyOrNull()? src.RealName.Substring(0,1):"")
            .Map(dest=>dest.headIcon,src=> src.HeadIcon.IsNotEmptyOrNull() && src.HeadIcon.StartsWith("/") ? src.HeadIcon : ($"/api/File/Image/userAvatar/{src.HeadIcon}"))
            ;


        #region 报价系统 Quote

        config.ForType<QuoteCustomerEntity, QuoteCustomerInfoOutput>()
                .Map(dest => dest.license, src => string.IsNullOrEmpty(src.License) ? new List<FileControlsModel>() : src.License.ToObject<List<FileControlsModel>>());

        config.ForType<QuoteCustomerInfoOutput, QuoteCustomerEntity>()
            .Map(dest => dest.License, src => src.license.IsAny() ? src.license.ToJsonString() : string.Empty);

        config.ForType<QuoteCustomerCrInput, QuoteCustomerEntity>()
            .Map(dest => dest.License, src => src.license.IsAny() ? src.license.ToJsonString() : string.Empty);
        #endregion


        #region 客户档案 ErpCustomerEntity

        config.ForType<ErpCustomerEntity, ErpCustomerInfoOutput>()
                .Map(dest => dest.license, src => string.IsNullOrEmpty(src.License) ? new List<FileControlsModel>() : src.License.ToObject<List<FileControlsModel>>());

        config.ForType<ErpCustomerInfoOutput, ErpCustomerEntity>()
            .Map(dest => dest.License, src => src.license.IsAny() ? src.license.ToJsonString() : string.Empty);

        config.ForType<ErpCustomerCrInput, ErpCustomerEntity>()
            .Map(dest => dest.License, src => src.license.IsAny() ? src.license.ToJsonString() : string.Empty);
        #endregion

        #region 智能财务
        config.ForType<ExtExpenseRecordCrInput, ExtExpenseRecordEntity>()
            .Map(dest => dest.ImageJson, src => src.imageJson.ToJsonString())
        ;
        config.ForType<ExtExpenseRecordEntity, ExtExpenseRecordInfoOutput>()
            .Map(dest => dest.imageJson, src => src.ImageJson.ToObject<List<FileControlsModel>>())
        ;
        config.ForType<ExtExpenseRecordEntity, ExtExpenseRecordCrInput>()
            .Map(dest => dest.imageJson, src => src.ImageJson.ToObject<List<FileControlsModel>>())
        ; 
        #endregion
    }
}
