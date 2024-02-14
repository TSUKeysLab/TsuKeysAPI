using tsuKeysAPIProject.DBContext.DTO.UserDTO;

namespace tsuKeysAPIProject.Services.IServices.IUserService
{
    public interface IUserService
    {
        bool IsUniqueUser(string email);
        public Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
        public Task<RegisterResponseDTO> Register(RegisterRequestDTO registrationRequestDTO);
        public Task<GetProfileResponseDTO> getProfile(string token);
        public Task logout(string token);
    }
}
