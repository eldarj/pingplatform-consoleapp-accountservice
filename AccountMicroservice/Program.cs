using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using AccountMicroservice.Data;
using Microsoft.EntityFrameworkCore;

using AccountMicroservice.SignalR.ClientServices;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;

using AccountMicroservice.Data.Services;
using AccountMicroservice.Data.Services.Impl;
using AccountMicroservice.MessageBus.Publishers;
using AccountMicroservice.MessageBus.Publishers.Interfaces;

namespace AccountMicroservice
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
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
                    //services.AddDbContextPool<MyDbContext>(options =>
                    //    options.UseMySql(hostContext.Configuration.GetConnectionString("MysqlAccountMicroservice"), b =>
                    //        b.MigrationsAssembly("AccountMicroservice.Data"))
                    //);
                    services.AddDbContext<MyDbContext>();

                    var root = Directory.GetCurrentDirectory();
                    IFileProvider physicalProvider = new PhysicalFileProvider(root);
                    services.AddSingleton<IFileProvider>(physicalProvider);

                    services.AddSignalR();

                    services.AddHostedService<SignalRClientService>();

                    services.AddScoped<IAuthService, AuthService>();
                    services.AddScoped<IAccountService, AccountService>();

                    services.AddScoped<IAccountMQPublisher, AccountMQPublisher>();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConsole();
                    configLogging.AddDebug();
                })
                .UseConsoleLifetime()
                .Build();
            
            Console.WriteLine("::Account Microservice::");
            await host.RunAsync();
        }
    }
}
