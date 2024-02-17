using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.DBContext.DTO.UserDTO;
using tsuKeysAPIProject.DBContext;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.Services;
using tsuKeysAPIProject.Services.IServices.IUserService;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.Services.IServices.IRolesService;

namespace tsuKeysAPIProject.Controllers
{
    [Route("/")]
    public class UserController : Controller
    {
        private readonly IUserService _userRepo;
        private readonly TokenInteraction _tokenHelper;
        private readonly AppDBContext _db;

        public UserController(IUserService userRepo, AppDBContext db, TokenInteraction tokenHelper)
        {
            _userRepo = userRepo;
            _db = db;
            _tokenHelper = tokenHelper;
        }
        [HttpPost("Login")]
        [ProducesResponseType(typeof(LoginResponseDTO), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var loginResponse = await _userRepo.Login(loginRequestDTO);
            var user = _db.Users.FirstOrDefault(x => x.Email == loginRequestDTO.email);
            return Ok(new { token = loginResponse.token });
        }



        [HttpPost("Register")]
        [ProducesResponseType(typeof(RegisterResponseDTO), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO registerRequestDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var registrationResponse = await _userRepo.Register(registerRequestDTO);
            return Ok(new { token = registrationResponse.token });

        }

        [Authorize(Policy = "TokenNotInBlackList")]
        [HttpGet("GetProfile")]
        [ProducesResponseType(typeof(GetProfileResponseDTO), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> GetProfile()
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
            return Ok(await _userRepo.getProfile(token));
        }

        [Authorize(Policy = "TokenNotInBlackList")]
        [HttpGet("Logout")]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> Logout()
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
            await _userRepo.logout(token);
            return Ok();

        }

        //TODO сделать получение всех пользователей с параметрами (поиск по имени и фильтрация по роли)

    }
}
