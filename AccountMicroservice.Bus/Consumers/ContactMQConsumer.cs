using AccountMicroservice.Data;
using AccountMicroservice.Data.Services;
using AccountMicroservice.MessageBus.Consumers.Interfaces;
using AccountMicroservice.MessageBus.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ping.Commons.Dtos.Models.Chat;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AccountMicroservice.MessageBus.Consumers
{
    public class ContactMQConsumer : IHostedService, IContactMQConsumer
    {
        private readonly string ExchangeType = "fanout";
        private readonly string ExchangeName = "CreateContact_FanoutExchange";
        private readonly string QueueName = "AccountMicroservice_CreateContact_Queue";

        private static ConnectionFactory connectionFactory;
        private static IConnection connection;
        private static IModel channel;
        private static EventingBasicConsumer consumer;

        private readonly MyDbContext dbContext;
        private readonly IAccountService accountService;
        private readonly ILogger logger;

        public ContactMQConsumer(MyDbContext dbContext, IAccountService accountService, ILogger<ContactMQConsumer> logger)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.accountService = accountService;
        }

        public Task StartAsync(CancellationToken cancellationToken) => CreateConnection();
        public Task StopAsync(CancellationToken cancellationToken) => Close();

        public Task CreateConnection()
        {
            return Task.Run(() =>
            {
                Console.WriteLine("- Creating connection to RabbitMQ...");
                connectionFactory = new ConnectionFactory
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest"
                };

                connection = connectionFactory.CreateConnection();
                channel = connection.CreateModel();

                // Define and bind the exchange and the queue
                channel.ExchangeDeclare(exchange: ExchangeName,
                    type: ExchangeType,
                    durable: true,
                    autoDelete: false,
                    arguments: null);

                channel.QueueDeclare(queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                channel.QueueBind(queue: QueueName,
                    exchange: ExchangeName,
                    routingKey: "");

                // Bind a consumer with our OnDeliveryReceived handler
                consumer = new EventingBasicConsumer(channel);
                consumer.Received += OnDeliveryReceived;

                // Start basic consuming on our channel, with auto-acknowledgement
                channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);
            });
        }

        public async void OnDeliveryReceived(object model, BasicDeliverEventArgs delivery)
        {
            Console.WriteLine($"Consuming data from RabbitMQ. Exchange: {ExchangeName} - Qeueue: {QueueName}");
            Console.WriteLine("------------------------------------------");

            var messageBody = delivery.Body;
            var contactDto = (ContactDto)messageBody.Deserialize(typeof(ContactDto));

            Console.WriteLine(" [x] RABBITMQ INFO: [Created New Contact] - Message received from exchange/queue [{0}/{1}], data: {2}",
                ExchangeName,
                QueueName,
                Encoding.UTF8.GetString(messageBody));

            if (await accountService.AddContact(contactDto))
            {
                logger.LogInformation($"-- Contact: {contactDto.ContactPhoneNumber} saved to db, for account: {contactDto.PhoneNumber}. ");
            }
            else
            {
                logger.LogError($"-- Couldn't save contact: {contactDto.ContactPhoneNumber} to db, for account: {contactDto.PhoneNumber}. ");
            }
        }

        public Task Close()
        {
            connection.Close();
            return Task.CompletedTask;
        }
    }
}
