using AccountMicroservice.Data.Services;
using AccountMicroservice.Data;
using Api.DtoModels.Auth;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Threading.Channels;
using AccountMicroservice.MessageBus.Publishers.Interfaces;
using Ping.Commons.Dtos.Models.Auth;

namespace AccountMicroservice.SignalR.ClientServices
{
    public class SignalRClientService : IHostedService
    {
        private readonly ILogger logger;
        private readonly IApplicationLifetime appLifetime;
        private readonly IAuthService authService;
        private readonly IAccountService accountService;

        private readonly HubConnection hubConnectionAuth;
        private readonly HubConnection hubConnectionAccount;

        private readonly IFileProvider fileProvider;

        private readonly IAccountMQPublisher accountMQPublisher;

        public SignalRClientService(
            ILogger<SignalRClientService> logger,
            IApplicationLifetime applicationLifetime,
            IAuthService authService,
            IAccountService accountService,
            IFileProvider fileProvider,
            IAccountMQPublisher accountMQPublisher)
        {
            this.accountService = accountService;
            this.authService = authService;
            this.logger = logger;
            this.appLifetime = applicationLifetime;
            this.fileProvider = fileProvider;
            this.accountMQPublisher = accountMQPublisher;

            // Setup SignalR Hub connection
            hubConnectionAuth = new HubConnectionBuilder()
                .WithUrl("https://localhost:44380/authhub?groupName=accountMicroservice")
                .Build();

            // TODO: Create a wrapper/helper service for handling hubConnections
            hubConnectionAccount = new HubConnectionBuilder()
                .WithUrl("https://localhost:44380/accounthub?groupName=accountMicroservice")
                .Build();
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async void OnStarted()
        {
            logger.LogInformation("Starting AccountMicroservice (OnStarted)");

            // Connect to hub
            try
            {
                await hubConnectionAuth.StartAsync().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        logger.LogInformation("-- Couln't connect to signalR AuthHub (OnStarted)");
                        return;
                    }
                    logger.LogInformation("AccountMicroservice connected to AuthHub successfully (OnStarted)");
                });

