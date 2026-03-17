using DatingApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<DatingappUser>
    {
        public DbSet<Compatibility> Compatibilities { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationMember> ConversationMembers { get; set; }
        public DbSet<Hobby> Hobbies { get; set; }
        public DbSet<HobbyUser> HobbyUsers { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MeetingAttendance> MeetingAttendances { get; set; }
        public DbSet<Message> Messages { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HobbyUser>()
                .HasKey(hu => new { hu.UserId, hu.HobbyId });

            modelBuilder.Entity<MeetingAttendance>()
                .HasOne(ma => ma.Meeting)
                .WithMany(m => m.MeetingAttendances)
                .HasForeignKey(ma => ma.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MeetingAttendance>()
                .HasOne(ma => ma.User)
                .WithMany(u => u.MeetingAttendances)
                .HasForeignKey(ma => ma.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Meeting>()
                .HasOne(m => m.Location)
                .WithMany(l => l.Meetings)
                .HasForeignKey(m => m.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HobbyUser>()
                .HasOne(hu => hu.Hobby)
                .WithMany(h => h.HobbyUsers)
                .HasForeignKey(hu => hu.HobbyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HobbyUser>()
                .HasOne(hu => hu.User)
                .WithMany(u => u.HobbyUsers)
                .HasForeignKey(hu => hu.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConversationMember>()
                .HasOne(cm => cm.Conversation)
                .WithMany(c => c.ConversationMembers)
                .HasForeignKey(cm => cm.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConversationMember>()
                .HasOne(cm => cm.User)
                .WithMany(u => u.ConversationMembers)
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConversationMember>()
                .HasIndex(cm => new { cm.ConversationId, cm.UserId })
                .IsUnique();

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compatibility>()
                .HasOne(c => c.UserA)
                .WithMany(u => u.CompatibilitiesAsUserA)
                .HasForeignKey(c => c.UserId_A)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Compatibility>()
                .HasOne(c => c.UserB)
                .WithMany(u => u.CompatibilitiesAsUserB)
                .HasForeignKey(c => c.UserId_B)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void SetDates()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = DateTime.UtcNow;

                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
