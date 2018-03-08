using AutoMapper;
using Instagraph.DataProcessor.Dtos.Export;
using Instagraph.Models;

namespace Instagraph.App
{
    public class InstagraphProfile : Profile
    {
        public InstagraphProfile()
        {
			CreateMap<User, PopularUserDto>()
				.ForMember(dto => dto.Followers, cfg => cfg.MapFrom(u => u.Followers.Count));
		}
	}
}
