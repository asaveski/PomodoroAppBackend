namespace PomodoroAppBackend.Models
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string? Name { get; set; }  // E.g., "Math", "Physics"
        public List<Note>? Notes { get; set; }  // A subject will have many notes
    }
}
