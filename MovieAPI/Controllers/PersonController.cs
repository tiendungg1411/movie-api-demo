using AutoMapper;
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
        private readonly IMapper _mapper;
        public PersonController(MovieDbContext context, IMapper mapper)
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
                var actorCount = _context.Person.Count();
                var actorList = _mapper.Map<List<ActorViewModel>>(_context.Person.Skip(pageIndex * pageSize).Take(pageSize).ToList());

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

        [HttpGet("{id}")]
        public IActionResult GetPersonById(Guid id)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var person = _context.Person.Where(x => x.Id == id).FirstOrDefault();

                if (person == null)
                {
                    response.Status = false;
                    response.Message = "Record not exist";
                    return BadRequest(response);
                }
                var personData = _mapper.Map<ActorDetailViewModel>(person);

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

        [HttpGet]
        [Route("Search/{searchText}")]
        public IActionResult Get(string searchText)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                var searchPerson = _context.Person.Where(x => x.Name.Contains(searchText)).Select(x => new
                {
                    x.Id,
                    x.Name,
                }).ToList();
                response.Status = true;
                response.Message = "Success";
                response.Data = searchPerson;
                return Ok(response);
            }
            catch (Exception)
            {
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
                    var postedModel = _mapper.Map<PersonEntity>(model);
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

        [HttpDelete]
        public IActionResult Delete(Guid id)
        {
            var response = new BaseResponseModel();
            try
            {
                var person = _context.Person.Where(item => item.Id == id).FirstOrDefault();
                if(person == null)
                {
                    response.Status = false;
                    response.Message = "Invalid record";
                    return BadRequest(response);
                }
                _context.Person.Remove(person);
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

        [HttpPut]
        public IActionResult Put(ActorViewModel model)
        {
            BaseResponseModel response = new BaseResponseModel();
            try
            {
                if (ModelState.IsValid)
                {
                    var postedModel = _mapper.Map<PersonEntity>(model);
                    var personDetail = _context.Person.Where(item => item.Id == model.Id).AsNoTracking().FirstOrDefault();
                    if (personDetail == null)
                    {
                        response.Status = false;
                        response.Message = "Invalid record";
                        return BadRequest(response);
                    }
                    _context.Person.Update(postedModel);
                    _context.SaveChanges();

                    response.Status = true;
                    response.Message = "Updated successfully";
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
