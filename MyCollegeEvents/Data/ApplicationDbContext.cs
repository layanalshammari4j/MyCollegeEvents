using Microsoft.EntityFrameworkCore;
using MyCollegeEvents.Models;

namespace MyCollegeEvents.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Participant> Participants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Event entity
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.EventID);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
            });

            // Configure Participant entity
            modelBuilder.Entity<Participant>(entity =>
            {
                entity.HasKey(p => p.ParticipantID);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.UniversityID).IsRequired().HasMaxLength(20);
                entity.Property(p => p.Department).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Email).IsRequired().HasMaxLength(100);

                // Configure relationship
                entity.HasOne(p => p.Event)
                      .WithMany(e => e.Participants)
                      .HasForeignKey(p => p.EventID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data with static dates
            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    EventID = 1,
                    Title = "ورشة البرمجة للمبتدئات",
                    Description = "ورشة تعليمية لتعلم أساسيات البرمجة باستخدام C#",
                    Date = new DateTime(2025, 9, 15, 10, 0, 0),
                    CreatedBy = "د. فاطمة أحمد",
                    CreatedDate = new DateTime(2025, 7, 28, 12, 0, 0)
                },
                new Event
                {
                    EventID = 2,
                    Title = "محاضرة الذكاء الاصطناعي",
                    Description = "محاضرة حول تطبيقات الذكاء الاصطناعي في الحياة العملية",
                    Date = new DateTime(2025, 10, 1, 14, 0, 0),
                    CreatedBy = "د. سارة محمد",
                    CreatedDate = new DateTime(2025, 7, 28, 12, 0, 0)
                },
                new Event
                {
                    EventID = 3,
                    Title = "مؤتمر التكنولوجيا النسائي",
                    Description = "مؤتمر يهدف لتمكين المرأة في مجال التكنولوجيا",
                    Date = new DateTime(2025, 10, 15, 9, 0, 0),
                    CreatedBy = "د. نورا علي",
                    CreatedDate = new DateTime(2025, 7, 28, 12, 0, 0)
                }
            );
        }
    }
}
