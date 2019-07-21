using Api.DtoModels.Auth;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AccountMicroservice.Data.Services
{
    public interface IAuthService
    {
        AccountDto Authenticate(AccountDto account, string secret);
        Task<AccountDto> Registration(AccountDto accountDto, string secret);
    }
}
