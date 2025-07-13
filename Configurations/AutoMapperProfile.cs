using AutoMapper;
using PengerAPI.DTOs;
using PengerAPI.Models;

namespace PengerAPI.Configurations
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Accounts, opt => opt.MapFrom(src => src.Accounts));
            CreateMap<User, UserSummaryDto>();
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RememberToken, opt => opt.Ignore())
                .ForMember(dest => dest.EmailVerifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Accounts, opt => opt.Ignore())
                .ForMember(dest => dest.OTPs, opt => opt.Ignore());
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.RememberToken, opt => opt.Ignore())
                .ForMember(dest => dest.EmailVerifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Accounts, opt => opt.Ignore())
                .ForMember(dest => dest.OTPs, opt => opt.Ignore());

            // Account mappings
            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => src.AccountType));
            CreateMap<Account, AccountSummaryDto>()
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency.Code))
                .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Currency.Symbol))
                .ForMember(dest => dest.AccountTypeName, opt => opt.MapFrom(src => src.AccountType.Name));
            CreateMap<Account, AccountBalanceDto>()
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency.Code))
                .ForMember(dest => dest.CurrencySymbol, opt => opt.MapFrom(src => src.Currency.Symbol));
            CreateMap<CreateAccountDto, Account>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AccountNumber, opt => opt.Ignore()) // Will be generated
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.InitialBalance))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.AccountType, opt => opt.Ignore());
            CreateMap<UpdateAccountDto, Account>()
                .ForMember(dest => dest.AccountNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Balance, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.AccountType, opt => opt.Ignore());

            // Currency mappings
            CreateMap<Currency, CurrencyDto>();
            CreateMap<Currency, CurrencySummaryDto>();
            CreateMap<CreateCurrencyDto, Currency>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Accounts, opt => opt.Ignore());
            CreateMap<UpdateCurrencyDto, Currency>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Accounts, opt => opt.Ignore());

            // AccountType mappings
            CreateMap<AccountType, AccountTypeDto>();
            CreateMap<AccountType, AccountTypeSummaryDto>();
            CreateMap<CreateAccountTypeDto, AccountType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Accounts, opt => opt.Ignore());
            CreateMap<UpdateAccountTypeDto, AccountType>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Accounts, opt => opt.Ignore());

            // OTP mappings
            CreateMap<OTP, OTPDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));
            CreateMap<OTP, OTPSummaryDto>();
            CreateMap<GenerateOTPDto, OTP>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Code, opt => opt.Ignore()) // Will be generated
                .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore()) // Will be calculated
                .ForMember(dest => dest.IsUsed, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }
    }
}
