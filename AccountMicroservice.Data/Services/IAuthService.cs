using Api.DtoModels.Auth;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AccountMicroservice.Data.Services
{
    public interface IAuthService
    {
        AccountDto Authenticate(AccountLoginDto account);
        AccountDto Authenticate(string phoneNumber);
        Task<AccountDto> Registration(AccountRegisterDto accountDto);
    }
}
