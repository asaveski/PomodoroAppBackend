namespace PomodoroAppBackend.Models
{
    public class Cue
    {
        public int CueId { get; set; }
        public string? Text { get; set; }  // The text of the cue (e.g., "Machine Learning Algorithms")
                                           // No need for NoteId/Note as it's part of the Note
    }
}
