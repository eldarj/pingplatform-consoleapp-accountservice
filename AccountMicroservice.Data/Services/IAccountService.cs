﻿using Api.DtoModels.Auth;
using Ping.Commons.Dtos.Models.Auth;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AccountMicroservice.Data.Services
{
    public interface IAccountService
    {
        Task<AccountDto> UpdateAvatar(AccountDto accountDto);
        Task<AccountDto> UpdateCover(AccountDto accountDto);
        Task<AccountDto> Update(AccountDto accountDto);
        Task<bool> AddContact(ContactDto contactDto);
    }
}
