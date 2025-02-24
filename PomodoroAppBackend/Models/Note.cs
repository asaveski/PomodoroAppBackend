namespace PomodoroAppBackend.Models
{
    public class Note
    {
        public int NoteId { get; set; }
        public string? Topic { get; set; }
        public string? Summary { get; set; }

        // Owned entities (no need for explicit foreign keys in the owned entities)
        public List<Cue>? Cues { get; set; }  // A note can have many cues
        public List<SuccinctNote>? SuccinctNotes { get; set; }  // A note can have one or more succinct notes
        public int? SubjectId { get; set; }  // Reference to Subject (optional)
        public Subject? Subject { get; set; }  // The subject this note belongs to

        //public int? QuizId { get; set; }  // Reference to Quiz 
        // Navigation property to quizzes associated with the note
        public List<Quiz>? Quizzes { get; set; }  // A note can have many quizzes

    }
}
