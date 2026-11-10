using DatingApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace DatingApp.Data
{
    public class DatingAppDbContext : IdentityDbContext<DatingappUser>
    {
        public DbSet<Compatibility> Compatibilities { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationMember> ConversationMembers { get; set; }
        public DbSet<Hobby> Hobbies { get; set; }
        public DbSet<HobbyUser> HobbyUsers { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MeetingAttendance> MeetingAttendances { get; set; }
        public DbSet<MeetingInvitation> MeetingInvitations { get; set; }
        public DbSet<Message> Messages { get; set; }

        public DatingAppDbContext(DbContextOptions<DatingAppDbContext> options)
            : base(options)
        {
            //Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole>().HasData(
                new { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new { Id = "2", Name = "User", NormalizedName = "USER" }
            );

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
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MeetingAttendance>()
                .HasIndex(ma => new { ma.MeetingId, ma.UserId })
                .IsUnique();

            modelBuilder.Entity<MeetingInvitation>()
                .HasIndex(invitation => new
                {
                    invitation.MeetingId,
                    invitation.UserId
                })
                .IsUnique();

            modelBuilder.Entity<MeetingInvitation>()
                .HasOne(invitation => invitation.Meeting)
                .WithMany(meeting => meeting.MeetingInvitations)
                .HasForeignKey(invitation => invitation.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MeetingInvitation>()
                .HasOne(invitation => invitation.User)
                .WithMany()
                .HasForeignKey(invitation => invitation.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Meeting>()
                .HasOne(m => m.Location)
                .WithMany(l => l.Meetings)
                .HasForeignKey(m => m.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Meeting>()
                .HasOne(meeting => meeting.Conversation)
                .WithMany(conversation => conversation.Meetings)
                .HasForeignKey(meeting => meeting.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HobbyUser>()
                .HasOne(hu => hu.Hobby)
                .WithMany(h => h.HobbyUsers)
                .HasForeignKey(hu => hu.HobbyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HobbyUser>()
                .HasOne(hu => hu.User)
                .WithMany(u => u.HobbyUsers)
                .HasForeignKey(hu => hu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConversationMember>()
                .HasOne(cm => cm.Conversation)
                .WithMany(c => c.ConversationMembers)
                .HasForeignKey(cm => cm.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConversationMember>()
                .HasOne(cm => cm.User)
                .WithMany(u => u.ConversationMembers)
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

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
                .HasIndex(c => new { c.UserId_A, c.UserId_B })
                .IsUnique();

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

            SeedLocations(modelBuilder);
            SeedHobbies(modelBuilder);
        }

        private static void SeedLocations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>().HasData(
                new Location
                {
                    Id = 1,
                    Name = "Deák Ferenc tér",
                    Address = "1052 Budapest, Deák Ferenc tér",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Location
                {
                    Id = 2,
                    Name = "Széll Kálmán tér",
                    Address = "1024 Budapest, Széll Kálmán tér",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Location
                {
                    Id = 3,
                    Name = "Fővám tér",
                    Address = "1056 Budapest, Fővám tér",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Location
                {
                    Id = 4,
                    Name = "Városliget",
                    Address = "1146 Budapest, Városliget",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Location
                {
                    Id = 5,
                    Name = "Kopaszi-gát",
                    Address = "1117 Budapest, Kopaszi-gát",
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );
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

        private static void SeedHobbies(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Hobby>().HasData(
                new Hobby
                {
                    Id = 1,
                    Name = "Traveling",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 2,
                    Name = "Hiking",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 3,
                    Name = "Cycling",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 4,
                    Name = "Running",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 5,
                    Name = "Fitness",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 6,
                    Name = "Swimming",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 7,
                    Name = "Cooking",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 8,
                    Name = "Photography",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 9,
                    Name = "Gaming",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 10,
                    Name = "Movies",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 11,
                    Name = "Music",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 12,
                    Name = "Reading",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 13,
                    Name = "Concerts",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 14,
                    Name = "Board Games",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 15,
                    Name = "Animals",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 16,
                    Name = "Technology",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 17,
                    Name = "Art",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 18,
                    Name = "Dancing",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 19,
                    Name = "Football",
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Hobby
                {
                    Id = 20,
                    Name = "Eating Out",
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetDates();
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
