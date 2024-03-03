using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.DBContext;
using tsuKeysAPIProject.DBContext.DTO.RequestDTO;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.DBContext.Models.Enums;
using tsuKeysAPIProject.Services.IServices.IRequestService;

namespace tsuKeysAPIProject.Services
{
    public class RequestService : IRequestService
    {
        private readonly AppDBContext _db;
        private readonly TokenInteraction _tokenHelper;

        public RequestService(AppDBContext db, IConfiguration configuration, TokenInteraction tokenHelper)
        {
            _db = db;
            _tokenHelper = tokenHelper;
        }


        public async Task createRequest(CreateRequestDTO createRequestDTO, string token)
        {
            string email = _tokenHelper.GetUserEmailFromToken(token);

            if (!string.IsNullOrEmpty(email))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user.Role == Roles.User)
                {
                    throw new ForbiddenException("У вас не хватает прав, чтобы создавать заявки, дождитесь получения роли!");
                }
                var requestTime = await _db.TimeSlots.FirstOrDefaultAsync(rt => rt.SlotNumber == createRequestDTO.TimeId);
                var key = await _db.Keys.FirstOrDefaultAsync(k => k.ClassroomNumber == createRequestDTO.ClassroomNumber);

                DateTime utcNow = DateTime.UtcNow;
                DateTime utcNowTomsk = utcNow.AddHours(7);
                TimeOnly currentTime = new TimeOnly(utcNowTomsk.Hour, utcNowTomsk.Minute, utcNowTomsk.Second);
                DateOnly currentDay = new DateOnly(utcNowTomsk.Year, utcNowTomsk.Month, utcNowTomsk.Day);

                if (requestTime == null)
                {
                    throw new NotFoundException("Выбранное вами время не существует");
                }
                if (key == null)
                {
                    throw new NotFoundException("Выбранной вами аудитории не существует");
                }
                if (requestTime.StartTime <= currentTime && createRequestDTO.DateOfBooking == currentDay && key.Owner != "Dean")
                {
                    throw new NotFoundException("Данная пара уже началась, вы не можете ее забронировать");
                }

                if (requestTime.EndTime <= currentTime && createRequestDTO.DateOfBooking <= currentDay)
                {
                    throw new NotFoundException("Данная пара уже закончилась, вы не можете ее забронировать");
                }

