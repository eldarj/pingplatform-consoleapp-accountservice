﻿using AccountMicroservice.Data;
using AccountMicroservice.Data.Models;
using Api.DtoModels.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountMicroservice.Data.Services.Impl
{
    public class AuthService : IAuthService
    {
        private MyDbContext dbContext;

        public AuthService(MyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // TODO: Handle all the custom mapping (Db to Dto models) - probably by creating a IMapper?
        public AccountDto Authenticate(AccountLoginDto accountDto)
        {
            return this.Authenticate(accountDto.PhoneNumber);
        }

        public AccountDto Authenticate(string phoneNumber)
        {
            var account = dbContext.Accounts.Where(a => a.PhoneNumber == phoneNumber).SingleOrDefault();
            if (account == null) return null;

            return new AccountDto
            {
                Firstname = account.Firstname,
                Lastname = account.Lastname,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                DateRegistered = account.DateRegistered,
                Token = dbContext.AuthTokens.Where(at => at.AccountId == account.Id).SingleOrDefault()?.Value,
                AvatarImageUrl = account.AvatarImageUrl,
                CoverImageUrl = account.CoverImageUrl,
                CreateSession = true
            };
        }

        public async Task<AccountDto> Registration(AccountRegisterDto accountDto)
        {
            var account = dbContext.Accounts.Where(a => a.PhoneNumber == accountDto.PhoneNumber).SingleOrDefault();
            if (account != null) return null;

            account = new Account
            {
                Firstname = accountDto.Firstname,
                Lastname = accountDto.Lastname,
                Email = accountDto.Email,
                PhoneNumber = accountDto.PhoneNumber
            };

            // TODO: Generate a Token (on Prod this will be a phone/sms code confirmation ~ on Dev think about how to implement this)
            var token = new AuthToken
            {
                Account = account,
                Value = "xxx"
            };

            dbContext.Accounts.Add(account);
            dbContext.AuthTokens.Add(token);

            await dbContext.SaveChangesAsync();

            return new AccountDto
            {
                Firstname = account.Firstname,
                Lastname = account.Lastname,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                DateRegistered = account.DateRegistered,
                Token = token.Value,
                CreateSession = true
            };
        }
    }
}