using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMq.Watermark.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMq.Watermark.BackgroundServices
{
    public class ImageWatermarkBackgroundService : BackgroundService
    {
        private readonly RabbitMqClientService _rabbitMqClientService;
        private readonly ILogger<ImageWatermarkBackgroundService> _logger;
        private IModel _channel;
        public ImageWatermarkBackgroundService(RabbitMqClientService rabbitMqClientService, ILogger<ImageWatermarkBackgroundService> logger)
        {
            _rabbitMqClientService = rabbitMqClientService;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMqClientService.Connection();
            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            _channel.BasicConsume(RabbitMqClientService.QueueName, false, consumer);

            consumer.Received += Consumer_Received;

            return Task.CompletedTask;



        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {

            try
            {
                var imageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images",
                    imageCreatedEvent.ImageName);
                var text = "ahmet yardimci";

                using var image = Image.FromFile(path);

                using var graphic = Graphics.FromImage(image);

                var font = new Font(FontFamily.GenericMonospace, 32, FontStyle.Bold, GraphicsUnit.Pixel);

                var textSize = graphic.MeasureString(text, font);

                var color = Color.Red;

                var brush = new SolidBrush(color);

                var position = new Point(image.Width - ((int)textSize.Width + 30), image.Height - ((int)textSize.Height + 30));

                graphic.DrawString(text, font, brush, position);

                image.Save("wwwroot/Images" + imageCreatedEvent.ImageName);
                image.Dispose();
                graphic.Dispose();

                _channel.BasicAck(@event.DeliveryTag, false);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.Message);
                throw new Exception(e.Message);
            }

            return Task.CompletedTask;

        }
    }
}
