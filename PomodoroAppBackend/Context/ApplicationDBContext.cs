using Microsoft.EntityFrameworkCore;
using PomodoroAppBackend.Models;

namespace PomodoroAppBackend.Context
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {}
        public DbSet<Note> Notes { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Question> Questions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.Subject)  // A note has one subject
                .WithMany()  // A subject can have many notes
                .HasForeignKey(n => n.SubjectId)  // SubjectId is the foreign key in Note
                .OnDelete(DeleteBehavior.SetNull); // You can change this to cascade or restrict based on your need

            modelBuilder.Entity<Note>()
                .OwnsMany(n => n.Cues)  // Cue is owned by Note
                .WithOwner()  // Ownership is with Note
                .HasForeignKey("NoteId");  // EF will handle the relationship

            modelBuilder.Entity<Note>()
                .OwnsMany(n => n.SuccinctNotes)  // SuccinctNote is owned by Note
                .WithOwner()  // Ownership is with Note
                .HasForeignKey("NoteId");  // EF will handle the relationship

            modelBuilder.Entity<Note>()
                .OwnsMany(n => n.Quizzes, q =>
                {
                    q.WithOwner().HasForeignKey("NoteId");  // Ownership is with Note
                    q.HasKey(q => q.QuizId); // Define QuizId as the key
                    q.OwnsMany(q => q.Questions, qq =>
                    {
                        qq.WithOwner().HasForeignKey("QuizId");  // Ownership is with Quiz
                        qq.HasKey(qq => qq.QuestionId); // Define QuestionId as the key
                    });
                });
        }
    }
}
