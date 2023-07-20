using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RedMango_API.Data;
using RedMango_API.DTO;
using RedMango_API.Models;
using RedMango_API.Utility;

namespace RedMango_API.Controllers
{
    [ApiController]
	[Route("api/auth")]
    public class AuthController:ControllerBase
	{
		private readonly ApplicationDbContext _db;
		protected ApiResponse _response;
		private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private string secretKey;

		public AuthController(ApplicationDbContext db, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager )
		{
			_db = db;
			secretKey = configuration.GetValue<string>("ApiSettings:Secret");
			_response = new();
			_userManager = userManager;
			_roleManager = roleManager;
		}

		[HttpPost("register")]
		public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequestDTO registerRequestDTO)
		{
			ApplicationUser userFromDb = _db.ApplicationUsers
				.FirstOrDefault(u => u.UserName.ToLower() == registerRequestDTO.UserName.ToLower());

			if(userFromDb != null)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages = new() { "Username already exists" };
				return BadRequest(_response);
			}

			ApplicationUser newUser = new()
			{
				UserName = registerRequestDTO.UserName,
				Email = registerRequestDTO.UserName,
				NormalizedEmail = registerRequestDTO.UserName.ToUpper(),
				Name = registerRequestDTO.Name,
			};

			try
			{
                var result = await _userManager.CreateAsync(newUser, registerRequestDTO.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                    {
                        //create roles in database
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));

                    }

                    if (registerRequestDTO.Role.ToLower() == SD.Role_Admin)
                    {
                        await _userManager.AddToRoleAsync(newUser, SD.Role_Admin);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(newUser, SD.Role_Customer);
                    }

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    return Ok(_response);
                }
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = new() { "Error while registering" };
                return BadRequest(_response);
            }
			catch (Exception ex)
			{
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = new() { ex.ToString() };
                return BadRequest(_response);
            }
			

        }


        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse>> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            ApplicationUser userFromDb = _db.ApplicationUsers
                .FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

            if (userFromDb == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = new() { "Username or password is incorrect" };
                return BadRequest(_response);
            }

            bool isValid = await _userManager.CheckPasswordAsync(userFromDb, loginRequestDTO.Password);

            if (isValid == false)
            {
                _response.result = new LoginRequestDTO();
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = new() { "Username or password is incorrect" };
                return BadRequest(_response);
            }

            // generate JWT Token
            var roles = await _userManager.GetRolesAsync(userFromDb);
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.ASCII.GetBytes(secretKey);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("fullName", userFromDb.Name),
                    new Claim("id", userFromDb.Id.ToString()),
                    new Claim(ClaimTypes.Email, userFromDb.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            LoginResponseDTO loginResponseDTO = new()
            {
                Email = userFromDb.Email,
                Token = tokenHandler.WriteToken(token)
            };

            if (loginResponseDTO.Email == null || string.IsNullOrEmpty(loginResponseDTO.Token))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = new() { "Username or password is invalid" };
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.result = loginResponseDTO;
            return Ok(_response);

        }
    }

}

