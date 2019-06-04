using AccountMicroservice.Settings;
using Microsoft.Extensions.Options;
using Ping.Commons.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using Ping.Commons.Dtos.Models.Auth;
using AccountMicroservice.Data.Services;
using Microsoft.Extensions.Logging;
using AccountMicroservice.SignalRServices.Interfaces;
using AccountMicroservice.SignalRServices.Base;

namespace AccountMicroservice.SignalRServices
{
    public class AuthHubClientService : BaseHubClientService, IAuthHubClientService
    {
        private static readonly string HUB_ENDPOINT = "authhub";

        private readonly ILogger logger;
        private readonly IAccountService accountService;
        //private readonly IAccountMQPublisher accountMQPublisher;

        public AuthHubClientService(IOptions<GatewayBaseSettings> gatewayBaseOptions, 
            IOptions<SecuritySettings> securityOptions,
            IAccountService accountService,
            ILogger<AuthHubClientService> logger)
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
                    logger.LogInformation("-- Couln't connect to signalR AuthHub (OnStarted)");
                    return;
                }
                logger.LogInformation("AccountMicroservice connected to AuthHub successfully (OnStarted)");

                RegisterHandlers();
            });
        }

        public void RegisterHandlers()
        {
            hubConnection.On<string>("RequestCallingCodes", async (appIdentifier) =>
            {
                logger.LogInformation($"[{appIdentifier}] requesting CallingCodes.");

                List<CallingCodeDto> callingCodes = await accountService.GetCallingCodes();
                if (callingCodes != null)
                {
                    logger.LogInformation($"-- {appIdentifier} requesting CallingCodes (SUCCESS) - Returning data to hub.");

                    await hubConnection.SendAsync("ResponseCallingCodes", appIdentifier, callingCodes);
                    return;
                }

                logger.LogInformation($"[{appIdentifier}] requesting CallingCodes (FAIL). ");
            });

            //hubConnectionAuth.On<AccountDto>("RequestAuthentication", async (accountRequest) =>
            //{
            //    logger.LogInformation($"-- {accountRequest.PhoneNumber} requesting auth.");

            //    var authedAccount = authService.Authenticate(accountRequest, securitySettings.Secret);
            //    if (authedAccount != null)
            //    {
            //        logger.LogInformation($"-- {accountRequest.PhoneNumber} authenticated (Success). Sending back data.");
            //        await hubConnectionAuth.SendAsync("AuthenticationDone", authedAccount);
            //    }
            //    else
            //    {
            //        logger.LogInformation($"-- {accountRequest.PhoneNumber} did not authenticate (Fail). Sending back error message.");
            //        await hubConnectionAuth.SendAsync("AuthenticationFailed", 
            //            new ResponseDto<AccountDto> { Dto = accountRequest, Message = "Authentication failed.", MessageCode = "401" });
            //    }
            //});

            //hubConnectionAuth.On<string, AccountDto>("RequestRegistration", async (appId, accountRequest) =>
            //{
            //    logger.LogInformation($"-- {appId} requesting registration for {accountRequest.PhoneNumber}.");

            //    AccountDto newAccount = await authService.Registration(accountRequest);
            //    if (newAccount != null)
            //    {
            //        // Log to microservice log
            //        logger.LogInformation($"-- {accountRequest.PhoneNumber} registered (Success). " +
            //            $"Requested by: {appId} - sending back data.");

            //        // TODO: Sent new registered account (message) to MQ
            //        accountMQPublisher.SendCreatedAccount(newAccount);

            //        // Send signalR message (trigger any MQ consumer and consumer-apps)
            //        await hubConnectionAuth.SendAsync("RegistrationDone", appId, newAccount);
            //    }
            //    else
            //    {
            //        logger.LogWarning($"-- {accountRequest.PhoneNumber} did not register - " +
            //            $"account with same phonenumber already exists(Fail). " +
            //            $"Requested by: {appId} - sending back error message.");

            //        await hubConnectionAuth.SendAsync("RegistrationFailed", appId, $"Account registration failed for: {appId}");
            //    }
            //});
        }
    }
}
