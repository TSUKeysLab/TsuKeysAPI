using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.DBContext.DTO.UserDTO;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.DBContext;
using tsuKeysAPIProject.Services.IServices.IUserService;
using tsuKeysAPIProject.DBContext.DTO.RolesDTO;
using tsuKeysAPIProject.Services.IServices.IRolesService;
using tsuKeysAPIProject.DBContext.Models.Enums;

namespace tsuKeysAPIProject.Controllers
{
    [Route("/")]
    public class RolesController : Controller
    {
        private readonly IRoleService _rolesService;
        private readonly TokenInteraction _tokenHelper;
        private readonly AppDBContext _db;

        public RolesController(IRoleService rolesService, AppDBContext db, TokenInteraction tokenHelper)
        {
            _rolesService = rolesService;
            _db = db;
            _tokenHelper = tokenHelper;
        }

        [Authorize(Policy = "TokenNotInBlackList")]
        [HttpPut("GrantRole")]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> grantRole([FromQuery] GrantRoleRequestDTO grantRoleRequestDTO)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
            await _rolesService.grantRole(grantRoleRequestDTO,token);
            return Ok();
        }
        [HttpGet("GetUserInformation")]
        [ProducesResponseType(typeof(GetUsersPageDTO), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> getUserInformation([FromQuery] string? fullname, Roles? role, int size=5, int page=1)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
            return Ok(await _rolesService.getUsersInformation(token,fullname,role,size,page));
        }
    }
}
