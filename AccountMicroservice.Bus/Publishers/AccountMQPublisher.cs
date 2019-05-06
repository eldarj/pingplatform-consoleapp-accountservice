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
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static IModel _model;

        private const string ExchangeType = "fanout";
        private const string ExchangeName = "RegisterAccount_FanoutExchange";

        public AccountMQPublisher()
        {
            CreateConnection();
        }

        public void Close()
        {
            _connection.Close();
        }

        public void CreateConnection()
        {
            _factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = _factory.CreateConnection();
            _model = _connection.CreateModel();

            _model.ExchangeDeclare(exchange: ExchangeName, 
                type: ExchangeType, 
                durable: true, 
                autoDelete: false, 
                arguments: null);
        }

        public void SendCreatedAccount<T>(T account)
        {
            var serializedAccount = account.Serialize();

            _model.BasicPublish(exchange: ExchangeName, 
                routingKey: "", 
                basicProperties: null, 
                body: serializedAccount);

            Console.WriteLine("RABBITMQ INFO: [Account Registered] - Message sent to exchange-queue [{0}], data: {1}", 
                ExchangeName, 
                Encoding.Default.GetString(serializedAccount));
        }
    }
}
