using AccountMicroservice.Settings;
using AccountMicroservice.SignalRServices.Base;
using AccountMicroservice.SignalRServices.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ping.Commons.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AccountMicroservice.SignalRServices
{
    public class AccountHubClientService : BaseHubClientService, IAccountHubClientService
    {
        private static readonly string HUB_ENDPOINT = "accounthub";

        private readonly ILogger logger;
        //private readonly IAccountMQPublisher accountMQPublisher;

        public AccountHubClientService(IOptions<GatewayBaseSettings> gatewayBaseOptions, 
            IOptions<SecuritySettings> securityOptions,
            ILogger<AuthHubClientService> logger)
            : base(gatewayBaseOptions, securityOptions, HUB_ENDPOINT)
        {
            this.logger = logger;
        }

        public async void Connect()
        {
            await hubConnection.StartAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    logger.LogInformation("-- Couln't connect to signalR AuthHub (OnStarted)");
                    return;
                }
                logger.LogInformation("AccountMicroservice connected to AuthHub successfully (OnStarted)");
                this.RegisterHandlers();
            });
        }

        public void RegisterHandlers()
        {
            // TODO: Finish this after implementing AuthHub and ChatHub
            //await hubConnectionAccount.StartAsync().ContinueWith(t =>
            //{
            //    if (t.IsFaulted)
            //    {
            //        logger.LogInformation("-- Couln't connect to signalR AccountHub (OnStarted)");
            //        return;
            //    }
            //    logger.LogInformation("AccountMicroservice connected to AccountHub successfully (OnStarted)");
            //});

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

            //hubConnectionAccount.On<string, string, string>("AvatarUpload", async (appId, phoneNumber, imgUrl) =>
            //{
            //    logger.LogInformation($"-- {appId} requesting AvatarUpload. for {phoneNumber}.");

            //    //var authedAccount = authService.Authenticate(phoneNumber);
            //    //if (authedAccount != null)
            //    //{
            //    //    logger.LogInformation($"-- {phoneNumber} authenticated (Success). " +
            //    //        $"Proceeding to avatar upload.");

            //    //    authedAccount.AvatarImageUrl = imgUrl;

            //    //    AccountDto accountResponse = await accountService.UpdateAvatar(authedAccount);
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
