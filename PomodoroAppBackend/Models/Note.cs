namespace PomodoroAppBackend.Models
{
    public class Note
    {
        public int NoteId { get; set; }
        public string? Topic { get; set; }
        public string? Summary { get; set; }
        public List<Cue>? Cues { get; set; }  // A note can have many cues
        public int? SubjectId { get; set; }  // Each note is tied to a subject
        public Subject? Subject { get; set; }
        public List<SuccinctNote>? SuccinctNotes { get; set; }  // A note can have one or more succinct notes
    }
}
