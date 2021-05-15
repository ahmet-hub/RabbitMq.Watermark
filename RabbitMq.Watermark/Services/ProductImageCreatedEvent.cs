using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMq.Watermark.Services
{
    public class ProductImageCreatedEvent
    {
        public string ImageName { get; set; }
    }
}
