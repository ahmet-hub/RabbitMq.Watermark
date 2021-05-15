using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitMq.Watermark.Models;

namespace RabbitMq.Watermark.Models
{
    public class AppContext:DbContext
    {
        public AppContext(DbContextOptions<AppContext> options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }

        public DbSet<RabbitMq.Watermark.Models.Picture> Picture { get; set; }

    }
}
