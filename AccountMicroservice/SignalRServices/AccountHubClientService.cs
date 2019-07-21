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
        private readonly IAuthService authService;

        public AccountHubClientService(IOptions<GatewayBaseSettings> gatewayBaseOptions, 
            IOptions<SecuritySettings> securityOptions,
            ILogger<AuthHubClientService> logger)
            : base(gatewayBaseOptions, securityOptions, HUB_ENDPOINT)
        {
            this.logger = logger;
            this.authService = authService;
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
            // TODO: Finish this after implementing AuthHub and ChatHub
            //hubConnectionAccount.On<string, string, string>("CoverUpload", async (appId, phoneNumber, imgUrl) =>
            //{
            //    logger.LogInformation($"-- {appId} requesting CoverUpload. for {phoneNumber}.");

            //    //var authedAccount = authService.Authenticate(phoneNumber);
            //    //if (authedAccount != null)
            //    //{
            //    //    logger.LogInformation($"-- {phoneNumber} authenticated (Success). " +
            //    //        $"Proceeding to cover upload.");

            //    //    authedAccount.CoverImageUrl = imgUrl;

            //    //    AccountDto accountResponse = await accountService.UpdateCover(authedAccount);
            //    //    if (accountResponse != null)
            //    //    {
            //    //        await hubConnectionAccount.SendAsync("UpdateProfileSuccess", appId, accountResponse);
            //    //        return;
            //    //    }
            //    //}

            //    //logger.LogInformation($"-- {phoneNumber} did not authenticate (Fail). " +
            //    //    $"Requested by: {appId} - sending back error message.");
            //    //await hubConnectionAccount.SendAsync("UpdateProfileFailed", appId, $"Authentication failed for: {appId}");
            //});

            hubConnection.On<string, string, string>("AvatarUpload", async (appId, phoneNumber, imgUrl) =>
            {
                logger.LogInformation($"-- {appId} requesting AvatarUpload. for {phoneNumber}.");

                //var authedAccount = authService.Authenticate(phoneNumber);
                //if (authedAccount != null)
                //{
                //    logger.LogInformation($"-- {phoneNumber} authenticated (Success). " +
                //        $"Proceeding to avatar upload.");

                //    authedAccount.AvatarImageUrl = imgUrl;

                //    AccountDto accountResponse = await accountService.UpdateAvatar(authedAccount);
                //    if (accountResponse != null)
                //    {
                //        await hubConnectionAccount.SendAsync("UpdateProfileSuccess", appId, accountResponse);
                //        return;
                //    }
                //}

                //logger.LogInformation($"-- {phoneNumber} did not authenticate (Fail). " +
                //    $"Requested by: {appId} - sending back error message.");
                //await hubConnectionAccount.SendAsync("UpdateProfileFailed", appId, $"Authentication failed for: {appId}");
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
