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
    public class AuthHubClientService : BaseHubClientService, IAuthHubClientService
    {
        private static readonly string HUB_ENDPOINT = "authhub";

        private readonly ILogger logger;
        private readonly IAuthService authService;
        private readonly IAccountService accountService;
        private readonly IAccountMQPublisher accountMQPublisher;

        public AuthHubClientService(IOptions<GatewayBaseSettings> gatewayBaseOptions, 
            IOptions<SecuritySettings> securityOptions,
            IAccountService accountService,
            IAuthService authService,
            IAccountMQPublisher accountMQPublisher,
            ILogger<AuthHubClientService> logger)
            : base(gatewayBaseOptions, securityOptions, HUB_ENDPOINT)
        {
            this.logger = logger;

            this.accountMQPublisher = accountMQPublisher;

            this.authService = authService;
            this.accountService = accountService;
        }

        public async void Connect()
        {
            await hubConnection.StartAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    logger.LogInformation("-- Couln't connect to SignalR AuthHub (OnStarted)");
                    return;
                }
                logger.LogInformation("AccountMicroservice connected to AuthHub successfully (OnStarted)");
                RegisterHandlers();
            });
        }

        public void RegisterHandlers()
        {
            base.hubConnection.On<string>("RequestCallingCodes", async (string appIdentifier) =>
            {
                logger.LogInformation($"[{appIdentifier}] requesting CallingCodes.");

                List<CallingCodeDto> callingCodes = await accountService.GetCallingCodes();
                if (callingCodes != null)
                {
                    logger.LogInformation($"-- {appIdentifier} requesting CallingCodes (SUCCESS) - Returning data to hub.");

                    await base.hubConnection.SendAsync("ResponseCallingCodes", appIdentifier, callingCodes);
                    return;
                }

                logger.LogInformation($"[{appIdentifier}] requesting CallingCodes (FAIL). ");
            });

            hubConnection.On<string, AccountDto>("RequestAuthentication", async (appIdentifier, accountRequest) =>
            {
                logger.LogInformation($"[{appIdentifier}] - {accountRequest.PhoneNumber} requesting auth.");

                var authedAccount = authService.Authenticate(accountRequest, base.securitySettings.Secret);
                if (authedAccount != null)
                {
                    logger.LogInformation($"[{appIdentifier}] - {accountRequest.PhoneNumber} authenticated (Success). Sending back data.");
                    await hubConnection.SendAsync("AuthenticationDone", appIdentifier, authedAccount);
                }
                else
                {
                    logger.LogInformation($"[{appIdentifier}] - {accountRequest.PhoneNumber} did not authenticate (Fail). Sending back error message.");
                    await hubConnection.SendAsync("AuthenticationFailed", appIdentifier,
                        new ResponseDto<AccountDto> { Dto = accountRequest, Message = "Authentication failed.", MessageCode = "401" });
                }
            });

            hubConnection.On<string, AccountDto>("RequestRegistration", async (appIdentifier, accountRequest) =>
            {
                logger.LogInformation($"[{appIdentifier}] - {accountRequest.PhoneNumber} requesting registration.");

                AccountDto newAccount = await authService.Registration(accountRequest);
                if (newAccount != null)
                {
                    // Log to microservice log
                    logger.LogInformation($"[{appIdentifier}] - {accountRequest.PhoneNumber} registered (Success). Sending back data.");

                    // TODO: Sent new registered account (message) to MQ
                    accountMQPublisher.SendCreatedAccount(newAccount);

                    // Send signalR message (trigger any MQ consumer and consumer-apps)
                    await hubConnection.SendAsync("RegistrationDone", appIdentifier, newAccount);
                }
                else
                {
                    logger.LogWarning($"[{appIdentifier}] - {accountRequest.PhoneNumber} did not register (Fail). Sending back error message.");
                    await hubConnection.SendAsync("RegistrationFailed", appIdentifier, $"Account registration failed for: {appIdentifier}");
                }
            });
        }
    }
}
