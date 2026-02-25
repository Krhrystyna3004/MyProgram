using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SecureNotes
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private string _accessToken;

        public ApiClient(string baseUrl)
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public bool HasToken => !string.IsNullOrWhiteSpace(_accessToken);

        public void SetAccessToken(string token)
        {
            _accessToken = token;
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        // ---------- AUTH ----------
        public AuthResponse Register(string username, string password)
        {
            var auth = Send<AuthResponse>(
                HttpMethod.Post,
                "api/auth/register",
                new { username, password });

            SetAccessToken(auth.AccessToken);
            return auth;
        }

        public AuthResponse Login(string username, string password)
        {
            var auth = Send<AuthResponse>(
                HttpMethod.Post,
                "api/auth/login",
                new { username, password });

            SetAccessToken(auth.AccessToken);
            return auth;
        }

        // ---------- NOTES ----------
        public List<Note> GetNotes()
            => Send<List<Note>>(HttpMethod.Get, "api/notes");

        public Note GetNoteById(int noteId)
            => Send<Note>(HttpMethod.Get, $"api/notes/{noteId}");

        public List<Note> GetNotesForGroup(int groupId)
            => Send<List<Note>>(HttpMethod.Get, $"api/groups/{groupId}/notes");

        public int CreateNote(Note note)
        {
            var result = Send<IdResponse>(HttpMethod.Post, "api/notes", new
            {
                groupId = note.GroupId,
                title = note.Title ?? "",
                content = note.Content ?? "",
                type = note.Type ?? "note",
                color = note.Color ?? "#FFFFFF",
                tags = note.Tags ?? "",
                ivBase64 = note.IvBase64,
                attachments = note.Attachments ?? ""
            });

            return result.Id;
        }

        public void UpdateNote(Note note)
        {
            SendNoContent(HttpMethod.Put, $"api/notes/{note.Id}", new
            {
                groupId = note.GroupId,
                title = note.Title ?? "",
                content = note.Content ?? "",
                type = note.Type ?? "note",
                color = note.Color ?? "#FFFFFF",
                tags = note.Tags ?? "",
                ivBase64 = note.IvBase64,
                attachments = note.Attachments ?? ""
            });
        }

        public void DeleteNote(int noteId)
            => SendNoContent(HttpMethod.Delete, $"api/notes/{noteId}");

        // ---------- GROUPS ----------
        public List<Group> GetGroups()
            => Send<List<Group>>(HttpMethod.Get, "api/groups");

        public Group CreateGroup(string name)
            => Send<Group>(HttpMethod.Post, "api/groups", new { name });

        public Group JoinGroup(string inviteCode)
            => Send<Group>(HttpMethod.Post, "api/groups/join", new { inviteCode });

        public void LeaveGroup(int groupId)
            => SendNoContent(HttpMethod.Post, $"api/groups/{groupId}/leave");

        public void DeleteGroup(int groupId)
            => SendNoContent(HttpMethod.Delete, $"api/groups/{groupId}");

        // ---------- USER ----------
        public void UpdateTheme(string theme)
            => SendNoContent(HttpMethod.Put, "api/users/theme", new { theme });

        public void ChangePassword(string currentPassword, string newPassword)
            => SendNoContent(HttpMethod.Put, "api/users/password", new { currentPassword, newPassword });

        public void DeleteMyAccount()
            => SendNoContent(HttpMethod.Delete, "api/users/me");

        // ---------- PIN ----------
        public void SetPin(string newPin)
            => SendNoContent(HttpMethod.Post, "api/pin/set", new { newPin });

        public void ChangePin(string oldPin, string newPin)
            => SendNoContent(HttpMethod.Post, "api/pin/change", new { oldPin, newPin });

        public bool VerifyPin(string pin)
        {
            var result = Send<VerifyPinResult>(HttpMethod.Post, "api/pin/verify", new { pin });
            return result?.IsValid == true;
        }

        // ---------- INTERNAL HELPERS ----------
        private T Send<T>(HttpMethod method, string url, object payload = null)
        {
            using (var req = new HttpRequestMessage(method, url))
            {
                if (payload != null)
                {
                    var json = JsonConvert.SerializeObject(payload);
                    req.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var resp = _http.SendAsync(req).Result;
                var body = resp.Content.ReadAsStringAsync().Result;

                if (!resp.IsSuccessStatusCode)
                    throw new Exception(string.IsNullOrWhiteSpace(body)
                        ? $"{(int)resp.StatusCode} {resp.ReasonPhrase}"
                        : body);

                if (typeof(T) == typeof(string))
                    return (T)(object)body;

                return JsonConvert.DeserializeObject<T>(body);
            }
        }

        private void SendNoContent(HttpMethod method, string url, object payload = null)
        {
            using (var req = new HttpRequestMessage(method, url))
            {
                if (payload != null)
                {
                    var json = JsonConvert.SerializeObject(payload);
                    req.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var resp = _http.SendAsync(req).Result;
                var body = resp.Content.ReadAsStringAsync().Result;

                if (!resp.IsSuccessStatusCode)
                    throw new Exception(string.IsNullOrWhiteSpace(body)
                        ? $"{(int)resp.StatusCode} {resp.ReasonPhrase}"
                        : body);
            }
        }
    }

    // ---------- DTOs ----------
    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string AccessToken { get; set; }
    }

    public class VerifyPinResult
    {
        public bool IsValid { get; set; }
    }

    public class IdResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
