using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using AccountMicroservice.Data;


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using AccountMicroservice.Data.Services;
using AccountMicroservice.Data.Services.Impl;
using AccountMicroservice.MessageBus.Publishers;
using AccountMicroservice.MessageBus.Publishers.Interfaces;
using AccountMicroservice.MessageBus.Consumers;
using Ping.Commons.Settings;
using AccountMicroservice.HostedServices;
using AccountMicroservice.SignalRServices.Interfaces;
using AccountMicroservice.SignalRServices;

namespace AccountMicroservice
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("::Account Microservice::");

            var host = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("hostsettings.json", optional: true);
                    configHost.AddEnvironmentVariables(prefix: "PREFIX_");
                    configHost.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddJsonFile("appsettings.json", optional: false);
                    configApp.AddEnvironmentVariables(prefix: "PREFIX_");
                    configApp.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<MyDbContext>();
                    //services.AddDbContextPool<MyDbContext>(options =>
                    //{
                    //    // TODO: Add this in appsettings or ENV (dev, prod) vars
                    //    options.UseLazyLoadingProxies()
                    //        .UseMySql(hostContext.Configuration.GetConnectionString("MysqlAccountMicroservice"), a =>
                    //            a.MigrationsAssembly("AccountMicroservice.Data"));
                    //});

                    // Jwt authentication// configure strongly typed settings objects
                    services.Configure<SecuritySettings>(hostContext.Configuration.GetSection("SecuritySettings"));
                    services.Configure<GatewayBaseSettings>(hostContext.Configuration.GetSection("GatewayBaseSettings"));

                    services.AddSignalR();

                    services.AddHostedService<ConsoleHostedService>();
                    services.AddHostedService<ContactMQConsumer>();

                    services.AddScoped<IAuthService, AuthService>();
                    services.AddScoped<IAccountService, AccountService>();

                    services.AddScoped<IAuthHubClientService, AuthHubClientService>();
                    services.AddScoped<IAccountHubClientService, AccountHubClientService>();

                    services.AddScoped<IAccountMQPublisher, AccountMQPublisher>();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConsole();
                    configLogging.AddDebug();
                })
                .UseConsoleLifetime()
                .Build();
            
            await host.RunAsync();
        }
    }
}
