﻿
using tsuKeysAPIProject.AdditionalServices.Exceptions;
using tsuKeysAPIProject.AdditionalServices.TokenHelpers;
using tsuKeysAPIProject.DBContext;
using tsuKeysAPIProject.DBContext.DTO.KeyDTO;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.DBContext.Models.Enums;
using tsuKeysAPIProject.Services.IServices.IKeyService;

namespace tsuKeysAPIProject.Services
{
    public class KeyService: IKeyService
    {

        private readonly AppDBContext _db;
        private readonly TokenInteraction _tokenHelper;

        public KeyService(AppDBContext db, TokenInteraction tokenInteraction)
        {
            _db = db;
            _tokenHelper = tokenInteraction;
        }

        public async Task CreateKey(CreateKeyDTO createKeyDTO, string token)
        {
            var userEmail = _tokenHelper.GetUserEmailFromToken(token);

            var userRole = _db.Users
                .Where(u => u.Email == userEmail)
                .Select(u => u.Role)
                .FirstOrDefault();

            if (userRole != Roles.Dean || userRole != Roles.Administrator)
            {
                throw new ForbiddenException("Ключи могут создавать только работники деканата или администраторы");
            }

            Key key = new Key()
            {
                Owner = "Dean",
                ClassroomNumber = createKeyDTO.ClassroomNumber,
            };

            await _db.Keys.AddAsync(key);
            await _db.SaveChangesAsync();
        }

        public async Task AcceptKeyRequest(string classroomNumber)
        {
            throw new NotImplementedException();
        }

        public async Task DeclineKeyRequest(string classroomNumber)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteKey(string classroom)
        {
            throw new NotImplementedException();
        }

        public async Task GetAllKeys()
        {
            throw new NotImplementedException();
        }

        public async Task SendKeyRequest(KeyRequestDTO keyRequestDTO)
        {
            throw new NotImplementedException();
        }

        public Task GetAllRequests()
        {
            throw new NotImplementedException();
        }

        public Task ConfirmReceipt(string classroomNumber)
        {
            throw new NotImplementedException();
        }
    }
}
