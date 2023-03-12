using AutoMapper;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Model.Enum;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Mapping
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            RegisterPathfinderMappings();
            RegisterHonorMappings();
            RegisterPathfinderHonorMappings();
            RegisterClubMappings();
        }

        private void RegisterPathfinderMappings()
        {
            CreateMap<Pathfinder, Outgoing.PathfinderDto>()
                .IncludeMembers(p => p.PathfinderClass)
                .IncludeMembers(p => p.Club);
            CreateMap<Club, Outgoing.PathfinderDto>();
            CreateMap<PathfinderClass, Outgoing.PathfinderDto>();
            CreateMap<Pathfinder, Outgoing.PathfinderDependantDto>()
                .IncludeMembers(p => p.PathfinderClass);
            CreateMap<Outgoing.PathfinderDependantDto, Outgoing.PathfinderDto>();
            CreateMap<PathfinderClass, Outgoing.PathfinderDependantDto>();
            CreateMap<Incoming.PathfinderDto, Pathfinder>();
            CreateMap<PathfinderHonor, Outgoing.PathfinderDependantDto>();
            CreateMap<Incoming.PathfinderDto, Incoming.PathfinderDtoInternal>();
            CreateMap<Pathfinder, Incoming.PathfinderDtoInternal>();
            CreateMap<Incoming.PathfinderDtoInternal, Pathfinder>();
            CreateMap<Incoming.PathfinderDtoInternal, Club>()
                .ForMember(dest => dest.ClubID, opt => opt.MapFrom(src => src.ClubID));

        }

        private void RegisterHonorMappings()
        {
            CreateMap<Honor, Outgoing.HonorDto>();
            CreateMap<Incoming.HonorDto, Honor>();
        }


        private void RegisterClubMappings()
        {
            CreateMap<Club, Outgoing.ClubDto>();
        }

        private void RegisterPathfinderHonorMappings()
        {
            CreateMap<Honor, Outgoing.PathfinderHonorDto>();
            CreateMap<Incoming.PathfinderHonorDto, PathfinderHonor>();
            CreateMap<PathfinderHonor, Outgoing.PathfinderHonorDto>()
                .IncludeMembers(s => s.Honor, s => s.PathfinderHonorStatus);
            CreateMap<PathfinderHonorStatus, Outgoing.PathfinderHonorDto>();
            CreateMap<PathfinderHonorStatus, Outgoing.PathfinderHonorStatusDto>();
            CreateMap<HonorStatus, Outgoing.PathfinderHonorStatusDto>();
        }
    }
}
