using AutoMapper;
using Bankmore.Accounts.Command.Domain.Accounts;
using Bankmore.Accounts.Command.Infrastructure.Db.Entities;

namespace Bankmore.Command.Infra.Profiles;

public class AccountDBProfile : Profile
{
    public AccountDBProfile()
    {
        CreateMap<CreateAccountModel, AccountDbModel>().ReverseMap();
    }
}