namespace PomodoroAppBackend.DTOs
{
    public class NoteDto
    {
        public string Topic { get; set; }
        public string Summary { get; set; }
        public List<string> Cues { get; set; }
        public List<string> SuccinctNotes { get; set; }
    }
}
