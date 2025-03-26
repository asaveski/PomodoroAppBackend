namespace PomodoroAppBackend.DTOs
{
    public class NoteDto
    {
        public string Topic { get; set; }
        public string Summary { get; set; }

        public int? SubjectId { get; set; }
        public string SubjectName { get; set; }

        public List<string> Cues { get; set; }
        public List<string> SuccinctNotes { get; set; }
    }
}
