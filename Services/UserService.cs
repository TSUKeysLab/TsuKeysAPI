using Microsoft.AspNetCore.Identity;
using tsuKeysAPIProject.DBContext.DTO.UserDTO;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.DBContext;
using tsuKeysAPIProject.Services.IServices.IUserService;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.AdditionalServices.Validators;
using tsuKeysAPIProject.DBContext.Models.Enums;
using tsuKeysAPIProject.AdditionalServices.HashPassword;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace tsuKeysAPIProject.Services
{
    public class UserService : IUserService
    {
        private readonly AppDBContext _db;
        private readonly TokenInteraction _tokenHelper;
        private string secretKey;
        private string issuer;
        private string audience;


        public UserService(AppDBContext db, IConfiguration configuration, TokenInteraction tokenHelper)
        {
            _db = db;
            secretKey = configuration.GetValue<string>("AppSettings:Secret");
            issuer = configuration.GetValue<string>("AppSettings:Issuer");
            audience = configuration.GetValue<string>("AppSettings:Audience");
            _tokenHelper = tokenHelper;
        }

        public bool IsUniqueUser(string Email)
        {
            var user = _db.Users.FirstOrDefault(x => x.Email == Email);
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == loginRequestDTO.email);

            if (user == null)
            {
                throw new BadRequestException("Неправильный Email или пароль");
            }
            else if (!HashPassword.VerifyPassword(loginRequestDTO.password, user.Password))
            {
                throw new BadRequestException("Неправильный Email или пароль");
            }

            var token = _tokenHelper.GenerateToken(user);
            LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
            {
                token = token
            };
            return loginResponseDTO;
        }

        public async Task<RegisterResponseDTO> Register(RegisterRequestDTO registerRequestDTO)
        {
            var IsTrueUser = _db.Users.FirstOrDefault(u => registerRequestDTO.Email == u.Email);
            if (IsTrueUser != null)
            {
                throw new BadRequestException("Данный Email уже используется");
            }
            if (!DateOfBirthValidator.ValidateDateOfBirth(registerRequestDTO.BirthDate))
            {
                throw new BadRequestException("Неверная дата рождения. Вам должно быть не менее 13 лет и не более 100 лет.");
            }
            if (!PasswordValidator.ValidatePassword(registerRequestDTO.Password))
            {
                throw new BadRequestException("Пароль не соответсвует требованиям, должна быть минимум одна заглавная буква, семь обычных букв, минимум одна цифра и один спец.символ");
            }
            User user = new User()
            {
                Name = registerRequestDTO.Name,
                Lastname = registerRequestDTO.Lastname,
                BirthDate = registerRequestDTO.BirthDate,
                Gender = registerRequestDTO.Gender,
                Email = registerRequestDTO.Email,
                Role = Roles.User,
                Password = HashPassword.HashingPassword(registerRequestDTO.Password),
                Fullname = registerRequestDTO.Name + " " + registerRequestDTO.Lastname
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = _tokenHelper.GenerateToken(user);

            RegisterResponseDTO registrationResponseDTO = new RegisterResponseDTO()
            {
                token = token
            };

            return registrationResponseDTO;
        }


        public async Task<GetProfileResponseDTO> getProfile(string token)
        {
            string email = _tokenHelper.GetUserEmailFromToken(token);

            if (!string.IsNullOrEmpty(email))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user != null)
                {
                    return new GetProfileResponseDTO
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Lastname = user.Lastname,
                        BirthDate = user.BirthDate,
                        Gender = user.Gender,
                        Role = user.Role,
                        Email = user.Email,
                        Fullname = user.Fullname
                    };
                }
                else
                {
                    throw new NotFoundException("Пользователь не найден");
                }
            }
            else
            {
                throw new UnauthorizedException("Пользователь не авторизован");
            }
        }

        public async Task logout(string token)
        {

            string email = _tokenHelper.GetUserEmailFromToken(token);

            if (!string.IsNullOrEmpty(email))
            {
                await _db.BlackTokens.AddAsync(new BlackToken { Blacktoken = token });
                await _db.SaveChangesAsync();
            }
            else
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }

        }
    }
}
