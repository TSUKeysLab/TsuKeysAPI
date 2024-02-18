using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Collections;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.DBContext;
using tsuKeysAPIProject.DBContext.DTO.RequestDTO;
using tsuKeysAPIProject.DBContext.DTO.RolesDTO;
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
                var requestTime = await _db.TimeSlots.FirstOrDefaultAsync(rt => rt.Id == createRequestDTO.TimeId);
                var key = await _db.Keys.FirstOrDefaultAsync(k => k.ClassroomNumber == createRequestDTO.ClassroomNumber);
                if (requestTime == null)
                {
                    throw new NotFoundException("Выбранное вами время не существует");
                }
                if (key == null)
                {
                    throw new NotFoundException("Выбранной вами аудитории не существует");

                }

                Request request = new Request()
                {
                    ClassroomNumber = createRequestDTO.ClassroomNumber,
                    RequestOwner = user.Name,
                    DateOfBooking = createRequestDTO.DateOfBooking,
                    DateOfSent = DateTime.UtcNow,
                    StartTime = requestTime.StartTime,
                    EndTime = requestTime.EndTime,
                    Status = RequestStatus.Pending,
                    Id = Guid.NewGuid(),
                    TimeId = createRequestDTO.TimeId
                };
                _db.Requests.Add(request);
                await _db.SaveChangesAsync();
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
        public async Task<GetRequestsPageDTO> getAllRequestsDTO(List<RequestStatus> statuses, string token, int page,int size, string? classroomNumber, RequestSorting sorting, Guid? timeId)
        {
            var allRequests = _db.Requests.AsQueryable();
            List<GetAllRequestsDTO> requests = new List<GetAllRequestsDTO>();
            string email = _tokenHelper.GetUserEmailFromToken(token);

            if (!string.IsNullOrEmpty(email))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user.Role == Roles.Administrator)
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
                        Requests = allRequests,
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
    }
}