                List<Request> requestsTeacher = await _db.Requests.Where(r => 
                r.Status == RequestStatus.Approved 
                && user.Id == r.OwnerId 
                && r.ClassroomNumber == createRequestDTO.ClassroomNumber && r.TimeId == createRequestDTO.TimeId && r.DateOfBooking.DayOfWeek == createRequestDTO.DateOfBooking.DayOfWeek).ToListAsync();
                List<Request> requestsAll = await _db.Requests.Where(rA => rA.Status == RequestStatus.Approved && rA.ClassroomNumber == createRequestDTO.ClassroomNumber && rA.TimeId == createRequestDTO.TimeId && rA.DateOfBooking == createRequestDTO.DateOfBooking).ToListAsync();
                List<Request> userRequests = await _db.Requests.Where(uR => uR.OwnerId == user.Id && uR.ClassroomNumber == createRequestDTO.ClassroomNumber && uR.TimeId == createRequestDTO.TimeId && uR.DateOfBooking == createRequestDTO.DateOfBooking).ToListAsync();
                if(userRequests.Count > 0)
                {
                    throw new BadRequestException("У вас уже есть заявка на этот день и на это время");
                }
                if (requestsAll.Count > 0)
                {
                    Request requestAll = new Request()
                    {
                        ClassroomNumber = createRequestDTO.ClassroomNumber,
                        RequestOwner = user.Name,
                        DateOfBooking = createRequestDTO.DateOfBooking,
                        DateOfSent = utcNow,
                        StartTime = requestTime.StartTime,
                        EndTime = requestTime.EndTime,
                        Status = RequestStatus.Rejected,
                        Id = Guid.NewGuid(),
                        TimeId = createRequestDTO.TimeId,
                        OwnerId = user.Id
                    };
                    _db.Requests.Add(requestAll);
                    await _db.SaveChangesAsync();
                }
                else if (requestsTeacher.Count > 0 && (user.Role == Roles.Teacher || user.Role == Roles.DeanTeacher))
                {
                    var allRequests = await _db.Requests.Where(aR => aR.ClassroomNumber.Contains(createRequestDTO.ClassroomNumber) && aR.TimeId == createRequestDTO.TimeId && aR.DateOfBooking == createRequestDTO.DateOfBooking).ToListAsync();
                    allRequests.ForEach(req => req.Status = RequestStatus.Rejected);

                    Request requestTeacher = new Request()
                    {
                        ClassroomNumber = createRequestDTO.ClassroomNumber,
                        RequestOwner = user.Name,
                        DateOfBooking = createRequestDTO.DateOfBooking,
                        DateOfSent = utcNow,
                        StartTime = requestTime.StartTime,
                        EndTime = requestTime.EndTime,
                        Status = RequestStatus.Approved,
                        Id = Guid.NewGuid(),
                        TimeId = createRequestDTO.TimeId,
                        OwnerId = user.Id
                    };
                    _db.Requests.Add(requestTeacher);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    Request request = new Request()
                    {
                        ClassroomNumber = createRequestDTO.ClassroomNumber,
                        RequestOwner = user.Name,
                        DateOfBooking = createRequestDTO.DateOfBooking,
                        DateOfSent = utcNow,
                        StartTime = requestTime.StartTime,
                        EndTime = requestTime.EndTime,
                        Status = RequestStatus.Pending,
                        Id = Guid.NewGuid(),
                        TimeId = createRequestDTO.TimeId,
                        OwnerId = user.Id,
                        ownerRole = user.Role
                    };
                    _db.Requests.Add(request);
                    await _db.SaveChangesAsync();
                }
            }
        }

        private IQueryable<Request> FilterRequests(RequestSorting sorting, IQueryable<Request> requests)
        {
            switch (sorting)
            {
                case RequestSorting.CreateAsc:
                    return requests.OrderBy(p => p.DateOfSent);
                default:
                    return requests.OrderByDescending(p => p.DateOfSent);
            }

        }
        public async Task<GetRequestsPageDTO> getAllRequestsDTO(List<RequestStatus> statuses, string token, int page,int size, string? classroomNumber, RequestSorting sorting, int? timeId)
        {
            var allRequests = _db.Requests.AsQueryable();
            List<GetAllRequestsDTO> requests = new List<GetAllRequestsDTO>();
            string email = _tokenHelper.GetUserEmailFromToken(token);

            if (!string.IsNullOrEmpty(email))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user.Role == Roles.Administrator || user.Role == Roles.Dean || user.Role == Roles.DeanTeacher)
                {
                    if (statuses != null && statuses.Any())
                    {
                        allRequests = allRequests.Where(aR => statuses.Contains(aR.Status));
                    }

                    if (classroomNumber != null) allRequests = allRequests.Where(aR => aR.ClassroomNumber.Contains(classroomNumber));
                    if (timeId != null)
                    {
                        allRequests = allRequests.Where(aR => aR.TimeId == timeId);
                    }
                    if (page <= 0)
                    {
                        page = 1;
                    }
                    allRequests = FilterRequests(sorting, allRequests);

                    int sizeOfPage = size;
                    var countOfPages = (int)Math.Ceiling((double)allRequests.Count() / sizeOfPage);
                    if (page <= countOfPages)
                    {
                        var lowerBound = page == 1 ? 0 : (page - 1) * sizeOfPage;
                        if (page < countOfPages)
                        {
                            allRequests = allRequests.Skip(lowerBound).Take(sizeOfPage);
                        }
                        else
                        {
                            allRequests = allRequests.Skip(lowerBound).Take(allRequests.Count() - lowerBound);
                        }
                    }
                    else
                    {
                        throw new BadRequestException("Такой страницы нет");
                    }
                    var paginationDto = new PaginationDTO
                    {
                        Current = page,
                        Count = countOfPages,
                        Size = sizeOfPage
                    };

                    var pageDto = new GetRequestsPageDTO
                    {
                        Requests = allRequests.Select(u => new GetAllRequestsDTO
                        {
                            Id = u.Id,
                            DateOfBooking = u.DateOfBooking,
                            Fullname = user.Fullname,
                            StartTime = u.StartTime,
                            EndTime = u.EndTime,
                            Status = u.Status,
                            ownerRole = u.ownerRole,
                            ClassroomNumber = u.ClassroomNumber,
                        }),
                        Pagination = paginationDto
                    };


                    return pageDto;

                }
                else
                {
                    throw new ForbiddenException("Вам недоступна данная функция");
                }
            }
            else
            {
                throw new ForbiddenException("Вам недоступна данная функция");
            }
        }

        public async Task<GetRequestsPageDTO> getAllUsersRequests(List<RequestStatus> statuses, string token, int page)
        {
            string email = _tokenHelper.GetUserEmailFromToken(token);

            if (!string.IsNullOrEmpty(email))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                var allRequests = _db.Requests.Where(aL => aL.OwnerId == user.Id).AsQueryable();
                if (user.Role == Roles.Teacher || user.Role == Roles.Student || user.Role == Roles.DeanTeacher)
                {
                    if (statuses != null && statuses.Any())
                    {
                        allRequests = allRequests.Where(aR => statuses.Contains(aR.Status));
                    }
                    if (page <= 0)
                    {
                        page = 1;
                    }

                    int sizeOfPage = 5;
                    var countOfPages = (int)Math.Ceiling((double)allRequests.Count() / sizeOfPage);
                    if (page <= countOfPages)
                    {
                        var lowerBound = page == 1 ? 0 : (page - 1) * sizeOfPage;
                        if (page < countOfPages)
                        {
                            allRequests = allRequests.Skip(lowerBound).Take(sizeOfPage);
                        }
                        else
                        {
                            allRequests = allRequests.Skip(lowerBound).Take(allRequests.Count() - lowerBound);
                        }
                    }
                    else
                    {
                        throw new BadRequestException("Такой страницы нет");
                    }
                    var paginationDto = new PaginationDTO
                    {
                        Current = page,
                        Count = countOfPages,
                        Size = sizeOfPage
                    };

                    var pageDto = new GetRequestsPageDTO
                    {
                        Requests = allRequests.Select(u => new GetAllRequestsDTO
                        {
                            Id = u.Id,
                            DateOfBooking = u.DateOfBooking,
                            Fullname = user.Fullname,
                            StartTime = u.StartTime,
                            EndTime = u.EndTime,
                            Status = u.Status,
                            ownerRole = u.ownerRole,
                            ClassroomNumber = u.ClassroomNumber,
                        }),
                        Pagination = paginationDto
                    };


                    return pageDto;

                }
                else
                {
                    throw new ForbiddenException("Вам недоступна данная функция");
                }


            }
            else
            {
                throw new ForbiddenException("Вам недоступна данная функция");
            }
        }

        public async Task approveRequest(ApproveRequestDTO approveRequestDTO, string token)
        {
            string email = _tokenHelper.GetUserEmailFromToken(token);

            if (!string.IsNullOrEmpty(email))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                var request = await _db.Requests.FirstOrDefaultAsync(r => r.Id == approveRequestDTO.RequestId);

                if (user.Role == Roles.Dean || user.Role == Roles.Administrator || user.Role == Roles.DeanTeacher )
                {
                    if(request != null)
                    {
                        if (request.Status != RequestStatus.Approved)
                        {
                            var allRequests = await _db.Requests.Where(aR => aR.ClassroomNumber.Contains(request.ClassroomNumber) && aR.TimeId == request.TimeId && aR.DateOfBooking == request.DateOfBooking).ToListAsync();
                            allRequests.ForEach(req => req.Status = RequestStatus.Rejected);
                            request.Status = RequestStatus.Approved;
                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            throw new NotFoundException("Данной заявке уже был выдан вердикт");
                        }
                    }
                    else
                    {
                        throw new NotFoundException("Данная заявка не найдена");
                    }
                }
                else
                {
                    throw new ForbiddenException("Ваша роль не подходит для подтверждения заявок");
                }
            }
            else
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
        }
        public async Task rejectRequest(RejectRequestDTO rejectRequestDTO, string token)
        {
            string email = _tokenHelper.GetUserEmailFromToken(token);

            if (!string.IsNullOrEmpty(email))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                var request = await _db.Requests.FirstOrDefaultAsync(r => r.Id == rejectRequestDTO.RequestId);

                if (user.Role == Roles.Dean || user.Role == Roles.Administrator || user.Role == Roles.DeanTeacher)
                {
                    if (request != null)
                    {
                        if (request.Status != RequestStatus.Rejected)
                        {
                            request.Status = RequestStatus.Rejected;
                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            throw new NotFoundException("Данной заявке уже был выдан вердикт");
                        }
                    }
                    else
                    {
                        throw new NotFoundException("Данная заявка не найдена");
                    }
                }
                else
                {
                    throw new ForbiddenException("Ваша роль не подходит для подтверждения заявок");
                }
            }
            else
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
        }

        public async Task deleteRequest(DeleteRequestDTO deleteRequestDTO, string token)
        {
            string email = _tokenHelper.GetUserEmailFromToken(token);

            if (!string.IsNullOrEmpty(email))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if(user != null)
                {
                    var request = await _db.Requests.FirstOrDefaultAsync(r => r.Id == deleteRequestDTO.RequestId);
                    if (user.Role == Roles.Teacher || user.Role == Roles.Student || user.Role == Roles.DeanTeacher)
                    {
                        if (request != null)
                        {
                            if (request.OwnerId == user.Id)
                            {
                                _db.Requests.Remove(request);
                                await _db.SaveChangesAsync();
                            }
                            else
                            {
                                throw new NotFoundException("Вы не можете удалить не свою заявку");
                            }
                        }
                        else
                        {
                            throw new NotFoundException("Данная заявка не найдена");
                        }
                    }
                    else
                    {
                        throw new ForbiddenException("Ваша роль не подходит для удаления заявок");
                    }

                }
                else
                {
                    throw new UnauthorizedException("Данный пользователь не авторизован");
                }

            }
        }
    }
}
