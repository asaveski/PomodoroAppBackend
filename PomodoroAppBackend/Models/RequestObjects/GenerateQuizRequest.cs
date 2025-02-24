namespace PomodoroAppBackend.Models.RequestObjects
{
    public class GenerateQuizRequest
    {
        public int NoteId { get; set; }  // The ID of the note you're working with
        public string NoteText { get; set; }  // The note text, if needed for additional context
    }
}
