using System;
using System.Collections.Generic;

namespace SecureNotes
{
    public class NoteSyncService
    {
        private DatabaseHelper _db;

        private DatabaseHelper Db
        {
            get
            {
                if (_db == null)
                {
                    _db = new DatabaseHelper(AppConfig.ConnStr);
                }

                return _db;
            }
        }

        public Note GetNoteById(int noteId)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    return api.GetNoteById(noteId);
                }
            }
            catch
            {
                // fallback to DB below
            }

            return Db.GetNoteById(noteId);
        }

        public List<Note> GetNotesForUser(int userId)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    return api.GetNotes();
                }
            }
            catch
            {
                // fallback to DB below
            }

            return Db.GetNotesForUser(userId);
        }

        public List<Note> GetNotesForGroup(int groupId)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    return api.GetNotesForGroup(groupId);
                }
            }
            catch
            {
                // fallback to DB below
            }

            return Db.GetNotesForGroup(groupId);
        }

        public int AddNote(Note note)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    return api.CreateNote(note);
                }
            }
            catch
            {
                // fallback to DB below
            }

            return Db.AddNote(note);
        }

        public void UpdateNote(Note note)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    api.UpdateNote(note);
                    return;
                }
            }
            catch
            {
                // fallback to DB below
            }

            Db.UpdateNote(note);
        }

        public void DeleteNote(int noteId)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    api.DeleteNote(noteId);
                    return;
                }
            }
            catch
            {
                // fallback to DB below
            }

            Db.DeleteNote(noteId);
        }

        private ApiClient CreateApiClient()
        {
            if (string.IsNullOrWhiteSpace(ApiConfig.BaseUrl) || string.IsNullOrWhiteSpace(SessionStore.AccessToken))
            {
                return null;
            }

            var api = new ApiClient(ApiConfig.BaseUrl);
            api.SetAccessToken(SessionStore.AccessToken);
            return api;
        }
    }
}
