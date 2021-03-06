﻿using System;
using System.Collections.Generic;
using Gallery.MQ.Abstraction;
using RabbitMQ.Client;

namespace Gallery.RMQ.Implementation
{
    public class InitializerRMQ : InitializerMQ
    {
        private readonly string _connectionString;

        public InitializerRMQ(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
        public override void Initializer(Dictionary<string, string> queues)
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(_connectionString)
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                foreach (var queueName in queues)
                {
                    channel.QueueDeclare(queue: queueName.Value,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                }
                
            }
        }
    }
}
