using Microsoft.EntityFrameworkCore;
using PomodoroAppBackend.Models;

namespace PomodoroAppBackend.Context
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {}
        public DbSet<Note> Notes { get; set; }

        // DbSet for Subject model (to manage categories of notes)
        public DbSet<Subject> Subjects { get; set; }

        // DbSet for Cue model (for keywords or main ideas related to the note)
        public DbSet<Cue> Cues { get; set; }

        // DbSet for SuccinctNote model (for summaries or key points)
        public DbSet<SuccinctNote> SuccinctNotes { get; set; }
    }
}
