namespace PomodoroAppBackend.DTOs
{
    public class QuizDto
    {
        public int QuizId { get; set; }
        public List<QuestionDto> Questions { get; set; } // List of questions with details
    }
}
