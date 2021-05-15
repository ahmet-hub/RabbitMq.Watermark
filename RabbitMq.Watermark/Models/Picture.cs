using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMq.Watermark.Models
{
    public class Picture
    {
        [Key]
        public string Id { get; set; }
        public string Text { get; set; }
        public string FileName { get; set; }
    }
}
