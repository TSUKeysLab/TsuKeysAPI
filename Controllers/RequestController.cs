﻿using Microsoft.AspNetCore.Authorization;
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
        [HttpPost("createRequest")]
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
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 401)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> getRequests([FromQuery] List<RequestStatus> statuses, int page = 1,int size = 5, RequestSorting sorting = RequestSorting.CreateAsc, string? classroomNumber = null, Guid? timeId = null)
        {

            string token = _tokenHelper.GetTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

            var result = await _requestService.getAllRequestsDTO(statuses, token, page, size,classroomNumber, sorting, timeId);
            return Ok(result);
        }

    }
}
