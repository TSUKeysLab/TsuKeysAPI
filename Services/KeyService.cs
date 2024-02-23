
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Security.Cryptography.Xml;
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
                    Key key = new Key()
                    {
                        Owner = "Dean",
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

        public async Task UpdateKeyRequestStatus(Guid keyRequestId, string token, KeyRequestStatus status)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);

            var request = await _db.KeyRequest.FirstOrDefaultAsync(u => u.KeyRecipient == userEmail && u.Status == KeyRequestStatus.Pending);

            if (request == null)
            {
                throw new NotFoundException("Такой заявки не существует");
            }

            if (request.Status == KeyRequestStatus.Pending)
            {
                request.Status = status;
                await _db.SaveChangesAsync();
            }
            else
            {
                throw new BadRequestException("Статус заявки уже изменить нельзя");
            }
        }

        public async Task DeleteKey(string classroom, string token)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);
            var userRole = await _userInfoHelper.GetUserRole(userEmail);

            if (userRole == Roles.Administrator || userRole == Roles.Dean || userRole == Roles.DeanTeacher)
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

        public async Task<List<KeyInfoDTO>> GetAllKeys(DateOnly? dateOfRequest, Guid? timeId, string token, KeyGettingStatus gettingStatus)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);
            var userRole = await _userInfoHelper.GetUserRole(userEmail);

            if (userRole != Roles.User)
            {
                var allKeys = _db.Keys.ToList();

                var keysForRequest = new List<KeyInfoDTO>();

                allKeys = allKeys.OrderBy(u => u.ClassroomNumber).ToList();

                if ((userRole == Roles.Student || userRole == Roles.Teacher || userRole == Roles.DeanTeacher) && gettingStatus == KeyGettingStatus.AvailableKeys)
                {

                    var timeExist = await _db.TimeSlots.AnyAsync(u => u.Id == timeId);

                    if (!timeExist)
                    {
                        throw new NotFoundException("Такого времени в расписании нет");
                    }

                    var notAvailableClassrooms = await _db.Requests
                    .Where(u => u.TimeId == timeId && u.Status == RequestStatus.Approved && u.DateOfBooking == dateOfRequest)
                    .Select(u => u.ClassroomNumber)
                    .ToListAsync();

                    foreach (var key in allKeys)
                    {
                        if (!notAvailableClassrooms.Contains(key.ClassroomNumber))
                        {
                            var keyInfoDTO = new KeyInfoDTO
                            {
                                ClassroomNumber = key.ClassroomNumber,
                            };
                            keysForRequest.Add(keyInfoDTO);
                        }
                    }
                    return keysForRequest;
                }
                else if ((userRole == Roles.Dean || userRole == Roles.Administrator || userRole == Roles.DeanTeacher) && gettingStatus == KeyGettingStatus.AllKeys)
                {
                    foreach (var key in allKeys)
                    {
                        var keyInfo = new KeyInfoDTO
                        {
                            ClassroomNumber = key.ClassroomNumber,
                            Owner = key.Owner
                        };
                        keysForRequest.Add(keyInfo);
                    }

                    return keysForRequest;
                }
                throw new ForbiddenException("Роль пользователя не подходит");
            }
            else
            {
                throw new ForbiddenException("для отображения ключей нужна роль выше User");
            }
        }

        public async Task SendKeyRequest(KeyRequestsDTO keyRequestDTO, string token)
        {
            var ownerEmail = _tokenHelper.GetUserEmailFromToken(token);

            var ownerRole = await _userInfoHelper.GetUserRole(ownerEmail);

            var recepientRole = await _userInfoHelper.GetUserRole(keyRequestDTO.KeyRecipient);

            if (ownerRole == Roles.Teacher || ownerRole == Roles.Student || ownerRole == Roles.DeanTeacher
                && recepientRole == Roles.Teacher || recepientRole == Roles.Student || recepientRole == Roles.DeanTeacher)
            {
                var keyOwner = _db.Keys
                    .Where(u => u.ClassroomNumber == keyRequestDTO.ClassroomNumber)
                    .Select(u => u.Owner)
                    .FirstOrDefault();

                if (keyOwner == ownerEmail)
                {
                    var keyRequestExist = _db.KeyRequest.Any(u => u.ClassroomNumber == keyRequestDTO.ClassroomNumber
                            && u.KeyRecipient == keyRequestDTO.KeyRecipient
                            && u.Status == KeyRequestStatus.Pending );

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

            if (userRole != Roles.Teacher && userRole != Roles.Student && userRole != Roles.DeanTeacher)
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
                    KeyOwnerEmail = request.KeyOwner,
                    Status = request.Status,
       
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

        public async Task ConfirmReceiptFromUser(Guid requestId, string token)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);
            var userRole = await _userInfoHelper.GetUserRole(userEmail);
            var request = await _db.KeyRequest.FirstOrDefaultAsync(u => u.Id == requestId);
            if (request == null)
            {
                throw new NotFoundException("Заявки на получение не существует");
            }
            if (request.Status == KeyRequestStatus.Rejected)
            {
                throw new BadRequestException("Заявка была отклонена");
            }
            if (request.Status == KeyRequestStatus.Ended)
            {
                throw new BadRequestException("Заявка больше недействительна");
            }
            var key = await _db.Keys.FirstOrDefaultAsync(u => u.ClassroomNumber == request.ClassroomNumber);

            if (key == null)
            {
                throw new NotFoundException("Такого ключа не существует");
            }
                if (userRole != Roles.User && userRole != Roles.Administrator)
                {
                    if (request.KeyRecipient == "Dean" && (userRole == Roles.Dean || userRole == Roles.DeanTeacher))
                    {

                        var allUsersRequest = await _db.KeyRequest
                                .Where(u => u.KeyOwner == key.Owner && u.ClassroomNumber == key.ClassroomNumber)
                                .ToListAsync();

                        allUsersRequest.ForEach(request => request.Status = KeyRequestStatus.Ended);

                        key.Owner = "Dean";
                        await _db.SaveChangesAsync();
                    }
                    else if (request.KeyRecipient == userEmail)
                    {
                        var allUsersRequest = await _db.KeyRequest
                                .Where(u => u.KeyOwner == key.Owner && u.ClassroomNumber == key.ClassroomNumber)
                                .ToListAsync();

                        allUsersRequest.ForEach(request => request.Status = KeyRequestStatus.Ended);

                        key.Owner = userEmail;
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        throw new BadRequestException("Пользователь не имеет отношения к этой заявке");
                    }
                }
                else
                {
                    throw new ForbiddenException("Ключ может получить только пользователь с ролью: Student, Dean, Teacher");
                }
        }

        public async Task ConfirmReceiptFromDean(Guid requestId, string token)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);

            var userId = await _db.Users
                .Where(u => u.Email == userEmail)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            var userRole = await _userInfoHelper.GetUserRole(userEmail);
            var request = await _db.Requests.FirstOrDefaultAsync(u => u.Id == requestId);
            if (request == null)
            {
                throw new NotFoundException("Заявки на бронь не существует");
            }
            if (request.Status == RequestStatus.Rejected)
            {
                throw new BadRequestException("Заявка была отклонена");
            }
            if (request.Status == RequestStatus.Pending)
            {
                throw new BadRequestException("Заявка ещё не одобрена");
            }
            var key = await _db.Keys.FirstOrDefaultAsync(u => u.ClassroomNumber == request.ClassroomNumber);

            if (key == null)
            {
                throw new NotFoundException("Такого ключа не существует");
            }

            if (userRole != Roles.User && userRole != Roles.Administrator && userRole != Roles.Dean)
            {
                if (request.OwnerId == userId)
                {
                    DateTime utcNow = DateTime.UtcNow;
                    TimeOnly currentTime = new TimeOnly(utcNow.Hour, utcNow.Minute, utcNow.Second);
                    DateOnly currentDay = new DateOnly(utcNow.Year, utcNow.Month, utcNow.Day);

                    if (currentDay == request.DateOfBooking && currentTime > request.StartTime && currentTime < request.EndTime)
                    {
                        key.Owner = userEmail;
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        throw new BadRequestException("Пара ещё не началась или уже прошла");
                    }
                }
                else
                {
                    throw new BadRequestException("Пользователь не имеет отношения к этой заявке");
                }
            }
            else
            {
                throw new ForbiddenException("Ключ может получить только пользователь с ролью: Student, DeanTeacher, Teacher");
            }
        }

        public async Task<List<UsersWithoutKeysDTO>> GetUsersWithoutKeys(string token)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);
            var userRole = await _userInfoHelper.GetUserRole(userEmail);

            if (userRole != Roles.User)
            {
                var allUsers = await _db.Users.ToListAsync();
                var allKeysUsers = await _db.Keys
                    .Select(u => u.Owner)
                    .ToListAsync();
                var allUsersWithoutKeys = new List<UsersWithoutKeysDTO>();
                foreach (var user in allUsers)
                {
                    if (!allKeysUsers.Contains(user.Email))
                    {
                        var userWithoutKey = new UsersWithoutKeysDTO();

                        userWithoutKey.UserEmail = user.Email;
                        userWithoutKey.UserFullName = user.Fullname;
                        userWithoutKey.UserRole = user.Role;
                        userWithoutKey.Gender = user.Gender;

                        allUsersWithoutKeys.Add(userWithoutKey);
                    }
                }
                return allUsersWithoutKeys;
            }
            else
            {
                throw new ForbiddenException("Роль пользователя не подходит для этого");
            }
        }
        public async Task DeleteKeyRequest(string token, Guid requestId)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);
            var request = await _db.KeyRequest.FirstOrDefaultAsync(u => u.Id == requestId);
            if (request == null)
            {
                throw new NotFoundException("Такой заявки не существует");
            }

            if (request.Status != KeyRequestStatus.Ended && request.Status != KeyRequestStatus.Rejected)
                {
                    if (request.KeyOwner == userEmail)
                    {
                        _db.KeyRequest.Remove(request);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        throw new ForbiddenException("Удалить заявку может только создатель");
                    }
                }
                else
                {
                    throw new BadRequestException("Заявку уже обработана");
                }
            }
        }
}