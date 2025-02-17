namespace PomodoroAppBackend.Models
{
    public class Cue
    {
        public int CueId { get; set; }
        public string? Text { get; set; }  // The text of the cue (e.g., "Machine Learning Algorithms")
        public int NoteId { get; set; }  // Each cue is tied to a specific note
        public Note? Note { get; set; }
    }
}
