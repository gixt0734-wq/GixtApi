using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GixtApiBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GixtApiBackend.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> users { get; set; } = null!;
        public DbSet<Worker> workers { get; set; } = null!;
        public DbSet<Session> sessions { get; set; } = null!;
        public DbSet<Advertisement> advertisements { get; set; } = null!;
        public DbSet<Category> categories { get; set; } = null!;
        public DbSet<Service> services { get; set; } = null!;
        public DbSet<Service_image> serviceimages { get; set; } = null!;
        public DbSet<Favorite> favorites { get; set; } = null!;

    }

}
