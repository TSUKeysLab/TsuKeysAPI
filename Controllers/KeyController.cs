using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.DBContext.DTO.KeyDTO;
using tsuKeysAPIProject.DBContext.DTO.UserDTO;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.DBContext.Models.Enums;
using tsuKeysAPIProject.Services.IServices.IKeyService;

namespace tsuKeysAPIProject.Controllers
{ 
    [Route("/key")]
    public class KeyController: Controller
    {
        private readonly IKeyService _keyService;
        private readonly TokenInteraction _tokenHelper;
        
        public KeyController(IKeyService keyService, TokenInteraction tokenInteraction)
        {
            _keyService = keyService;
            _tokenHelper = tokenInteraction;
        }

        [HttpPost("create")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> CreateKey([FromBody] CreateKeyDTO createKeyDTO)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            await _keyService.CreateKey(createKeyDTO, token);
            return Ok();
        }


        

        [HttpPost("request")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> SendKeyRequest([FromBody] KeyRequestsDTO keyRequestDTO)
        {
            string token = _tokenHelper.GetTokenFromHeader();

            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            await _keyService.SendKeyRequest(keyRequestDTO, token);
            return Ok();
        }

        [HttpGet("")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(typeof(List<KeyInfoDTO>), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> GetAllKeys([FromBody]RequestForAllKeysDTO requestDto)
        {
            string token = _tokenHelper.GetTokenFromHeader();

            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            var allKeys = await _keyService.GetAllKeys(requestDto, token);
            return Ok(allKeys);
        }

        [HttpGet("users")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(typeof(List<UsersWithoutKeysDTO>), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> GetUsersWithoutKeys(string classroom)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            var usersWithoutKeys = await _keyService.GetUsersWithoutKeys(token);
            return Ok(usersWithoutKeys);
        }

        [HttpGet("requests")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(typeof(List<KeyRequestResponseDTO>), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> GetAllRequests([FromQuery][Required] RequestUserStatus userStatus)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            var keyRequests = await _keyService.GetAllRequests(userStatus, token);
            return Ok(keyRequests);
        }

        [HttpPut("accept/{classroom}")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> AcceptKeyRequest(string classroom)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            await _keyService.UpdateKeyRequestStatus(classroom, token, RequestStatus.Approved);
            return Ok();
        }
        [HttpPut("decline/{classroom}")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> DeclineKeyRequest(string classroom)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            await _keyService.UpdateKeyRequestStatus(classroom, token, RequestStatus.Rejected);
            return Ok();
        }

        [HttpPut("confirm/{classroom}")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> ConfirmGetting(string classroom)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            await _keyService.ConfirmReceipt(classroom, token);
            return Ok();
        }

        [HttpDelete("delete/{classroom}")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> DeleteKey(string classroom)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            await _keyService.DeleteKey(classroom, token);
            return Ok();
        }

    }
}
