

using Microsoft.EntityFrameworkCore;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.DBContext;
using tsuKeysAPIProject.DBContext.DTO.ScheduleDTO;
using tsuKeysAPIProject.Services.IServices.IScheduleService;

namespace tsuKeysAPIProject.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly AppDBContext _db;
        private readonly TokenInteraction _tokenHelper;

        public ScheduleService(AppDBContext db, IConfiguration configuration, TokenInteraction tokenHelper)
        {
            _db = db;
            _tokenHelper = tokenHelper;
        }

        public async Task<List<GetScheduleDTO>> getAllSchedule()
        {
            var schedule = await _db.TimeSlots
                        .Select(ts => new GetScheduleDTO
                        {
                            Id = ts.SlotNumber,
                            StartTime = ts.StartTime,
                            EndTime = ts.EndTime
                        })
                        .ToListAsync();

            return schedule;
        }
    }
}
