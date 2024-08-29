using AutoMapper;
using Data.DataContext;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MovieAPI.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace MovieAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly MovieDbContext _context;
        private readonly IMapper _mapper;
        private readonly AuthenticateUser _authenticateUser;
        public LoginController(IConfiguration configuration, MovieDbContext context, IMapper mapper, AuthenticateUser authenticateUser)
        {
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
            _authenticateUser = authenticateUser;

        }

        private LoginModel AuthenticateUser(LoginModel model)
        {
            var userEntity = _context.Person.Where(item => item.Email == model.Email).FirstOrDefault();
            if (userEntity != null)
            {
                var validatePassword = _authenticateUser.VerifyPassword(model.Password, userEntity.Password);
                if (validatePassword) return _mapper.Map<LoginModel>(userEntity);
            }
            return null;
        }

        private string GenerateToken(LoginModel model)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], null,
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            BaseResponseModel response = new BaseResponseModel();
            var user = AuthenticateUser(model);
            if (user != null)
            {
                var token = GenerateToken(user);

                response.Status = true;
                response.Message = "Login successfully!";
                response.Data = token;
                return Ok(response);
            }

            response.Status = false;
            response.Message = "Login failed!";
            return BadRequest(response);
        }

    }
}
