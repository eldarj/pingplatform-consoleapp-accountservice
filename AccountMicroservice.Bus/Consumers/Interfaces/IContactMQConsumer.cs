using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AccountMicroservice.MessageBus.Consumers.Interfaces
{
    public interface IContactMQConsumer
    {
        Task CreateConnection();

        Task Close();

        void OnDeliveryReceived(object model, BasicDeliverEventArgs delivery);
    }
}
