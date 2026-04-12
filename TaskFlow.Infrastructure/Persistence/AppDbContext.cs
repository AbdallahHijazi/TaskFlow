using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public AppDbContext()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TaskFlowDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
            }
        }
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Status> Statuses => Set<Status>();
        public DbSet<DependencyType> DependencyTypes => Set<DependencyType>();
        public DbSet<Image> Images => Set<Image>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Initiative> Initiatives => Set<Initiative>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<TaskDependency> TaskDependencies => Set<TaskDependency>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

    }
}
