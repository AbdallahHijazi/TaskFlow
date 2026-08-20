using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        private readonly ICurrentUserService currentUser;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUser) : base(options)
        {
            this.currentUser = currentUser;
        }
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Status> Statuses => Set<Status>();
        public DbSet<DependencyType> DependencyTypes => Set<DependencyType>();
        public DbSet<Image> Images => Set<Image>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Initiative> Initiatives => Set<Initiative>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<TaskDependency> TaskDependencies => Set<TaskDependency>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public Guid? CurrentUserId { get; set; }
        private Guid? CurrentClientId => currentUser?.ClientId;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            modelBuilder.Entity<Initiative>().HasQueryFilter(entity => entity.ClientId == CurrentClientId);
            modelBuilder.Entity<TaskItem>().HasQueryFilter(entity => entity.ClientId == CurrentClientId);
            modelBuilder.Entity<Status>().HasQueryFilter(entity => entity.ClientId == CurrentClientId);
            modelBuilder.Entity<DependencyType>().HasQueryFilter(entity => entity.ClientId == CurrentClientId);
            modelBuilder.Entity<Comment>().HasQueryFilter(entity => entity.ClientId == CurrentClientId);
            modelBuilder.Entity<TaskDependency>().HasQueryFilter(entity => entity.ClientId == CurrentClientId);
            modelBuilder.Entity<Image>().HasQueryFilter(entity => entity.ClientId == CurrentClientId);

            modelBuilder.Entity<Initiative>().HasOne(entity => entity.Client).WithMany(client => client.Initiatives).HasForeignKey(entity => entity.ClientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TaskItem>().HasOne(entity => entity.Client).WithMany(client => client.Tasks).HasForeignKey(entity => entity.ClientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Status>().HasOne(entity => entity.Client).WithMany(client => client.Statuses).HasForeignKey(entity => entity.ClientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DependencyType>().HasOne(entity => entity.Client).WithMany(client => client.DependencyTypes).HasForeignKey(entity => entity.ClientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Comment>().HasOne(entity => entity.Client).WithMany(client => client.Comments).HasForeignKey(entity => entity.ClientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TaskDependency>().HasOne(entity => entity.Client).WithMany(client => client.TaskDependencies).HasForeignKey(entity => entity.ClientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Image>().HasOne(entity => entity.Client).WithMany(client => client.Images).HasForeignKey(entity => entity.ClientId).OnDelete(DeleteBehavior.Restrict);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUser?.UserId ?? Guid.Empty;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = currentUser?.UserId ?? Guid.Empty;
                }
            }

            var clientId = CurrentClientId;
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>().Where(entry => entry.State == EntityState.Added))
            {
                if (entry.Entity.ClientId == Guid.Empty)
                {
                    entry.Entity.ClientId = clientId
                        ?? throw new InvalidOperationException("A client context is required to create this record.");
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

    }
}
