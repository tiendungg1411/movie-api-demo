using AutoMapper;
using Data.DataContext;
using Data.Entities;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace MovieAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly MovieDbContext _context;
        private readonly IMapper _mapper;
        public MovieController(MovieDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex = 0, int pageSize = 10)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var movieCount = _context.Movie.Count();
                var movieList = _mapper.Map<List<MovieListViewModel>>(_context.Movie.Include(item => item.Actors)
                    .Skip(pageIndex * pageSize).Take(pageSize).ToList());

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

        [HttpGet("{id}")]
        public IActionResult GetMovieById(Guid id)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var movie = _mapper.Map<MovieDetailViewModel>(_context.Movie.Include(item => item.Actors)
                    .Where(x => x.Id == id).FirstOrDefault());

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
                    var postedModel = _mapper.Map<MovieEntity>(model);
                    postedModel.Actors = actors;
                    _context.Movie.Add(postedModel);
                    _context.SaveChanges();

                    var responseData = _mapper.Map<MovieDetailViewModel>(postedModel);

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
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong  " + ex.Message;
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
                    movieDetail.CoverImage = model.CoverImage;
                    movieDetail.Description = model.Description;
                    movieDetail.Language = model.Language;
                    movieDetail.Title = model.Title;
                    movieDetail.ReleaseDate = model.ReleaseDate;
                    movieDetail.Actors = actors;
                    _context.SaveChanges();
                    var responseData = _mapper.Map<MovieDetailViewModel>(movieDetail);

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
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong " + ex.Message ;
                return BadRequest(response);
            }
        }

        [HttpDelete]
        public IActionResult Delete(Guid id)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var movie = _context.Movie.Where(item => item.Id == id).FirstOrDefault();
                if(movie == null)
                {
                    response.Status = false;
                    response.Message = "Invalid record";
                    return BadRequest(response);
                }
                _context.Movie.Remove(movie);
                _context.SaveChanges();
                response.Status = true;
                response.Message = "Deleted successfully";
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong";
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("upload-movie-poster")]
        public async Task<IActionResult> UploadMoviePoster(IFormFile imageFile)
        {
            try
            {
                var fileName = ContentDispositionHeaderValue.Parse(imageFile.ContentDisposition).FileName.TrimStart('\"').TrimEnd('\"');
                string newPath = @"D:\MoviePoster";
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
                string[] allowedImageExtensions = new string[] { ".jpg", ".jpeg", ".png" };
                if(!allowedImageExtensions.Contains(Path.GetExtension(fileName)))
                {
                    return BadRequest(new BaseResponseModel
                    {
                        Status = false,
                        Message = "Only .jpg, .jped, .png type files are allowed."
                    });
                }

                string newFileName = Guid.NewGuid() + Path.GetExtension(fileName);
                string fullFilePath = Path.Combine(newPath, newFileName);

                using (var stream = new FileStream(fullFilePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                return Ok(new { ProfileImage = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/StaticFiles/{newFileName}"});
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponseModel
                {
                    Status = false,
                    Message = "Something went wrong"
                });
                
            }
        }
    }
}
