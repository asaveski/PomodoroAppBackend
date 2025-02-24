namespace PomodoroAppBackend.DTOs
{
    public class QuestionDto
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }
    }
}
