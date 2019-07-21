using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Api.DtoModels.Auth;
using AccountMicroservice.Data.Services;
using AccountMicroservice.SignalRServices.Interfaces;
using AccountMicroservice.MessageBus.Publishers.Interfaces;

using Ping.Commons.Settings;
using Ping.Commons.SignalR.Base;
using Ping.Commons.Dtos.Models.Auth;
using Ping.Commons.Dtos.Models.Wrappers.Response;

namespace AccountMicroservice.SignalRServices
{
    public class AccountHubClientService : BaseHubClientService, IAccountHubClientService
    {
        private static readonly string HUB_ENDPOINT = "accounthub";

        private readonly ILogger logger;
        private readonly IAccountService accountService;

        public AccountHubClientService(IOptions<GatewayBaseSettings> gatewayBaseOptions, 
            IOptions<SecuritySettings> securityOptions,
            IAccountService accountService,
            ILogger<AccountHubClientService> logger)
            : base(gatewayBaseOptions, securityOptions, HUB_ENDPOINT)
        {
            this.logger = logger;
            this.accountService = accountService;
        }

        public async void Connect()
        {
            await hubConnection.StartAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    logger.LogInformation("-- Couln't connect to SignalR AccountHub (OnStarted)");
                    return;
                }
                logger.LogInformation("AccountMicroservice connected to AuthHub successfully (OnStarted)");
                RegisterHandlers();
            });
        }

        public void RegisterHandlers()
        {
            hubConnection.On<string, string>("CoverUpload", async (phoneNumber, imgUrl) =>
            {
                logger.LogInformation($"[{phoneNumber}] - request cover update {imgUrl}.");

                AccountDto accountResponse = await accountService.UpdateCover(phoneNumber, imgUrl);
                if (accountResponse != null)
                {
                    logger.LogInformation($"[{phoneNumber}] - Updated cover image to {imgUrl} (Success). Sending back data.");
                    await hubConnection.SendAsync("UpdateProfileSuccess", phoneNumber, accountResponse);
                    return;
                }

                logger.LogWarning($"[{phoneNumber}] - Couldn't find account by given phone number (Fail - CoverUpload). Sending back error message.");
                await hubConnection.SendAsync("UpdateProfileFailed", phoneNumber, $"Authentication failed for: {phoneNumber}");
            });

            hubConnection.On<string, string>("AvatarUpload", async (phoneNumber, imgUrl) =>
            {
                logger.LogInformation($"[{phoneNumber}] - requesting avatar update {imgUrl}.");

                AccountDto accountResponse = await accountService.UpdateAvatar(phoneNumber, imgUrl);
                if (accountResponse != null)
                {
                    logger.LogInformation($"[{phoneNumber}] - Updated profile image to {imgUrl} (Success). Sending back data.");
                    await hubConnection.SendAsync("UpdateProfileSuccess", phoneNumber, accountResponse);
                    return;
                }

                logger.LogWarning($"[{phoneNumber}] - Couldn't find account by given phone number (Fail - AvatarUpload). Sending back error message.");
                await hubConnection.SendAsync("UpdateProfileFailed", phoneNumber, $"Authentication failed for: {phoneNumber}");
            });

            //hubConnectionAccount.On<string, AccountDto>("UpdateProfile", async (appId, accountRequest) =>
            //{
            //    logger.LogInformation($"-- {appId} requesting ProfileUpdate. for {accountRequest.PhoneNumber}.");

            //    //var authedAccount = authService.Authenticate(accountRequest.PhoneNumber);
            //    //if (authedAccount != null)
            //    //{
            //    //    logger.LogInformation($"-- {accountRequest.PhoneNumber} authenticated (Success). " +
            //    //        $"Proceeding to profile update.");

            //    //    AccountDto accountResponse = await accountService.Update(accountRequest);
            //    //    if (accountResponse != null)
            //    //    {
            //    //        await hubConnectionAccount.SendAsync("UpdateProfileSuccess", appId, accountResponse);
            //    //        return;
            //    //    }
            //    //}

            //    //logger.LogInformation($"-- {accountRequest.PhoneNumber} did not authenticate (Fail). " +
            //    //    $"Requested by: {appId} - sending back error message.");
            //    //await hubConnectionAccount.SendAsync("UpdateProfileFailed", appId, $"Authentication failed for: {appId}");
            //});
        }
    }
}