                await hubConnectionAccount.StartAsync().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        logger.LogInformation("-- Couln't connect to signalR AccountHub (OnStarted)");
                        return;
                    }
                    logger.LogInformation("AccountMicroservice connected to AccountHub successfully (OnStarted)");
                });

                // TODO: Remove this here and from the hub
                IFileInfo meta = fileProvider.GetFileInfo("files/file.txt");
                if (meta.Exists)
                {
                    using(var fs = meta.CreateReadStream())
                    {
                        byte[] readBytes = File.ReadAllBytes(meta.PhysicalPath);
                        await hubConnectionAuth.SendAsync("FileReceivedTest", "test");
                    }
                }


                hubConnectionAccount.On<string, string, string>("CoverUpload", async (appId, phoneNumber, imgUrl) =>
                {
                    logger.LogInformation($"-- {appId} requesting CoverUpload. for {phoneNumber}.");

                    var authedAccount = authService.Authenticate(phoneNumber);
                    if (authedAccount != null)
                    {
                        logger.LogInformation($"-- {phoneNumber} authenticated (Success). " +
                            $"Proceeding to cover upload.");

                        authedAccount.CoverImageUrl = imgUrl;

                        AccountDto accountResponse = await accountService.UpdateCover(authedAccount);
                        if (accountResponse != null)
                        {
                            await hubConnectionAccount.SendAsync("UpdateProfileSuccess", appId, accountResponse);
                            return;
                        }
                    }

                    logger.LogInformation($"-- {phoneNumber} did not authenticate (Fail). " +
                        $"Requested by: {appId} - sending back error message.");
                    await hubConnectionAccount.SendAsync("UpdateProfileFailed", appId, $"Authentication failed for: {appId}");
                });

                hubConnectionAccount.On<string, string, string>("AvatarUpload", async (appId, phoneNumber, imgUrl) =>
                {
                    logger.LogInformation($"-- {appId} requesting AvatarUpload. for {phoneNumber}.");

                    var authedAccount = authService.Authenticate(phoneNumber);
                    if (authedAccount != null)
                    {
                        logger.LogInformation($"-- {phoneNumber} authenticated (Success). " +
                            $"Proceeding to avatar upload.");

                        authedAccount.AvatarImageUrl = imgUrl;

                        AccountDto accountResponse = await accountService.UpdateAvatar(authedAccount);
                        if (accountResponse != null)
                        {
                            await hubConnectionAccount.SendAsync("UpdateProfileSuccess", appId, accountResponse);
                            return;
                        }
                    }

                    logger.LogInformation($"-- {phoneNumber} did not authenticate (Fail). " +
                        $"Requested by: {appId} - sending back error message.");
                    await hubConnectionAccount.SendAsync("UpdateProfileFailed", appId, $"Authentication failed for: {appId}");
                });

                hubConnectionAccount.On<string, AccountDto>("UpdateProfile", async (appId, accountRequest) =>
                {
                    logger.LogInformation($"-- {appId} requesting ProfileUpdate. for {accountRequest.PhoneNumber}.");

                    var authedAccount = authService.Authenticate(accountRequest.PhoneNumber);
                    if (authedAccount != null)
                    {
                        logger.LogInformation($"-- {accountRequest.PhoneNumber} authenticated (Success). " +
                            $"Proceeding to profile update.");

                        AccountDto accountResponse = await accountService.Update(accountRequest);
                        if (accountResponse != null)
                        {
                            await hubConnectionAccount.SendAsync("UpdateProfileSuccess", appId, accountResponse);
                            return;
                        }
                    }

                    logger.LogInformation($"-- {accountRequest.PhoneNumber} did not authenticate (Fail). " +
                        $"Requested by: {appId} - sending back error message.");
                    await hubConnectionAccount.SendAsync("UpdateProfileFailed", appId, $"Authentication failed for: {appId}");
                });

                hubConnectionAuth.On<string>("RequestCallingCodes", async (appId) =>
                {
                    logger.LogInformation($"-- {appId} requesting CallingCodes.");

                    List<CallingCodeDto> callingCodes = await accountService.GetCallingCodes();
                    if (callingCodes != null)
                    {
                        logger.LogInformation($"-- {appId} requesting CallingCodes (Success). " +
                            $"Returning data to hub.");

                        await hubConnectionAuth.SendAsync("ResponseCallingCodes", appId, callingCodes);
                        return;
                    }

                    logger.LogInformation($"-- Couldn't fetch list of calling codes from db.");
                });

                hubConnectionAuth.On<string, AccountLoginDto>("RequestAuthentication", async (appId, accountRequest) =>
                {
                    logger.LogInformation($"-- {appId} requesting auth. for {accountRequest.PhoneNumber}.");

                    var authedAccount = authService.Authenticate(accountRequest);
                    if (authedAccount != null)
                    {
                        logger.LogInformation($"-- {accountRequest.PhoneNumber} authenticated (Success). " +
                            $"Requested by: {appId} - sending back data.");
                        await hubConnectionAuth.SendAsync("AuthenticationDone", appId, authedAccount);
                    }
                    else
                    {
                        logger.LogInformation($"-- {accountRequest.PhoneNumber} did not authenticate (Fail). " +
                            $"Requested by: {appId} - sending back error message.");
                        await hubConnectionAuth.SendAsync("AuthenticationFailed", appId, $"Authentication failed for: {appId}");
                    }
                });

                hubConnectionAuth.On<string, AccountRegisterDto>("RequestRegistration", async (appId, accountRequest) =>
                {
                    logger.LogInformation($"-- {appId} requesting registration for {accountRequest.PhoneNumber}.");

                    AccountDto newAccount = await authService.Registration(accountRequest);
                    if (newAccount != null)
                    {
                        // Log to microservice log
                        logger.LogInformation($"-- {accountRequest.PhoneNumber} registered (Success). " +
                            $"Requested by: {appId} - sending back data.");

                        // TODO: Sent new registered account (message) to MQ
                        accountMQPublisher.SendCreatedAccount(newAccount);

                        // Send signalR message (trigger any MQ consumer and consumer-apps)
                        await hubConnectionAuth.SendAsync("RegistrationDone", appId, newAccount);
                    }
                    else
                    {
                        logger.LogWarning($"-- {accountRequest.PhoneNumber} did not register - " +
                            $"account with same phonenumber already exists(Fail). " +
                            $"Requested by: {appId} - sending back error message.");

                        await hubConnectionAuth.SendAsync("RegistrationFailed", appId, $"Account registration failed for: {appId}");
                    }
                });
            }
            catch (Exception e)
            {
                logger.LogInformation("AccountMicroservice couldn't be started (OnStarted)");
                return;
            }
            // Perform on-started activites here
        }

        private void OnStopping()
        {
            logger.LogInformation("Stopping AccountMicroservice (OnStopping)");
            // Perform on-stopping activities here
        }

        private void OnStopped()
        {
            logger.LogInformation("AccountMicroservice stopped (OnStopped)");
            // Perform post-stopped activities here
        }

        private async Task WriteItems(ChannelWriter<int> writer, int count, int delay)
        {
            for (var i = 0; i < count; i++)
            {
                await writer.WriteAsync(i);
                await Task.Delay(delay);
            }

            writer.TryComplete();
        }
    }
}
