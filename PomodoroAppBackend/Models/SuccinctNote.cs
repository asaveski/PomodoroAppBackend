namespace PomodoroAppBackend.Models
{
    public class SuccinctNote
    {
        public int SuccinctNoteId { get; set; }
        public string? Summary { get; set; }  // The succinct summary or key takeaway from the note
        public int NoteId { get; set; }  // Reference to the original note (a note can have one or more succinct notes)
        public Note? Note { get; set; }  // Navigation property to the main note
    }
}
