using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountMicroservice.Data;
using AccountMicroservice.Data.Models;
using Api.DtoModels.Auth;
using Microsoft.EntityFrameworkCore;
using Ping.Commons.Dtos.Models.Auth;

namespace AccountMicroservice.Data.Services.Impl
{
    public class AccountService : IAccountService
    {
        private MyDbContext dbContext;

        public AccountService(MyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<bool> AddContact(ContactDto contactDto)
        {
            Account account = await dbContext.Accounts
                .Where(a => a.PhoneNumber == contactDto.PhoneNumber)
                .SingleOrDefaultAsync();

            if (account == null) return false;

            Account contactAccount = await dbContext.Accounts
                .Where(a => a.PhoneNumber == contactDto.ContactPhoneNumber)
                .SingleOrDefaultAsync();

            if (contactAccount == null) return false; // Implement adding contacts that are not currently registered

            Contact contact = new Contact
            {
                AccountId = account.Id,
                ContactAccountId = contactAccount.Id,
                ContactName = contactDto.ContactName,
                DateAdded = contactDto.DateAdded
            };

            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync();

            return true;
        }

        // TODO: Handle all the custom mapping (Db to Dto models) - probably by creating a IMapper?
        public async Task<AccountDto> UpdateAvatar(AccountDto accountDto)
        {
            var account = dbContext.Accounts.Where(a => a.PhoneNumber == accountDto.PhoneNumber).SingleOrDefault();
            if (account == null) return null;

            account.AvatarImageUrl = accountDto.AvatarImageUrl;

            await dbContext.SaveChangesAsync();

            return new AccountDto
            {
                Id = account.Id,
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
        public async Task<AccountDto> UpdateCover(AccountDto accountDto)
        {
            var account = dbContext.Accounts.Where(a => a.PhoneNumber == accountDto.PhoneNumber).SingleOrDefault();
            if (account == null) return null;

            account.CoverImageUrl = accountDto.CoverImageUrl;

            await dbContext.SaveChangesAsync();

            return new AccountDto
            {
                Id = account.Id,
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

        public async Task<AccountDto> Update(AccountDto accountDto)
        {
            var account = dbContext.Accounts.Where(a => a.PhoneNumber == accountDto.PhoneNumber).SingleOrDefault();
            if (account == null) return null;

            account.Firstname = accountDto.Firstname;
            account.Lastname = accountDto.Lastname;
            account.Email = accountDto.Email;
            account.AvatarImageUrl = accountDto.AvatarImageUrl;

            await dbContext.SaveChangesAsync();

            return new AccountDto
            {
                Id = account.Id,
                Firstname = account.Firstname,
                Lastname = account.Lastname,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                DateRegistered = account.DateRegistered,
                Token = dbContext.AuthTokens.Where(at => at.AccountId == account.Id).SingleOrDefault()?.Value,
                CreateSession = true
            };
        }

    }
}
