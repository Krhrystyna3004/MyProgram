using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.Extensions.Configuration;

namespace SecureNotes
{
    public class NoteSyncService
    {
        private DatabaseHelper _db;
        private readonly string _apiBaseUrl;
        private readonly JavaScriptSerializer _json = new JavaScriptSerializer();

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

        public NoteSyncService()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            _apiBaseUrl = (config["Api:BaseUrl"] ?? string.Empty).Trim().TrimEnd('/');
        }

        public Note GetNoteById(int noteId)
        {
            if (TryGetFromApi($"/notes/{noteId}", out Note note) && note != null)
            {
                return note;
            }

            return Db.GetNoteById(noteId);
        }

        public List<Note> GetNotesForUser(int userId)
        {
            if (TryGetFromApi($"/users/{userId}/notes", out List<Note> notes) && notes != null)
            {
                return notes;
            }

            return Db.GetNotesForUser(userId);
        }

        public List<Note> GetNotesForGroup(int groupId)
        {
            if (TryGetFromApi($"/groups/{groupId}/notes", out List<Note> notes) && notes != null)
            {
                return notes;
            }

            return Db.GetNotesForGroup(groupId);
        }

        public int AddNote(Note note)
        {
            if (TryCreateNoteViaApi(note, out int createdId) && createdId > 0)
            {
                return createdId;
            }

            return Db.AddNote(note);
        }

        public void UpdateNote(Note note)
        {
            if (TryUseApi($"/notes/{note.Id}", "PUT", note))
            {
                return;
            }

            Db.UpdateNote(note);
        }

        public void DeleteNote(int noteId)
        {
            if (TryUseApi($"/notes/{noteId}", "DELETE", null))
            {
                return;
            }

            Db.DeleteNote(noteId);
        }

        private bool TryCreateNoteViaApi(Note note, out int createdId)
        {
            createdId = 0;

            if (string.IsNullOrWhiteSpace(_apiBaseUrl))
            {
                return false;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(15);
                    var jsonBody = _json.Serialize(note);
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(_apiBaseUrl + "/notes", content).GetAwaiter().GetResult();

                    if (!response.IsSuccessStatusCode)
                    {
                        return false;
                    }

                    var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (string.IsNullOrWhiteSpace(body))
                    {
                        return false;
                    }

                    var map = _json.Deserialize<Dictionary<string, object>>(body);
                    if (map != null && map.ContainsKey("id") && int.TryParse(map["id"]?.ToString(), out int idFromApi))
                    {
                        createdId = idFromApi;
                        return true;
                    }

                    if (int.TryParse(body, out int rawId))
                    {
                        createdId = rawId;
                        return true;
                    }

                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool TryGetFromApi<T>(string endpoint, out T data)
        {
            data = default(T);

            if (string.IsNullOrWhiteSpace(_apiBaseUrl))
            {
                return false;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(15);
                    var response = client.GetAsync(_apiBaseUrl + endpoint).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        return false;
                    }

                    var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    data = _json.Deserialize<T>(body);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool TryUseApi(string endpoint, string method, object payload)
        {
            if (string.IsNullOrWhiteSpace(_apiBaseUrl))
            {
                return false;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(15);
                    var request = new HttpRequestMessage(new HttpMethod(method), _apiBaseUrl + endpoint);

                    if (payload != null)
                    {
                        var jsonBody = _json.Serialize(payload);
                        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    }

                    var response = client.SendAsync(request).GetAwaiter().GetResult();
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
