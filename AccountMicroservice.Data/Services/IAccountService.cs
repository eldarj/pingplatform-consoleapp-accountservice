using Api.DtoModels.Auth;
using Ping.Commons.Dtos.Models.Auth;
using Ping.Commons.Dtos.Models.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AccountMicroservice.Data.Services
{
    public interface IAccountService
    {
        Task<AccountDto> UpdateAvatar(string phoneNumber, string imgUrl);
        Task<AccountDto> UpdateAvatar(AccountDto accountDto);
        Task<AccountDto> UpdateCover(string phoneNumber, string imgUrl);
        Task<AccountDto> UpdateCover(AccountDto accountDto);
        Task<AccountDto> Update(AccountDto accountDto);
        Task<bool> AddContact(ContactDto contactDto);
        Task<List<CallingCodeDto>> GetCallingCodes();
    }
}
