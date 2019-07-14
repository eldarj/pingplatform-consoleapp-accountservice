using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AccountMicroservice.MessageBus.Publishers.Interfaces;
using AccountMicroservice.Data.Services;
using Ping.Commons.Dtos.Models.Wrappers.Response;
using Ping.Commons.Dtos.Models.Auth;
using Ping.Commons.Settings;
using Api.DtoModels.Auth;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading;
using System;
using AccountMicroservice.SignalRServices.Interfaces;

namespace AccountMicroservice.HostedServices
{
    public class ConsoleHostedService : IHostedService
    {
        private readonly ILogger logger;
        private readonly IApplicationLifetime appLifetime;

        private readonly IAuthHubClientService authHubClient;
        private readonly IAccountHubClientService accountHubClient;

        public ConsoleHostedService(
            IApplicationLifetime applicationLifetime,
            ILogger<ConsoleHostedService> logger,
            IAccountHubClientService accountHubClient,
            IAuthHubClientService authHubClient)
        {
            this.authHubClient = authHubClient;
            this.accountHubClient = accountHubClient;

            this.logger = logger;
            this.appLifetime = applicationLifetime;
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

        private void OnStarted()
        {
            logger.LogInformation("Starting AccountMicroservice (OnStarted)");

            try
            {
                // Connect to hubs
                authHubClient.Connect();
                accountHubClient.Connect();
            }
            catch (Exception e)
            {
                logger.LogInformation("AccountMicroservice couldn't be started (OnStarted)");
                return;
            }
        }

        private void OnStopping()
        {
            logger.LogInformation("Stopping AccountMicroservice (OnStopping)");
        }

        private void OnStopped()
        {
            logger.LogInformation("AccountMicroservice stopped (OnStopped)");
        }
    }
}
