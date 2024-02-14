﻿using Microsoft.EntityFrameworkCore;
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.DBContext;
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

            Console.WriteLine(grantRole.Id);
            Console.WriteLine(grantRole.Role);

            if (!string.IsNullOrEmpty(email))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                var secondUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == grantRole.Id);

                if (user != null && secondUser != null)
                {
                    if (user.Role == Roles.Administrator || user.Role == Roles.Dean)
                    {
                        secondUser.Role = grantRole.Role;
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        throw new ForbiddenException("Ваша роль не та, которая нужна");
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

    }
}
