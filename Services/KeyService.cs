
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.AdditionalServices.UserInfoHelper;
using tsuKeysAPIProject.DBContext;
using tsuKeysAPIProject.DBContext.DTO.KeyDTO;
using tsuKeysAPIProject.DBContext.DTO.RolesDTO;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.DBContext.Models.Enums;
using tsuKeysAPIProject.Migrations;
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

            string pattern = @"^\d{1,4}[a-zA-Z]{0,2}$";
            bool isMatch = Regex.IsMatch(createKeyDTO.ClassroomNumber, pattern);
            if (!isMatch)
            {
                throw new BadRequestException("Номер ключа должен состоять из 1-4 цифр и 0-2 букв");
            }

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

            var userRole = await _userInfoHelper.GetUserRole(userEmail);

            var request = await _db.KeyRequest.FirstOrDefaultAsync(u => 
            (u.KeyRecipient == userEmail || (u.KeyRecipient == "Dean" && (userRole == Roles.Dean || userRole == Roles.DeanTeacher || userRole == Roles.Administrator)))
            && u.Status == KeyRequestStatus.Pending && u.Id == keyRequestId);    

            if (request == null)
            {
                throw new NotFoundException("Такой заявки не существует");
            }

            if (status == KeyRequestStatus.Rejected && request.KeyRecipient == "Dean")
            {
                throw new BadRequestException("Деканат не может отклонить принятие ключа");
            }

            if (userRole != Roles.User)
            {
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
            else
            {
                throw new ForbiddenException("СЛЫШ ТЕБЕ СЮДА НЕЛЬЗЯ");
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

        public async Task<List<KeyInfoDTO>> GetAvailableKeys(DateOnly dateOfRequest, int timeId, string token)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);
            var userRole = await _userInfoHelper.GetUserRole(userEmail);


            if ((userRole == Roles.Student || userRole == Roles.Teacher || userRole == Roles.DeanTeacher))
                {

                DateTime utcNow = DateTime.UtcNow;
                DateTime nowTomsk = utcNow.AddHours(7);
                TimeOnly timeOnly = new TimeOnly(nowTomsk.Hour, nowTomsk.Minute, nowTomsk.Second);
                DateOnly dateOnly = DateOnly.FromDateTime(nowTomsk);

                var endTime = await _db.TimeSlots
                    .Where(u => u.SlotNumber == timeId)
                    .Select(u => u.EndTime)
                    .FirstOrDefaultAsync();

                if ((dateOfRequest < dateOnly || (timeOnly > endTime && dateOfRequest == dateOnly)))
                {
                    throw new BadRequestException("Нельзя сделать заявку в прошлое");
                }

                var userId = await _db.Users
                    .Where(u => u.Email == userEmail)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();
                
                var allKeys = _db.Keys.ToList();

                    var keysForRequest = new List<KeyInfoDTO>();

                    allKeys = allKeys.OrderBy(u => u.ClassroomNumber).ToList();

                    var timeExist = await _db.TimeSlots.AnyAsync(u => u.SlotNumber == timeId);

                    if (!timeExist)
                    {
                        throw new NotFoundException("Такого времени в расписании нет");
                    }

                    var notAvailableClassrooms = await _db.Requests
                    .Where(u => u.TimeId == timeId && u.Status == RequestStatus.Approved && u.DateOfBooking == dateOfRequest
                        || (u.OwnerId == userId && u.TimeId == timeId && u.DateOfBooking == dateOfRequest))
                    .Select(u => u.ClassroomNumber)
                    .ToListAsync();

                    foreach (var key in allKeys)
                    {
                        if (!notAvailableClassrooms.Contains(key.ClassroomNumber))
                        {
                            var keyInfoDTO = new KeyInfoDTO
                            {
                                Owner = "Dean",
                                ClassroomNumber = key.ClassroomNumber,
                            };
                            keysForRequest.Add(keyInfoDTO);
                        }
                    }

                List<KeyInfoDTO> sortedKeys = keysForRequest.OrderBy(x =>
                {
                    string numStr = new string(x.ClassroomNumber.TakeWhile(char.IsDigit).ToArray());
                    return int.Parse(numStr);
                }).ToList();

                return sortedKeys;
                }
                else
                {
                    throw new ForbiddenException("Ваша роль не подходит");
                }
            }

        public async Task<List<KeyInfoDTO>> GetAllKeys(string token, bool owned, string classroomNumber)
        {

            var userEmail = _tokenHelper.GetUserEmailFromToken(token);

            var userRole = await _userInfoHelper.GetUserRole(userEmail);

            var allKeys = _db.Keys.AsQueryable();

            var keysForRequest = new List<KeyInfoDTO>();

            if ((userRole == Roles.Dean || userRole == Roles.Administrator || userRole == Roles.DeanTeacher))
            {
                if (!string.IsNullOrEmpty(classroomNumber))
                {
                    allKeys = allKeys.Where(key => key.ClassroomNumber.ToLower().Contains(classroomNumber.ToLower()));
                }

                if (owned)
                {
                    allKeys = allKeys.Where(key => key.Owner != "Dean");
                }

                foreach (var key in allKeys)
                {
                    var keyInfo = new KeyInfoDTO
                    {
                        ClassroomNumber = key.ClassroomNumber,
                        Owner = key.Owner
                    };
                    keysForRequest.Add(keyInfo);
                }

                List<KeyInfoDTO> sortedKeys = keysForRequest.OrderBy(x =>
                {
                    string numStr = new string(x.ClassroomNumber.TakeWhile(char.IsDigit).ToArray());
                    return int.Parse(numStr);
                }).ToList();

                return sortedKeys;
            }
            throw new ForbiddenException("Роль пользователя не подходит");
        }

        public async Task SendKeyRequest(KeyRequestsDTO keyRequestDTO, string token)
        {
            var ownerEmail = _tokenHelper.GetUserEmailFromToken(token);

            var ownerRole = await _userInfoHelper.GetUserRole(ownerEmail);

            var recepientRole = await _userInfoHelper.GetUserRole(keyRequestDTO.KeyRecipient);

            if ((ownerRole == Roles.Teacher || ownerRole == Roles.Student || ownerRole == Roles.DeanTeacher)
                && (recepientRole == Roles.Teacher || recepientRole == Roles.Student
                || recepientRole == Roles.DeanTeacher || keyRequestDTO.KeyRecipient == "Dean"))
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
                        DateTime utcNow = DateTime.UtcNow;
                        DateTime nowTomsk = utcNow.AddHours(7);
                        
                        TimeOnly timeOnly = new TimeOnly(nowTomsk.Hour, nowTomsk.Minute, nowTomsk.Second);


                        var endOfClass = await _db.TimeSlots
                            .Where(u => u.StartTime < timeOnly && u.EndTime > timeOnly)
                            .Select(u => u.EndTime)
                            .FirstOrDefaultAsync();

                        KeyRequest request = new KeyRequest()
                        {
                            KeyOwner = ownerEmail,
                            KeyRecipient = keyRequestDTO.KeyRecipient,
                            ClassroomNumber = keyRequestDTO.ClassroomNumber,
                            Status = KeyRequestStatus.Pending,
                            EndOfRequest = endOfClass
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
                throw new ForbiddenException("Ключ может передать только студент или преподаватель или преподаватель из деканата");
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
                userRequests = await _db.KeyRequest
                    .Where(u => u.KeyOwner == userEmail
                    && (u.Status == KeyRequestStatus.Pending || u.Status == KeyRequestStatus.Approved || u.Status == KeyRequestStatus.Rejected))
                    .ToListAsync();
            }
            else if (userStatus == RequestUserStatus.Recipient)
            {
                userRequests = await _db.KeyRequest
                    .Where(u => u.KeyRecipient == userEmail
                    && (u.Status == KeyRequestStatus.Pending || u.Status == KeyRequestStatus.Approved || u.Status == KeyRequestStatus.Rejected))
                    .ToListAsync();
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
                    RequestId = request.Id,
                    ClassroomNumber = request.ClassroomNumber,
                    KeyRecipientEmail = request.KeyRecipient,
                    KeyOwnerEmail = request.KeyOwner,
                    Status = request.Status,
                    EndOfRequest = request.EndOfRequest 
                };

                if (userStatus == RequestUserStatus.Owner)
                {
                    var keyRecipient = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.KeyRecipient);
                    requestDto.KeyRecipientFullName = request.KeyRecipient == "Dean" ? "Деканат" : keyRecipient?.Fullname; 
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

        public async Task<List<KeyRequestResponseDTO>> GetDeanRequests(string token)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);
            var userRole = await _userInfoHelper.GetUserRole(userEmail);

            if (userRole != Roles.Dean && userRole != Roles.DeanTeacher && userRole != Roles.Administrator)
            {
                throw new ForbiddenException("У вас нет прав для принятия ключа");
            }

            List<KeyRequest> userRequests;
            
                userRequests = await _db.KeyRequest
                    .Where(u => u.KeyRecipient == "Dean"
                    && (u.Status == KeyRequestStatus.Pending || u.Status == KeyRequestStatus.Approved || u.Status == KeyRequestStatus.Rejected))
                    .ToListAsync();
            
            var requestsDto = new List<KeyRequestResponseDTO>();
            foreach (var request in userRequests)
            {
                var requestDto = new KeyRequestResponseDTO
                {
                    RequestId = request.Id,
                    ClassroomNumber = request.ClassroomNumber,
                    KeyRecipientFullName = "Деканат",
                    KeyRecipientEmail = request.KeyRecipient,
                    KeyOwnerEmail = request.KeyOwner,
                    Status = request.Status,
                    EndOfRequest = request.EndOfRequest
                };

                    var keyOwner = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.KeyOwner);
                    requestDto.KeyOwnerFullName = keyOwner?.Fullname;
            
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
                if (userRole != Roles.User)
                {
                    if (request.KeyRecipient == "Dean" && (userRole == Roles.Dean || userRole == Roles.DeanTeacher || userRole == Roles.Administrator))
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
                    throw new ForbiddenException("Ключ может получить только пользователь с ролью: Student, Dean, DeanTeacher, Teacher");
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
                    DateTime now = DateTime.UtcNow;
                    DateTime nowTomsk = now.AddHours(7);
                    TimeOnly currentTime = new TimeOnly(nowTomsk.Hour, nowTomsk.Minute, nowTomsk.Second);
                    DateOnly currentDay = new DateOnly(nowTomsk.Year, nowTomsk.Month, nowTomsk.Day);
                    TimeOnly timeForKeyTaking = currentTime.AddMinutes(7);

                    if (currentDay == request.DateOfBooking && timeForKeyTaking >= request.StartTime && currentTime < request.EndTime)
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
        public async Task DeleteKeyRequest(string token, Guid requestId)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);
            var request = await _db.KeyRequest.FirstOrDefaultAsync(u => u.Id == requestId);
            if (request == null)
            {
                throw new NotFoundException("Такой заявки не существует");
            }

            if (request.Status != KeyRequestStatus.Ended)
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

        public async Task<List<UserKeysDTO>> GetUserKeys(string token)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);

            DateTime now = DateTime.UtcNow;
            DateTime nowTomsk = now.AddHours(20);
            TimeOnly currentTime = new TimeOnly(nowTomsk.Hour, nowTomsk.Minute, nowTomsk.Second);
            Console.WriteLine(currentTime.ToString());
            var endOfClassTime = await _db.TimeSlots
                .Where(u => u.StartTime <= currentTime && u.EndTime >= currentTime)
                .Select(u => u.EndTime)
                .FirstOrDefaultAsync();
            
            var userKeys = await _db.Keys
                .Where(u => u.Owner == userEmail)
                .ToListAsync();

            var userKeysDTO = new List<UserKeysDTO>();

            userKeys.ForEach(u => { 
                UserKeysDTO key = new UserKeysDTO();
                key.ClassroomNumber = u.ClassroomNumber;
                key.TimeToEndUsage = endOfClassTime;

                userKeysDTO.Add(key);
            });


            return userKeysDTO; 
        }

        public async Task<UsersForTransferDTO> GetUsersForTransfer(string token, string fullName)
        {
            
            string userEmail = _tokenHelper.GetUserEmailFromToken(token);
            var userRole = await _userInfoHelper.GetUserRole(userEmail);
            var allUsers = _db.Users
                .Where(u => u.Email != userEmail
                    && u.Role != Roles.Administrator && u.Role != Roles.User && u.Role != Roles.Dean)
                .AsQueryable();

            if (userRole != Roles.User && userRole != Roles.Dean && userRole != Roles.Administrator)
                {
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        if (!string.IsNullOrEmpty(fullName))
                        {
                            allUsers = allUsers.Where(aU => aU.Fullname.ToLower().Contains(fullName.ToLower())
                            && aU.Role != Roles.Administrator && aU.Role != Roles.User && aU.Role != Roles.Dean);
                        }

                        var usersDto = new UsersForTransferDTO
                        {
                            Users = allUsers.Select(u => new GetUserInformationResponseDTO
                            {
                                UserId = u.Id,
                                Fullname = u.Fullname,
                                Role = u.Role,
                                Email = u.Email
                            }),
                        };

                        return usersDto;
                    }
                    else
                    {
                        throw new BadRequestException("Вам недоступна данная функция");
                    
                    }
                }
                else
                {
                    throw new ForbiddenException("Вам недоступна данная функция");
                }
        }
    }
}