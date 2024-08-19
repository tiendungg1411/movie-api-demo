﻿using Data.DataContext;
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
                var movieList = _context.Movie.Include(item => item.Actors).Skip(pageIndex * pageSize).Take(pageSize).ToList();

                response.Status = true;
                response.Message = "Success";
                response.Data = new {Movies = movieList, Count = movieCount};
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
        public IActionResult GetMovieByName(string name)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var movie = _context.Movie.Include(item => item.Actors).Where(x => x.Title.Contains(name)).FirstOrDefault();

                if(movie == null)
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
                    if(actors.Count != model.Actors.Count)
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
                    response.Status = true;
                    response.Message = "Created successfully.";
                    response.Data = postedModel;
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