namespace PomodoroAppBackend.Models
{
    public class Question
    {
        public int QuestionId { get; set; }  // Unique identifier for the question
        public string? QuestionText { get; set; }  // The text of the question
        public List<string>? Options { get; set; }  // The options for the question
        public string? CorrectAnswer { get; set; }  // The correct answer for the question

        public int QuizId { get; set; }

    }
}
