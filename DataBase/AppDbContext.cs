

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{

    public virtual DbSet<MyTask> Tasks{ get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<MyTask>()
            .HasOne(t => t.User)
            .WithMany(u => u.MyTasks)
            .HasForeignKey(t => t.UserId)
            .IsRequired();
    }
}