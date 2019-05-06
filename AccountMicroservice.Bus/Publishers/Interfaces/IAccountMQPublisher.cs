using System;
using System.Collections.Generic;
using System.Text;

namespace AccountMicroservice.MessageBus.Publishers.Interfaces
{
    public interface IAccountMQPublisher
    {
        void CreateConnection();

        void Close();

        void SendCreatedAccount<T>(T account);
    }
}
