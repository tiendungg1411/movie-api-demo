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
    public class PersonController : ControllerBase
    {
        private readonly MovieDbContext _context;
        public PersonController(MovieDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get(int pageIndex = 0, int pageSize = 10)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var actorCount = _context.Person.Count();
                var actorList = _context.Person.Skip(pageIndex * pageSize).Take(pageSize).
                    Select(x => new ActorViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        DateOfBirth = x.DateOfBirth
                    }).ToList();

                response.Status = true;
                response.Message = "Success";
                response.Data = new { Persons = actorList, Count = actorCount };
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
        public IActionResult GetPersonByName(string name)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var person = _context.Person.Where(x => x.Name.Contains(name)).FirstOrDefault();

                if (person == null)
                {
                    response.Status = false;
                    response.Message = "Record not exist";
                    return BadRequest(response);
                }
                var personData = new ActorDetailViewModel
                {
                    Id = person.Id,
                    Name = person.Name,
                    DateOfBirth = person.DateOfBirth,
                    Movies = _context.Movie.Where(x => x.Actors.Contains(person)).Select(x => x.Title).ToArray()
                };

                response.Status = true;
                response.Message = "Success";
                response.Data = personData;
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
        public IActionResult Post(ActorViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                if (ModelState.IsValid)
                {
                    var postedModel = new PersonEntity()
                    {
                        Name = model.Name,
                        DateOfBirth = model.DateOfBirth
                    };
                    _context.Person.Add(postedModel);
                    _context.SaveChanges();

                   model.Id = postedModel.Id;

                    response.Status = true;
                    response.Message = "Created successfully.";
                    response.Data = model;
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
