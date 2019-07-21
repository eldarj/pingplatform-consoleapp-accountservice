using AccountMicroservice.Data;
using AccountMicroservice.Data.Models;
using Api.DtoModels.Auth;
using Microsoft.EntityFrameworkCore;
using Ping.Commons.Dtos.Models.Auth;
using Ping.Commons.Settings;
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
        public AccountDto Authenticate(AccountDto accountDto, string secret)
        {
            var account = dbContext.Accounts
                .Where(a => a.PhoneNumber == accountDto.PhoneNumber)
                .Include(a => a.CallingCodeObj)
                .Include(a => a.Contacts)
                    .ThenInclude(c => c.ContactAccount)
                .SingleOrDefault();

            if (account == null) return null;

            // Update token // TODO - check db, and also start using countrycode + phonenumber
            AuthToken token = dbContext.AuthTokens.Where(at => at.AccountId == account.Id).SingleOrDefault();
            if (token == null)
            {
                token = new AuthToken { AccountId = account.Id };
            }
            token.Value = JWTokenHandler.GenerateToken(account.PhoneNumber, secret);
            dbContext.SaveChangesAsync();

            return new AccountDto
            {
                Firstname = account.Firstname,
                Lastname = account.Lastname,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                CallingCountryCode = account.CallingCodeObj.CallingCountryCode,
                DateRegistered = account.DateRegistered,
                Token = token.Value,
                AvatarImageUrl = account.AvatarImageUrl,
                CoverImageUrl = account.CoverImageUrl,
                CreateSession = true,
                Contacts = account.Contacts?
                    .Select(c => new ContactDto
                    {
                        DateAdded = c.DateAdded,
                        ContactName = c.ContactName,
                        AvatarImageUrl = c.ContactAccount.AvatarImageUrl,
                        CoverImageUrl = c.ContactAccount.CoverImageUrl,
                        ContactPhoneNumber = c.ContactAccount.PhoneNumber,
                        ContactAccountId = c.ContactAccountId,
                        IsFavorite = c.IsFavorite
                    })
                    .OrderBy(c => c.ContactName)
                    .ToList()
            };
        }

        public async Task<AccountDto> Registration(AccountDto accountDto, string secret)
        {
            Account account = dbContext.Accounts
                .Where(a => a.PhoneNumber == accountDto.PhoneNumber && a.CallingCountryCode == accountDto.CallingCountryCode)
                .SingleOrDefault();
            if (account != null) return null;

            account = new Account
            {
                Firstname = accountDto.Firstname,
                Lastname = accountDto.Lastname,
                Email = accountDto.Email,
                PhoneNumber = accountDto.PhoneNumber,
                CallingCodeObj = await dbContext.CallingCode.Where(cc => cc.CallingCountryCode == accountDto.CallingCountryCode).SingleOrDefaultAsync()
            };

            if (accountDto.Contacts?.Count > 0)
            {
                // add as contacts those accounts that exist (TODO: Change to consider country prefix and possible '0' infront of phone number)
                var dtoContacts = accountDto.Contacts.Select(dto => dto.ContactPhoneNumber).ToList();
                List<Account> contactsToAdd = dbContext.Accounts 
                    .Where(a => dtoContacts.Contains(a.PhoneNumber))
                    .ToList();

                account.Contacts = contactsToAdd.Select(a => new Contact
                {
                    Account = account,
                    ContactAccountId = a.Id,
                    ContactName = accountDto.Contacts.SingleOrDefault(c => c.ContactPhoneNumber == a.PhoneNumber)?.ContactName
                })
                .ToList();
            }

            // TODO: Generate a Token (on Prod this will be a phone/sms code confirmation ~ on Dev think about how to implement this)
            var token = new AuthToken
            {
                Account = account,
                Value = JWTokenHandler.GenerateToken(account.PhoneNumber, secret)
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
                CallingCountryCode = account.CallingCodeObj.CallingCountryCode,
                DateRegistered = account.DateRegistered,
                Token = token.Value,
                CreateSession = true,
                Contacts = account.Contacts?.Select(c => new ContactDto
                {
                    DateAdded = c.DateAdded,
                    ContactName = c.ContactName,
                    AvatarImageUrl = c.ContactAccount.AvatarImageUrl,
                    CoverImageUrl = c.ContactAccount.CoverImageUrl,
                    ContactPhoneNumber = c.ContactAccount.PhoneNumber,
                    ContactAccountId = c.ContactAccountId
                }).ToList()
            };
        }
    }
}
