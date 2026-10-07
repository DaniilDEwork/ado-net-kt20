using Kt20Chat.Models;
using Microsoft.EntityFrameworkCore;

namespace Kt20Chat.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().HasIndex(user => user.NormalizedUserName).IsUnique();
        modelBuilder.Entity<AppUser>().Property(user => user.UserName).HasMaxLength(30);
        modelBuilder.Entity<AppUser>().Property(user => user.NormalizedUserName).HasMaxLength(30);
    }
}
