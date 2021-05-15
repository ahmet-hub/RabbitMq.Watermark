using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RabbitMq.Watermark.Services
{
    public class RabbitMqPublisher
    {
        private readonly RabbitMqClientService _rabbitMqClientService;

        public RabbitMqPublisher(RabbitMqClientService rabbitMqClientService)
        {
            _rabbitMqClientService = rabbitMqClientService;
        }

        public void Publish(PictureCreatedEvent ImageEvent)
        {
            var channel = _rabbitMqClientService.Connection();
            var bodyString = JsonSerializer.Serialize(ImageEvent);

            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: RabbitMqClientService.ExchangeName, 
                routingKey: RabbitMqClientService.RoutingWatermark, 
                basicProperties: properties, body: bodyByte);
        }
    }
}
