using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.DBContext.DTO.KeyDTO;
using tsuKeysAPIProject.DBContext.DTO.UserDTO;
using tsuKeysAPIProject.DBContext.Models;
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
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> CreateKey([FromBody] CreateKeyDTO createKeyDTO)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (token == null)
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            await _keyService.CreateKey(createKeyDTO);
            return Ok();
        }

        [HttpDelete("delete/{classroom}")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> DeleteKey(string classroom)
        {
            await _keyService.DeleteKey(classroom);
            return Ok();
        }

        [HttpPost("request")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> SendKeyRequest(KeyRequestDTO keyRequestDTO)
        {
            await _keyService.SendKeyRequest(keyRequestDTO);
            return Ok();
        }

        [HttpPost("accept/{classroom}")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> AcceptKeyRequest(string classroom)
        {
            await _keyService.AcceptKeyRequest(classroom);
            return Ok();
        }
        [HttpPost("decline/{classroom}")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> DeclineKeyRequest(string classroom)
        {
            await _keyService.DeclineKeyRequest(classroom);
            return Ok();
        }

        [HttpGet("")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(typeof(AllKeysDTO), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> GetAllKeys()
        {
            await _keyService.GetAllKeys();
            return Ok();
        }

        [HttpGet("requests")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(typeof(AllKeysDTO), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> GetAllRequests()
        {
            await _keyService.GetAllKeys();
            return Ok();
        }

        [HttpPost("confirm/{classroom}")]
        [Authorize(Policy = "TokenNotInBlackList")]
        [ProducesResponseType(typeof(AllKeysDTO), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> ConfirmGetting(string classroom)
        {
            await _keyService.GetAllKeys();
            return Ok();
        }


    }
}
