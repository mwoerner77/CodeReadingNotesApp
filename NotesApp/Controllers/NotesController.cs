namespace NotesApp.Controllers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using NotesApp;
    using NotesApp.Models;

    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private NotesLibrary notesLibrary;

        public NotesController()
        {
            this.notesLibrary = NotesLibrary.GetNotesLibraryInstance();
        }

        [HttpPost("CreateNote")]
        public ActionResult<string> Post(NoteCreationRequest noteCreationRequest)
        {
            if (noteCreationRequest.Title != null && noteCreationRequest.Contents != null)
            {
                NoteCreationStatusCode statusCode = this.notesLibrary.CreateOrUpdateNote(noteCreationRequest);

                switch (statusCode)
                {
                    case NoteCreationStatusCode.Error:
                        return BadRequest("An enexpected error has occured");
                    case NoteCreationStatusCode.Updated:
                        return Ok($"Updated the existing note titled \"{noteCreationRequest.Title}\"");
                    default:
                        return Ok($"Created a note titled \"{noteCreationRequest.Title}\"");
                }
            }
            else
            {
                return BadRequest("Invalid Request Object");
            }

        }

        [HttpGet("Notes/{title}")]
        public ActionResult<Note> Get(string title)
        {
            Note? note = this.notesLibrary.GetNoteByTitle(title);
            if (note == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(note);
            }
        }
    }
}