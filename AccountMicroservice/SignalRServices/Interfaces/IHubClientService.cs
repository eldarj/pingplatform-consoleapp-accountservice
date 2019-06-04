using System;
using System.Collections.Generic;
using System.Text;

namespace AccountMicroservice.SignalRServices.Interfaces
{
    public interface IHubClientService
    {
        void RegisterHandlers();

        void Connect();
    }
}
