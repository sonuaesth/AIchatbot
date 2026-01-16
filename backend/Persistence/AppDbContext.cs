using AiChat.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Backend.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Chat>(e =>
        {
            e.ToTable("Chat");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.CreatedAt).IsRequired();
            e.HasMany(x => x.Messages)
                .WithOne(x => x.Chat!)
                .HasForeignKey(x => x.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<Message>(e =>
        {
            e.ToTable("Message");
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).IsRequired();
            e.Property(x => x.CreatedAt).IsRequired();
            e.Property(x => x.Role)
                .HasConversion<string>()
                .HasMaxLength(20);
            e.HasIndex(x => x.ChatId);
            e.HasIndex(x => x.CreatedAt);
        });
    }
}
