using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMq.Watermark.Services
{
    public class RabbitMqClientService:IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMqClientService> _logger;
        private IConnection _connection;
        private IModel _channel;

        public static string ExchangeName = "WatermarkDirectExchange";
        public static string RoutingWatermark = "watermark-route";
        public static string QueueName = "queue-watermark";

        public RabbitMqClientService(ConnectionFactory connectionFactory, ILogger<RabbitMqClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connection()
        {
            _connection = _connectionFactory.CreateConnection();
            
            if(_channel is {IsOpen:true })
            {
                return _channel;
            }

            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, type: "direct", true, false);

            _channel.QueueDeclare(QueueName, true, false,false, null);

            _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingWatermark);

            _logger.LogInformation("Connected RabbitMQ");

            return _channel;
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("Disconnected RabbitMQ");
        }
    }
}
