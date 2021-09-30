using System.Linq;
using AutoMapper;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using PathfinderHonorManager.Model.Enum;
using PathfinderHonorManager.Model;

namespace PathfinderHonorManager.Mapping
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            RegisterPathfinderMappings();
            RegisterHonorMappings();
            RegisterPathfinderHonorMappings();
        }

        private void RegisterPathfinderMappings()
        {
            CreateMap<Pathfinder, Outgoing.PathfinderDto>();
            CreateMap<Pathfinder, Outgoing.PathfinderDependantDto>();
            CreateMap<Incoming.PathfinderDto, Pathfinder>();
            CreateMap<PathfinderHonor, Outgoing.PathfinderDependantDto>();
        }


        private void RegisterHonorMappings()
        {
            CreateMap<Honor, Outgoing.HonorDto>();
        }

        private void RegisterPathfinderHonorMappings()
        {
            CreateMap<Honor, Outgoing.PathfinderHonorDto>();
            CreateMap<Honor, Outgoing.PathfinderHonorChildDto>();
            CreateMap<Incoming.PathfinderHonorDto, PathfinderHonor>();
            CreateMap<PathfinderHonor, Outgoing.PathfinderHonorDto>()
                .IncludeMembers(s => s.Honor, s => s.PathfinderHonorStatus);
            CreateMap<PathfinderHonor, Outgoing.PathfinderHonorChildDto>()
                .IncludeMembers(s => s.Honor, s => s.PathfinderHonorStatus);
            CreateMap<PathfinderHonorStatus, Outgoing.PathfinderHonorDto>();
            CreateMap<PathfinderHonorStatus, Outgoing.PathfinderHonorChildDto>();
            CreateMap<PathfinderHonorStatus, Outgoing.PathfinderHonorStatusDto>();
            CreateMap<HonorStatus, Outgoing.PathfinderHonorStatusDto>();
        }
    }
}
