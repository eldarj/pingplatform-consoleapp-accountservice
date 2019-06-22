using AccountMicroservice.Settings;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Ping.Commons.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AccountMicroservice.SignalRServices.Base
{
    public abstract class BaseHubClientService
    {
        protected readonly HubConnection hubConnection;

        protected readonly SecuritySettings securitySettings;

        public BaseHubClientService(IOptions<GatewayBaseSettings> gatewayBaseOptions, IOptions<SecuritySettings> securityOptions, string hubEndpoint)
        {
            this.securitySettings = securityOptions.Value;
            GatewayBaseSettings gatewayBaseSettings = gatewayBaseOptions.Value;

            string connectionBaseUrl = gatewayBaseSettings.Scheme + "://" + gatewayBaseSettings.Url + ":" + gatewayBaseSettings.Port;

            string accessToken = JWTokenHandler.GenerateToken(securitySettings.ClientIdentifier, securitySettings.Secret);

            hubConnection = new HubConnectionBuilder()
                .WithUrl(connectionBaseUrl + "/" + hubEndpoint,
                    options => options.AccessTokenProvider = () => Task.FromResult<string>(accessToken))
                .Build();
        }
    }
}
