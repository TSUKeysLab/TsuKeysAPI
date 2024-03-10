using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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

        [HttpGet("available")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(typeof(List<KeyInfoDTO>), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> GetAvailableKeys([FromQuery][Required] int year, [FromQuery][Required] int month, [FromQuery][Required] int day, [FromQuery][Required] int timeId)
        {
            string token = _tokenHelper.GetTokenFromHeader();

            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            DateOnly dateOfRequest = new DateOnly(2010, 10, 10);
                try
                {
                    dateOfRequest = new DateOnly(year, month, day);
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new BadRequestException("Неверный формат даты");
                }
            

            var allKeys = await _keyService.GetAvailableKeys(dateOfRequest, timeId, token);
            return Ok(allKeys);
        }

        [HttpGet("")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(typeof(List<KeyInfoDTO>), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> GetAllKeys([FromQuery] bool owned, [FromQuery] string classroomNumber)
        {
            string token = _tokenHelper.GetTokenFromHeader();

            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
            
            var allKeys = await _keyService.GetAllKeys(token, owned, classroomNumber);
            return Ok(allKeys);
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
        [HttpGet("requests/dean")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(typeof(List<KeyRequestResponseDTO>), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> GetDeanRequests()
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            var keyRequests = await _keyService.GetDeanRequests(token);
            return Ok(keyRequests);
        }

        [HttpGet("owned")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(typeof(List<UserKeysDTO>), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> GetUserKeys()
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
    
            var userKeys = await _keyService.GetUserKeys(token);
            return Ok(userKeys);
        }

        [HttpGet("request/users")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(typeof(List<UsersForTransferDTO>), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> GetUsersForTransfer([FromQuery] string fullName)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
            var usersForTransfer = await _keyService.GetUsersForTransfer(token, fullName);
            return Ok(usersForTransfer);
        }

        [HttpPut("accept/request/{id}")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> AcceptKeyRequest(Guid id)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            await _keyService.UpdateKeyRequestStatus(id, token, KeyRequestStatus.Approved);
            return Ok();
        }
        [HttpPut("decline/request/{id}")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> DeclineKeyRequest(Guid id)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            await _keyService.UpdateKeyRequestStatus(id, token, KeyRequestStatus.Rejected);
            return Ok();
        }

        [HttpPut("confirm/getting/{request}")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> ConfirmGettingFromUser(Guid request)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            await _keyService.ConfirmReceiptFromUser(request, token);
            return Ok();
        }

        [HttpPut("confirm/dean/{request}")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> ConfirmGettingFromDean(Guid request)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            await _keyService.ConfirmReceiptFromDean(request, token);
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

        [HttpDelete("delete/request/{request}")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 403)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> DeleteRequest(Guid request)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            await _keyService.DeleteKeyRequest(token, request);
            return Ok();
        }
    }
}
