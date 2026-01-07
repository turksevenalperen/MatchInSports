using Microsoft.EntityFrameworkCore;
using MatchISportsAPI.Models;

namespace MatchISportsAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<MatchRequest> MatchRequests { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<TeamRequest> TeamRequests { get; set; }
    public DbSet<MatchHistory> MatchHistories { get; set; }
    public DbSet<Activity> Activities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.HasMany(u => u.CreatedMatches)
                .WithOne(m => m.Creator)
                .HasForeignKey(m => m.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.MatchRequests)
                .WithOne(mr => mr.Requester)
                .HasForeignKey(mr => mr.RequesterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.SentTeamRequests)
                .WithOne(tr => tr.Sender)
                .HasForeignKey(tr => tr.SenderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.ReceivedTeamRequests)
                .WithOne(tr => tr.Receiver)
                .HasForeignKey(tr => tr.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.SentMessages)
                .WithOne(m => m.Sender)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.SentChatMessages)
                .WithOne(cm => cm.Sender)
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.ReceivedChatMessages)
                .WithOne(cm => cm.Receiver)
                .HasForeignKey(cm => cm.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.MatchHistory1)
                .WithOne(mh => mh.Team1)
                .HasForeignKey(mh => mh.Team1Id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.MatchHistory2)
                .WithOne(mh => mh.Team2)
                .HasForeignKey(mh => mh.Team2Id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Activities)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Match configuration
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasMany(m => m.Requests)
                .WithOne(mr => mr.Match)
                .HasForeignKey(mr => mr.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Messages)
                .WithOne(msg => msg.Match)
                .HasForeignKey(msg => msg.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MatchRequest configuration
        modelBuilder.Entity<MatchRequest>(entity =>
        {
            entity.HasIndex(e => new { e.MatchId, e.RequesterId }).IsUnique();
        });

        // Convert enums to strings for PostgreSQL
        modelBuilder.Entity<User>()
            .Property(u => u.Sport)
            .HasConversion<string>();

        modelBuilder.Entity<Match>()
            .Property(m => m.Sport)
            .HasConversion<string>();

        modelBuilder.Entity<Match>()
            .Property(m => m.Status)
            .HasConversion<string>();

        modelBuilder.Entity<MatchRequest>()
            .Property(mr => mr.Status)
            .HasConversion<string>();

        modelBuilder.Entity<TeamRequest>()
            .Property(tr => tr.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Activity>()
            .Property(a => a.Type)
            .HasConversion<string>();
    }
}
