using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.DBContext;
using tsuKeysAPIProject.DBContext.DTO.RequestDTO;
using tsuKeysAPIProject.DBContext.DTO.RolesDTO;
using tsuKeysAPIProject.DBContext.DTO.UserDTO;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.DBContext.Models.Enums;
using tsuKeysAPIProject.Services.IServices.IRolesService;

namespace tsuKeysAPIProject.Services
{
    public class RoleService : IRoleService
    {
        private readonly AppDBContext _db;
        private readonly TokenInteraction _tokenHelper;

        public RoleService(AppDBContext db, IConfiguration configuration, TokenInteraction tokenHelper)
        {
            _db = db;
            _tokenHelper = tokenHelper;
        }


        public async Task grantRole(GrantRoleRequestDTO grantRole, string token)
        {
            string email = _tokenHelper.GetUserEmailFromToken(token);

            if (!string.IsNullOrEmpty(email))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                var secondUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == grantRole.Id);

                if (user != null && secondUser != null)
                {
                    if (user.Role == Roles.Administrator)
                    {
                        secondUser.Role = grantRole.Role;
                        await _db.SaveChangesAsync();
                    }
                    else if (user.Role == Roles.Dean)
                    {
                        if(grantRole.Role == Roles.Dean || grantRole.Role == Roles.Administrator)
                        {
                            throw new ForbiddenException("Ваша роль не подходит для выдачи данной роли");
                        }
                        else
                        {
                            secondUser.Role = grantRole.Role;
                            await _db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        throw new ForbiddenException("Ваша роль не подходит для выдачи данной роли");
                    }
                }
                else
                {
                    throw new NotFoundException("Пользователь не найден");
                }
            }
            else
            {
                throw new UnauthorizedException("Данный пользователь не авторизован");
            }
        }
        public async Task<GetUsersPageDTO> getUsersInformation(string token, string? fullname, Roles? role, int size, int page)
        {
            var allUsers = _db.Users.AsQueryable();
            string email = _tokenHelper.GetUserEmailFromToken(token);

            if (!string.IsNullOrEmpty(email))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user.Role == Roles.Administrator || user.Role == Roles.Dean)
                {
                    if (role != null)
                    {
                        allUsers = allUsers.Where(aU => aU.Role == role);
                    }

                    if (!string.IsNullOrEmpty(fullname))
                    {
                        allUsers = allUsers.Where(aU => aU.Fullname.Contains(fullname));
                    }

                    if (page <= 0)
                    {
                        page = 1;
                    }
                    int sizeOfPage = size;
                    var countOfPages = (int)Math.Ceiling((double)allUsers.Count() / sizeOfPage);
                    if (page <= countOfPages)
                    {
                        var lowerBound = page == 1 ? 0 : (page - 1) * sizeOfPage;
                        if (page < countOfPages)
                        {
                            allUsers = allUsers.Skip(lowerBound).Take(sizeOfPage);
                        }
                        else
                        {
                            allUsers = allUsers.Skip(lowerBound).Take(allUsers.Count() - lowerBound);
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

                    var pageDto = new GetUsersPageDTO
                    {
                        Users = allUsers.Select(u => new GetUserInformationResponseDTO
                        {
                            Fullname = u.Fullname,
                            Role = u.Role,
                            Email = u.Email
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

    }
}
