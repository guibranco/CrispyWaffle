using System.Reflection.Emit;
using CrispyWaffle.BackgroundJobs.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CrispyWaffle.BackgroundJobs.Persistence
{
    /// <summary>
    /// The Entity Framework Core context used by <see cref="EfCoreJobStore"/>.
    /// </summary>
    public class JobDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobDbContext"/> class.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public JobDbContext(DbContextOptions<JobDbContext> options)
            : base(options) { }

        /// <summary>
        /// Gets or sets the stored jobs.
        /// </summary>
        public DbSet<JobEntity> Jobs { get; set; } = null!;

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<JobEntity>(eb =>
            {
                eb.HasKey(j => j.Id);
                eb.Property(j => j.HandlerName).IsRequired().HasMaxLength(256);
                eb.Property(j => j.Payload).IsRequired(false);
                eb.Property(j => j.Priority).HasConversion<int>();
                eb.Property(j => j.Status).HasConversion<int>();
                eb.Property(j => j.CreatedAt).IsRequired();
                eb.Property(j => j.UpdatedAt).IsRequired();
                eb.Property(j => j.MaxAttempt).HasDefaultValue(3);
                eb.Property(j => j.Attempt).HasDefaultValue(0);
            });
        }
    }
}
