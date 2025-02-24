namespace PomodoroAppBackend.Models
{
    public class SuccinctNote
    {
        public int SuccinctNoteId { get; set; }
        public string? Summary { get; set; }  // The succinct summary or key takeaway from the note
                                              // No need for NoteId/Note as it's part of the Note
    }
}
