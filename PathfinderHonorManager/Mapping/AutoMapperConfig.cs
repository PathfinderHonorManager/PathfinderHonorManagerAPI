using System;
using AutoMapper;
using PathfinderHonorManager.Dto.Incoming;
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
            RegisterAchievementMappings();
            RegisterPathfinderAchievementMappings();
        }

        private void RegisterPathfinderMappings()
        {
            CreateMap<Pathfinder, Outgoing.PathfinderDto>()
                .IncludeMembers(p => p.PathfinderClass)
                .IncludeMembers(p => p.Club);
            CreateMap<Pathfinder, Outgoing.PathfinderDependantDto>()
                .IncludeMembers(p => p.PathfinderClass);
            CreateMap<Pathfinder, Incoming.PathfinderDtoInternal>();
            CreateMap<PathfinderClass, Outgoing.PathfinderDependantDto>();
            CreateMap<PathfinderClass, Outgoing.PathfinderDto>();
            CreateMap<PathfinderHonor, Outgoing.PathfinderDependantDto>();
            CreateMap<Club, Outgoing.PathfinderDto>();
            CreateMap<Incoming.PathfinderDto, Incoming.PathfinderDtoInternal>();
            CreateMap<Incoming.PathfinderDto, Pathfinder>();
            CreateMap<Incoming.PathfinderDtoInternal, Pathfinder>();
            CreateMap<Incoming.PathfinderDtoInternal, Club>()
                .ForMember(dest => dest.ClubID, opt => opt.MapFrom(src => src.ClubID));
            CreateMap<Outgoing.PathfinderDependantDto, Outgoing.PathfinderDto>();
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

        private void RegisterAchievementMappings()
        {
            CreateMap<Achievement, Outgoing.AchievementDto>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src => src.PathfinderClass.ClassName))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.CategorySequenceOrder, opt => opt.MapFrom(src => src.Category.CategorySequenceOrder))
                .ForMember(dest => dest.LevelName, opt => opt.MapFrom(src => Enum.GetName(typeof(LevelName), src.Level)));
            CreateMap<Outgoing.AchievementDto, Achievement>();
        }

        private void RegisterPathfinderAchievementMappings()
        {
            CreateMap<PathfinderAchievement, Outgoing.PathfinderAchievementDto>();
            CreateMap<Outgoing.PathfinderAchievementDto, PathfinderAchievement>();
            CreateMap<Incoming.PostPathfinderAchievementDto, PathfinderAchievement>();
            CreateMap<Incoming.PathfinderAchievementDto, PathfinderAchievement>();
            CreateMap<Incoming.PathfinderAchievementDto, Outgoing.PathfinderAchievementDto>();
            CreateMap<Incoming.PutPathfinderAchievementDto, Incoming.PathfinderAchievementDto>();
            CreateMap<Incoming.PutPathfinderAchievementDto, PathfinderAchievement>();
            CreateMap<PathfinderAchievement, Incoming.PutPathfinderAchievementDto>();
            CreateMap<PathfinderAchievement, Incoming.PathfinderAchievementDto>();
        }
    }
}