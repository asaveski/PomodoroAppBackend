namespace PomodoroAppBackend.Models
{
    public class Quiz
    {
        public int QuizId { get; set; }  // Unique identifier for the quiz
        public int NoteId { get; set; }  // Foreign key to the Note
        //public Note? Note { get; set; }  // Navigation property to the associated Note

        // List of questions in the quiz
        public List<Question> Questions { get; set; } = new List<Question>();
    }
}
