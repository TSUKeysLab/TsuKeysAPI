using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.DBContext.DTO.RolesDTO;
using tsuKeysAPIProject.DBContext.DTO.ScheduleDTO;
using tsuKeysAPIProject.DBContext.DTO.UserDTO;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.Services.IServices.IKeyService;
using tsuKeysAPIProject.Services.IServices.IScheduleService;

namespace tsuKeysAPIProject.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;
        private readonly TokenInteraction _tokenHelper;

        public ScheduleController(IScheduleService serviceService, TokenInteraction tokenInteraction)
        {
            _scheduleService = serviceService;
            _tokenHelper = tokenInteraction;
        }

        [HttpGet("getSchedule")]
        [ProducesResponseType(typeof(List<GetScheduleDTO>),200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(Error), 500)]
        public async Task<IActionResult> getSchedule()
        {
            return Ok(await _scheduleService.getAllSchedule());
        }
    }
}
