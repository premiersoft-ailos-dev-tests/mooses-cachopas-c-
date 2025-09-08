using AutoMapper;
using Bankmore.Accounts.Command.Application.Commands.Transactions.Movments;
using Bankmore.Accounts.Command.Domain.Models.Movments;

namespace Bankmore.Accounts.Command.Application.Profiles.Aplication.Movments;

public class MovmentsProfile :Profile
{
    public MovmentsProfile()
    {
        CreateMap<MovmentCommand, MovmentModel>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => DateTime.Now));
    }
}