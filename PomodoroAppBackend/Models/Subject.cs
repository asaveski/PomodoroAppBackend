namespace PomodoroAppBackend.Models
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string? Name { get; set; }  // E.g., "Math", "Physics"

        // No need for Notes property in the Subject class
        // The relationship is handled by the Note class
    }
}
