using tsuKeysAPIProject.DBContext.DTO.ScheduleDTO;

namespace tsuKeysAPIProject.Services.IServices.IScheduleService
{
    public interface IScheduleService
    {
        public Task<List<GetScheduleDTO>> getAllSchedule();
    }
}
