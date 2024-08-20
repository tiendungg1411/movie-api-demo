using Data.DataContext;
using Data.Entities;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MovieAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly MovieDbContext _context;
        public MovieController(MovieDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex = 0, int pageSize = 10)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var movieCount = _context.Movie.Count();
                var movieList = _context.Movie.Include(item => item.Actors).Skip(pageIndex * pageSize).Take(pageSize).
                    Select(x => new MovieListViewModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Actors = x.Actors.Select(y => new ActorViewModel
                        {
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth
                        }).ToList(),
                        CoverImage = x.CoverImage,
                        Language = x.Language,
                        ReleaseDate = x.ReleaseDate
                    }).ToList();

                response.Status = true;
                response.Message = "Success";
                response.Data = new { Movies = movieList, Count = movieCount };
                return Ok(response);

            }
            catch (Exception)
            {
                //TODO: do logging exceptions
                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }

        [HttpGet("{name}")]
        public IActionResult GetMovieByName(string name)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var movie = _context.Movie.Include(item => item.Actors).Where(x => x.Title.Contains(name)).
                    Select(x => new MovieDetailViewModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Actors = x.Actors.Select(y => new ActorViewModel
                        {
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth
                        }).ToList(),
                        CoverImage = x.CoverImage,
                        Language = x.Language,
                        ReleaseDate = x.ReleaseDate,
                        Description = x.Description
                    }).FirstOrDefault();

                if (movie == null)
                {
                    response.Status = false;
                    response.Message = "Record not exist";
                    return BadRequest(response);
                }
                response.Status = true;
                response.Message = "Success";
                response.Data = movie;
                return Ok(response);

            }
            catch (Exception)
            {
                //TODO: do logging exceptions
                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }

        [HttpPost]
        public IActionResult Post(CreateMovieViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                if (ModelState.IsValid)
                {
                    var actors = _context.Person.Where(item => model.Actors.Contains(item.Id)).ToList();
                    if (actors.Count != model.Actors.Count)
                    {
                        response.Status = false;
                        response.Message = "Invalid Actor assigned.";
                        return BadRequest(response);
                    }
                    var postedModel = new MovieEntity()
                    {
                        Title = model.Title,
                        ReleaseDate = model.ReleaseDate,
                        Language = model.Language,
                        CoverImage = model.CoverImage,
                        Description = model.Description,
                        Actors = actors
                    };
                    _context.Movie.Add(postedModel);
                    _context.SaveChanges();

                    var responseData = new MovieDetailViewModel
                    {
                        Id = postedModel.Id,
                        Title = postedModel.Title,
                        Actors = postedModel.Actors.Select(y => new ActorViewModel
                        {
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth
                        }).ToList(),
                        CoverImage = postedModel.CoverImage,
                        Language = postedModel.Language,
                        ReleaseDate = postedModel.ReleaseDate,
                        Description = postedModel.Description
                    };

                    response.Status = true;
                    response.Message = "Created successfully.";
                    response.Data = responseData;
                    return Ok(response);
                }
                else
                {
                    response.Status = false;
                    response.Message = "Validation failed";
                    response.Data = ModelState;
                    return BadRequest(response);
                }
            }
            catch (Exception)
            {
                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }

        [HttpPut]
        public IActionResult Put(CreateMovieViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                if (ModelState.IsValid)
                {
                    var actors = _context.Person.Where(item => model.Actors.Contains(item.Id)).ToList();
                    if (actors.Count != model.Actors.Count)
                    {
                        response.Status = false;
                        response.Message = "Invalid Actor assigned.";
                        return BadRequest(response);
                    }

                    var movieDetail = _context.Movie.Include(item => item.Actors).Where(item => item.Id == model.Id).FirstOrDefault();
                    if (movieDetail == null)
                    {
                        response.Status = false;
                        response.Message = "Invalid movie record";
                        return BadRequest(response);
                    }
                    movieDetail.CoverImage = model.CoverImage;
                    movieDetail.Description = model.Description;
                    movieDetail.Language = model.Language;
                    movieDetail.Title = model.Title;
                    movieDetail.ReleaseDate = model.ReleaseDate;

                    // Find removed actors
                    var removedActors = movieDetail.Actors.Where(item => !model.Actors.Contains(item.Id)).ToList();
                    foreach (var actor in removedActors)
                    {
                        movieDetail.Actors.Remove(actor);
                    }

                    // Find added actors
                    var addedActors = actors.Except(movieDetail.Actors).ToList();
                    foreach (var actor in addedActors)
                    {
                        movieDetail.Actors.Add(actor);
                    }
                    _context.SaveChanges();
                    var responseData = new MovieDetailViewModel
                    {
                        Id = movieDetail.Id,
                        Title = movieDetail.Title,
                        Actors = movieDetail.Actors.Select(y => new ActorViewModel
                        {
                            Id = y.Id,
                            Name = y.Name,
                            DateOfBirth = y.DateOfBirth
                        }).ToList(),
                        CoverImage = movieDetail.CoverImage,
                        Language = movieDetail.Language,
                        ReleaseDate = movieDetail.ReleaseDate,
                        Description = movieDetail.Description
                    };

                    response.Status = true;
                    response.Message = "Updated successfully.";
                    response.Data = responseData;
                    return Ok(response);
                }
                else
                {
                    response.Status = false;
                    response.Message = "Validation failed";
                    response.Data = ModelState;
                    return BadRequest(response);
                }
            }
            catch (Exception)
            {
                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }
    }
}
