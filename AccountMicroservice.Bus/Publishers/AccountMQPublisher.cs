using AccountMicroservice.MessageBus.Publishers.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using AccountMicroservice.MessageBus.Utils;

namespace AccountMicroservice.MessageBus.Publishers
{
    public class AccountMQPublisher : IAccountMQPublisher
    {
        private static ConnectionFactory connectionFactory;
        private static IConnection connection;
        private static IModel channel;

        private const string ExchangeType = "fanout";
        private const string ExchangeName = "RegisterAccount_FanoutExchange";
        private readonly string ChatQueueName = "ChatMicroservice_RegisterAccount_Queue";
        private readonly string DataSpaceQueueName = "DataSpaceMicroservice_RegisterAccount_Queue";

        public AccountMQPublisher()
        {
            CreateConnection();
        }

        public void Close()
        {
            connection.Close();
        }

        public void CreateConnection()
        {
            connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: ExchangeName, 
                type: ExchangeType, 
                durable: true, 
                autoDelete: false, 
                arguments: null);

            channel.QueueDeclare(queue: ChatQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.QueueBind(queue: ChatQueueName,
                exchange: ExchangeName,
                routingKey: "");

            channel.QueueDeclare(queue: DataSpaceQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.QueueBind(queue: DataSpaceQueueName,
                exchange: ExchangeName,
                routingKey: "");
        }

        public void SendCreatedAccount<T>(T account)
        {
            var serializedAccount = account.Serialize();

            channel.BasicPublish(exchange: ExchangeName, 
                routingKey: "", 
                basicProperties: null, 
                body: serializedAccount);

            Console.WriteLine("RABBITMQ INFO: [Account Registered] - Message sent to exchange-queue [{0}], data: {1}", 
                ExchangeName, 
                Encoding.Default.GetString(serializedAccount));
        }
    }
}
