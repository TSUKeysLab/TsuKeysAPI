
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.AdditionalServices.UserInfoHelper;
using tsuKeysAPIProject.DBContext;
using tsuKeysAPIProject.DBContext.DTO.KeyDTO;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.DBContext.Models.Enums;
using tsuKeysAPIProject.Services.IServices.IKeyService;

namespace tsuKeysAPIProject.Services
{
    public class KeyService : IKeyService
    {

        private readonly AppDBContext _db;
        private readonly TokenInteraction _tokenHelper;
        private readonly UserInfoHelper _userInfoHelper;

        public KeyService(AppDBContext db, TokenInteraction tokenInteraction, UserInfoHelper userInfoHelper)
        {
            _db = db;
            _tokenHelper = tokenInteraction;
            _userInfoHelper = userInfoHelper;
        }

        public async Task CreateKey(CreateKeyDTO createKeyDTO, string token)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);

            var userRole = await _userInfoHelper.GetUserRole(userEmail);

            Console.WriteLine(userRole != Roles.Administrator);

            if (userRole != Roles.Dean || userRole != Roles.Administrator)
            {
                var classroomNumber = await _db.Keys
                    .Where(u => u.ClassroomNumber == createKeyDTO.ClassroomNumber)
                    .Select(u => u.ClassroomNumber)
                    .FirstOrDefaultAsync();

                if (classroomNumber == null)
                {
                    Console.WriteLine(classroomNumber);
                    Key key = new Key()
                    {
                        OwnerEmail = "Dean",
                        ClassroomNumber = createKeyDTO.ClassroomNumber,
                    };

                    await _db.Keys.AddAsync(key);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    throw new BadRequestException("Такой ключ уже существует");
                }
            }
            else
            {
                throw new ForbiddenException("Ключи могут создавать только работники деканата или администраторы");
            }
        }

        public async Task UpdateKeyRequestStatus(string classroomNumber, string token, RequestStatus status)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);

            var request = await _db.KeyRequest.FirstOrDefaultAsync(u => u.KeyRecipient == userEmail && u.Status == RequestStatus.Pending);

            if (request != null && request.Status == RequestStatus.Pending)
            {
                request.Status = status;
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteKey(string classroom, string token)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);
            var userRole = await _userInfoHelper.GetUserRole(userEmail);

            if (userRole == Roles.Administrator || userRole == Roles.Dean) 
            {
                var key = await _db.Keys.FirstOrDefaultAsync(u => u.ClassroomNumber == classroom);

                if (key != null)
                {
                    _db.Keys.Remove(key);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    throw new BadRequestException("Данного ключа не существует");
                }
            }
            else
            {
                throw new ForbiddenException("Удалять ключи могут только работники деканата или администрация");
            }

        }

        public async Task GetAllKeys()
        {
            throw new NotImplementedException();
        }

        public async Task SendKeyRequest(KeyRequestsDTO keyRequestDTO, string token)
        {
            var ownerEmail = _tokenHelper.GetUserEmailFromToken(token);

            var ownerRole = await _userInfoHelper.GetUserRole(ownerEmail);

            var recepientRole = await _userInfoHelper.GetUserRole(keyRequestDTO.KeyRecipient);

            if (ownerRole == Roles.Teacher || ownerRole == Roles.Student
                && recepientRole == Roles.Teacher || recepientRole == Roles.Student)
            {      
                var keyOwner = _db.Keys
                    .Where(u => u.OwnerEmail == ownerEmail)
                    .Select(u => u.OwnerEmail)
                    .FirstOrDefault();

                if (keyOwner == ownerEmail)
                {
                    var keyRequestExist = _db.KeyRequest.Any(u => u.KeyOwner == ownerEmail
                            && u.KeyRecipient == keyRequestDTO.KeyRecipient
                            && u.Status == RequestStatus.Pending);

                    if (!keyRequestExist)
                    {
                        KeyRequest request = new KeyRequest()
                        {
                            KeyOwner = ownerEmail,
                            KeyRecipient = keyRequestDTO.KeyRecipient,
                            ClassroomNumber = keyRequestDTO.ClassroomNumber
                        };
                        await _db.AddAsync(request);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        throw new BadRequestException("Такая заявка уже существует");
                    }
                }
                else
                {
                    throw new ForbiddenException("У данного пользователя сейчас нет ключа");
                }   
            }
            else
            {
                throw new ForbiddenException("Ключ может передать только студент или преподаватель");
            }    
        }

        public async Task<List<KeyRequestResponseDTO>> GetAllRequests(RequestUserStatus userStatus, string token)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);
            var userRole = await _userInfoHelper.GetUserRole(userEmail);

            if (userRole != Roles.Teacher && userRole != Roles.Student)
            {
                throw new ForbiddenException("У вас нет прав для передачи ключа");
            }

            List<KeyRequest> userRequests;
            if (userStatus == RequestUserStatus.Owner)
            {
                userRequests = await _db.KeyRequest.Where(u => u.KeyOwner == userEmail).ToListAsync();
            }
            else if (userStatus == RequestUserStatus.Recipient)
            {
                userRequests = await _db.KeyRequest.Where(u => u.KeyRecipient == userEmail).ToListAsync();
            }
            else
            {
                throw new ForbiddenException("Вы не имеете отношения к данной заявке");
            }

            var requestsDto = new List<KeyRequestResponseDTO>();
            foreach (var request in userRequests)
            {
                var requestDto = new KeyRequestResponseDTO
                {
                    ClassroomNumber = request.ClassroomNumber,
                    KeyRecipientEmail = request.KeyRecipient,
                    KeyOwnerEmail = request.KeyOwner
                };

                if (userStatus == RequestUserStatus.Owner)
                {
                    var keyRecipient = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.KeyRecipient);
                    requestDto.KeyRecipientFullName = keyRecipient?.Fullname;
                }
                else if (userStatus == RequestUserStatus.Recipient)
                {
                    var keyOwner = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.KeyOwner);
                    requestDto.KeyOwnerFullName = keyOwner?.Fullname;
                }

                requestsDto.Add(requestDto);
            }

            return requestsDto;
        }

        public async Task ConfirmReceipt(string classroomNumber, string token)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);
            var userRole =  await _userInfoHelper.GetUserRole(userEmail);

            var key = await _db.Keys.FirstOrDefaultAsync(u => u.ClassroomNumber == classroomNumber);
            if (key != null)
            {
                if (userRole != Roles.User && userRole != Roles.Administrator)
                {
                    if (key.OwnerEmail != userEmail)
                    {
                        if (userRole == Roles.Dean)
                        {
                            key.OwnerEmail = "Dean";
                        }
                        else
                        {
                            key.OwnerEmail = userEmail;
                        }
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        throw new BadRequestException("Ключ уже у этого пользователя");
                    }
                }
                else
                {
                    throw new ForbiddenException("Ключ может получить только пользователь с ролью: Student, Dean, Teacher");
                }
            }
            else
            {
                throw new BadRequestException("Такого ключе не существует");
            }
        }
    }
}