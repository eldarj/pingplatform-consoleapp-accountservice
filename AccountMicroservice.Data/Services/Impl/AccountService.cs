using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountMicroservice.Data.Models;
using Api.DtoModels.Auth;
using Microsoft.EntityFrameworkCore;
using Ping.Commons.Dtos.Models.Auth;
using Ping.Commons.Dtos.Models.Chat;

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

            Contact contact = await dbContext.Contacts
                .Where(c => c.Account.PhoneNumber == contactDto.PhoneNumber && c.ContactAccount.PhoneNumber == contactDto.ContactPhoneNumber)
                .SingleOrDefaultAsync();

            if (contact == null)
            {
                contact = new Contact();
                dbContext.Contacts.Add(contact);
            }

            contact.AccountId = account.Id;
            contact.ContactAccountId = contactAccount.Id;
            contact.ContactName = contactDto.ContactName;
            contact.DateAdded = contactDto.DateAdded;
            contact.IsFavorite = contactDto.IsFavorite;

            await dbContext.SaveChangesAsync();

            return true;
        }

        // TODO: Handle all the custom mapping (Db to Dto models) - probably by creating a IMapper?
        public async Task<AccountDto> UpdateAvatar(string phoneNumber, string imgUrl)
        {
            var account = dbContext.Accounts
                .Include(a => a.CallingCodeObj)
                .Where(a => a.PhoneNumber == phoneNumber)
                .SingleOrDefault();

            if (account == null) return null;

            account.AvatarImageUrl = imgUrl;

            await dbContext.SaveChangesAsync();

            return new AccountDto
            {
                Id = account.Id,
                Firstname = account.Firstname,
                Lastname = account.Lastname,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                CallingCountryCode = account.CallingCodeObj.CallingCountryCode,
                DateRegistered = account.DateRegistered,
                Token = dbContext.AuthTokens.Where(at => at.AccountId == account.Id).SingleOrDefault()?.Value,
                AvatarImageUrl = account.AvatarImageUrl,
                CoverImageUrl = account.CoverImageUrl,
                CreateSession = true
            };
        }

        // TODO: Handle all the custom mapping (Db to Dto models) - probably by creating a IMapper?
        public async Task<AccountDto> UpdateAvatar(AccountDto accountDto) => 
            await UpdateAvatar(accountDto.PhoneNumber, accountDto.AvatarImageUrl);

        public async Task<AccountDto> UpdateCover(string phoneNumber, string imgUrl)
        {
            var account = dbContext.Accounts
                .Include(a => a.CallingCodeObj)
                .Where(a => a.PhoneNumber == phoneNumber)
                .SingleOrDefault();

            if (account == null) return null;

            account.CoverImageUrl = imgUrl;

            await dbContext.SaveChangesAsync();

            return new AccountDto
            {
                Id = account.Id,
                Firstname = account.Firstname,
                Lastname = account.Lastname,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                CallingCountryCode = account.CallingCodeObj.CallingCountryCode,
                DateRegistered = account.DateRegistered,
                Token = dbContext.AuthTokens.Where(at => at.AccountId == account.Id).SingleOrDefault()?.Value,
                AvatarImageUrl = account.AvatarImageUrl,
                CoverImageUrl = account.CoverImageUrl,
                CreateSession = true
            };
        }

        public async Task<AccountDto> UpdateCover(AccountDto accountDto) =>
            await UpdateCover(accountDto.PhoneNumber, accountDto.CoverImageUrl);

        public async Task<AccountDto> Update(AccountDto accountDto)
        {
            var account = dbContext.Accounts
                .Include(a => a.CallingCodeObj)
                .Where(a => a.PhoneNumber == accountDto.PhoneNumber)
                .SingleOrDefault();
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
                CallingCountryCode = account.CallingCodeObj.CallingCountryCode,
                DateRegistered = account.DateRegistered,
                Token = dbContext.AuthTokens.Where(at => at.AccountId == account.Id).SingleOrDefault()?.Value,
                CreateSession = true
            };
        }

        public async Task<List<CallingCodeDto>> GetCallingCodes()
        {
            return await dbContext.CallingCode
                .Select(cc => new CallingCodeDto
                {
                    CallingCountryCode = cc.CallingCountryCode,
                    CountryName = cc.CountryName,
                    IsoCode = cc.IsoCode
                })
                .ToListAsync();
        }
    }
}
