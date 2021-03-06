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
                .IncludeMembers(p => p.PathfinderClass);
            CreateMap<PathfinderClass, Outgoing.PathfinderDto>();
            CreateMap<Pathfinder, Outgoing.PathfinderDependantDto>()
                .IncludeMembers(p => p.PathfinderClass);
            CreateMap<Outgoing.PathfinderDependantDto, Outgoing.PathfinderDto>();
            CreateMap<PathfinderClass, Outgoing.PathfinderDependantDto>();
            CreateMap<Incoming.PathfinderDto, Pathfinder>();
            CreateMap<PathfinderHonor, Outgoing.PathfinderDependantDto>();
        }

        private void RegisterHonorMappings()
        {
            CreateMap<Honor, Outgoing.HonorDto>();
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
