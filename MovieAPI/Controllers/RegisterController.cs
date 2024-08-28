using AutoMapper;
using Data.DataContext;
using Data.Entities;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MovieAPI.Common;

namespace MovieAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly MovieDbContext _context;
        private readonly IMapper _mapper;
        private readonly AuthenticateUser _authenticateService;

        public RegisterController(MovieDbContext context, IMapper mapper, AuthenticateUser authenticateService)
        {
            _context = context;
            _mapper = mapper;
            _authenticateService = authenticateService;
        }

        [HttpPost]
        public IActionResult RegisterAccount(RegisterModel model)
        {
            BaseResponseModel response =  new BaseResponseModel();
            try
            {
                if (ModelState.IsValid)
                {
                    var user = checkUserByEmail(model.Email);
                    if(user == null)
                    {
                        model.Password = _authenticateService.PasswordEncryption(model.Password);
                        _context.Person.Add(_mapper.Map<PersonEntity>(model));
                        _context.SaveChanges();

                        response.Status = true;
                        response.Message = "Register successfully!";
                        response.Data = new {Email =  model.Email, Name = model.Name};
                        return Ok(response);
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = "Register failed!";
                        return BadRequest(response);
                    }
                }
                else
                {
                    response.Status = false;
                    response.Message = "Something went wrong!";
                    return BadRequest(response);
                }
            }
            catch (Exception)
            {
                response.Status = false;
                response.Message = "Something went wrong!";
                return BadRequest(response);
            }
        }

        private PersonEntity checkUserByEmail(string email)
        {
            return _context.Person.Where(item => item.Email == email).FirstOrDefault();
        }
    }
}
