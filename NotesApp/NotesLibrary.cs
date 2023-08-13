namespace NotesApp
{
    using Microsoft.Extensions.Caching.Memory;
    using NotesApp.Models;
    using System.Text.Json;

    public class NotesLibrary
    {
        private readonly IMemoryCache recentNotesCache;
        private List<Note?>? notes = new List<Note?>();
        private static NotesLibrary notesLibraryInstance = new NotesLibrary();

        private NotesLibrary() 
        {
            MemoryCacheOptions cacheOptions = new MemoryCacheOptions();
            this.recentNotesCache = new MemoryCache(cacheOptions);
        }

        public static NotesLibrary GetNotesLibraryInstance()
        {
            return notesLibraryInstance;
        }

        public NoteCreationStatusCode CreateOrUpdateNote(NoteCreationRequest noteCreationRequest)
        {
            Note? noteToAdd = null;
            NoteCreationStatusCode statusCode = NoteCreationStatusCode.Created;
            if (this.TryGetNote(noteCreationRequest.Title, out Note? note))
            {
                if (note != null && this.notes != null)
                {
                    this.notes.Remove(note);
                    note.Contents = noteCreationRequest.Contents;
                    noteToAdd = note;
                    statusCode = NoteCreationStatusCode.Updated;
                }
                else
                {
                    return NoteCreationStatusCode.Error;
                }
            }
            else
            {
                noteToAdd = new Note()
                {
                    Title = noteCreationRequest.Title,
                    Contents = noteCreationRequest.Contents,
                    CreationTime = DateTime.Now
                };
            }

            if (noteToAdd != null && this.notes != null) 
            {
                this.recentNotesCache.Set(noteToAdd.Title, noteToAdd);
                this.notes.Add(noteToAdd);
                this.WriteAllNotes();
                return statusCode;
            }
            else
            {
                return NoteCreationStatusCode.Error;
            }
        }

        public Note? GetNoteByTitle(string title)
        {
            if (this.TryGetNote(title, out Note? note))
            {
                return note;
            }
            else
            {
                return null;
            }
        }

        public bool TryGetNote(string? noteTitle, out Note? note)
        {
            if (this.recentNotesCache.TryGetValue(noteTitle, out Note? n))
            {
                note = n;
                return true;
            }

            this.FetchAllNotes();
            if (this.notes != null) 
            {
                foreach (Note? note1 in this.notes)
                {
                    if (note1 != null && note1.Title == noteTitle)
                    {
                        note = note1;
                        return true;
                    }
                }
            }

            note = null;
            return false;
        }

        private void FetchAllNotes()
        {
            string notesJsonText = File.ReadAllText("Notes.json");
            this.notes = JsonSerializer.Deserialize<List<Note?>>(notesJsonText);
        }

        private void WriteAllNotes()
        {
            string notesJsonText = JsonSerializer.Serialize(this.notes);
            File.WriteAllText("Notes.json", notesJsonText);
        }
    }
}
