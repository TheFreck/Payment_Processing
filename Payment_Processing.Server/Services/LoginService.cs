﻿using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
using Payment_Processing.Server.DTO;
using Payment_Processing.Server.Repos;
using System.Security.Cryptography;
using System.Text;

namespace Payment_Processing.Server.Services
{
    public interface ILoginService
    {
        (string hash, byte[] salt) CreateLogin(string request);
        Task<Account> LoginAsync(string username, string password);
        Task<bool> LogOutAsync(string username);
        string MakeHash(string preHash, out byte[] salt);
        Task<bool> ValidatePermissionsAsync(Account account, PermissionType admin);
        Task<bool> ValidateTokenAsync(string username, string token);
        bool VerifyHash(string password, string hashword, byte[] salt);
    }

    public class LoginService : ILoginService
    {
        private readonly IAccountRepo accountRepo;
        public LoginService(IAccountRepo accountRepo)
        {
            this.accountRepo = accountRepo;
        }

        public (string hash, byte[] salt) CreateLogin(string request)
        {
            var passwordHash = MakeHash(request, out var passwordSalt);
            return (passwordHash, passwordSalt);
        }

        public async Task<Account> LoginAsync(string username, string password)
        {
            var account = await accountRepo.GetByUsernameAsync(username);
            if(VerifyHash(password, account.Password, account.PasswordSalt))
            {
                account.Token = MakeHash(Guid.NewGuid().ToString(),out var tokenSalt);
                account.TokenSalt = tokenSalt;
                await accountRepo.UpdateAsync(account);
                return account;
            }
            return new Account();
        }

        public async Task<bool> LogOutAsync(string username)
        {
            var account = await accountRepo.GetByUsernameAsync(username);
            account.Token = Guid.Empty.ToString();
            account.TokenSalt = new byte[64];
            await accountRepo.UpdateAsync(account);
            return true;
        }

        public string MakeHash(string preHash, out byte[] salt)
        {
            const int keySize = 64;
            const int iterations = 350000;
            salt = RandomNumberGenerator.GetBytes(keySize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(preHash),
                salt,
                iterations,
                HashAlgorithmName.SHA512,
                keySize);
            return Convert.ToHexString(hash);
        }

        public async Task<bool> ValidatePermissionsAsync(Account account, PermissionType permission)
        {
            var gotten = await accountRepo.GetByUsernameAsync(account.Username);
            foreach(var perm in gotten.Permissions)
            {
                if (perm.Type == permission && VerifyHash(permission.ToString(), perm.Token, perm.TokenSalt))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> ValidateTokenAsync(string username, string token)
        {
            var account = await accountRepo.GetByUsernameAsync(username);
            return account != null && account.Token == token;
        }

        public bool VerifyHash(string password, string hashword, byte[] salt)
        {
            const int keySize = 64;
            const int iterations = 350000;
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA512,
                keySize);
            return Convert.ToHexString(hash) == hashword;
        }
    }
}