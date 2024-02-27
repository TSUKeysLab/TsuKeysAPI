using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.DBContext;
using tsuKeysAPIProject.DBContext.DTO.RequestDTO;
using tsuKeysAPIProject.DBContext.DTO.RolesDTO;
using tsuKeysAPIProject.DBContext.DTO.UserDTO;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.DBContext.Models.Enums;
using tsuKeysAPIProject.Services.IServices.IRequestService;
using tsuKeysAPIProject.Services.IServices.IUserService;

namespace tsuKeysAPIProject.Controllers
{
    [Route("/request")]
    public class RequestController : Controller
    {

        private readonly IRequestService _requestService;
        private readonly TokenInteraction _tokenHelper;
        private readonly AppDBContext _db;

        public RequestController(IRequestService requestService, AppDBContext db, TokenInteraction tokenHelper)
        {
            _requestService = requestService;
            _db = db;
            _tokenHelper = tokenHelper;
        }


        [Authorize(Policy = "TokenNotInBlackList")]
        [HttpPost("create")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> createRequest([FromBody] CreateRequestDTO createRequestDTO)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
            await _requestService.createRequest(createRequestDTO,token);
            return Ok();

        }
        [Authorize(Policy = "TokenNotInBlackList")]
        [HttpGet("getRequests")]
        [ProducesResponseType(typeof(GetRequestsPageDTO), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> getRequests([FromQuery] List<RequestStatus> statuses, int page = 1,int size = 5, RequestSorting sorting = RequestSorting.CreateAsc, string? classroomNumber = null, int? timeId = null)
        {

            string token = _tokenHelper.GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            var result = await _requestService.getAllRequestsDTO(statuses, token, page, size,classroomNumber, sorting, timeId);
            return Ok(result);
        }

        [Authorize(Policy = "TokenNotInBlackList")]
        [HttpGet("getMyRequests")]
        [ProducesResponseType(typeof(GetRequestsPageDTO), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> getAllUsersRequests([FromQuery] List<RequestStatus> statuses, int page = 1)
        {

            string token = _tokenHelper.GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            var result = await _requestService.getAllUsersRequests(statuses, token, page);
            return Ok(result);
        }

        [Authorize(Policy = "TokenNotInBlackList")]
        [HttpPut("approve")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> approveRequest([FromQuery] ApproveRequestDTO approveRequestDTO)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
            await _requestService.approveRequest(approveRequestDTO, token);
            return Ok();
        }

        [Authorize(Policy = "TokenNotInBlackList")]
        [HttpPut("reject")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> rejectRequest([FromQuery] RejectRequestDTO rejectRequestDTO)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
            await _requestService.rejectRequest(rejectRequestDTO, token);
            return Ok();

        }

        [Authorize(Policy = "TokenNotInBlackList")]
        [HttpDelete("delete")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> deleteRequest([FromQuery] DeleteRequestDTO deleteRequestDTO)
        {
            string token = _tokenHelper.GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
            await _requestService.deleteRequest(deleteRequestDTO, token);
            return Ok();

        }

    }
}
