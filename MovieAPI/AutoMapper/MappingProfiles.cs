using AutoMapper;
using Data.Entities;
using Data.Models;

namespace MovieAPI.AutoMapper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<MovieEntity, MovieListViewModel>();
            CreateMap<MovieEntity, MovieDetailViewModel>();
            CreateMap<MovieListViewModel, MovieEntity>();
            CreateMap<CreateMovieViewModel, MovieEntity>().ForMember(x => x.Actors, y => y.Ignore());

            CreateMap<PersonEntity, ActorViewModel>();
            CreateMap<PersonEntity, ActorDetailViewModel>();
            CreateMap<PersonEntity, LoginModel>();
            CreateMap<PersonEntity, RegisterModel>();

            CreateMap<ActorViewModel, PersonEntity>();
            CreateMap<LoginModel, PersonEntity>();
            CreateMap<RegisterModel, PersonEntity>();
            
        }
    }
}
