using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.Extensions.Configuration;

namespace SecureNotes
{
    public class GroupSyncService
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

        public GroupSyncService()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            _apiBaseUrl = (config["Api:BaseUrl"] ?? string.Empty).Trim().TrimEnd('/');
        }

        public Group CreateGroup(int ownerId, string name)
        {
            if (TryCreateGroupViaApi(ownerId, name, out Group group) && group != null)
            {
                return group;
            }

            return Db.CreateGroup(ownerId, name);
        }

        public Group GetGroupByInvite(string inviteCode)
        {
            if (TryGetFromApi($"/groups/by-invite/{Uri.EscapeDataString(inviteCode)}", out Group group) && group != null)
            {
                return group;
            }

            return Db.GetGroupByInvite(inviteCode);
        }

        public List<Group> GetGroupsForUser(int userId)
        {
            if (TryGetFromApi($"/users/{userId}/groups", out List<Group> groups) && groups != null)
            {
                return groups;
            }

            return Db.GetGroupsForUser(userId);
        }

        public void AddMember(int groupId, int userId, string role = "edit")
        {
            var payload = new { groupId, userId, role };
            if (TryUseApi("/groups/members", "POST", payload))
            {
                return;
            }

            Db.AddMember(groupId, userId, role);
        }

        public void LeaveGroup(int groupId, int userId)
        {
            if (TryUseApi($"/groups/{groupId}/members/{userId}", "DELETE", null))
            {
                return;
            }

            Db.LeaveGroup(groupId, userId);
        }

        public void DeleteGroup(int groupId, int userId)
        {
            if (TryUseApi($"/groups/{groupId}?requestUserId={userId}", "DELETE", null))
            {
                return;
            }

            Db.DeleteGroup(groupId, userId);
        }

        private bool TryCreateGroupViaApi(int ownerId, string name, out Group group)
        {
            group = null;
            if (string.IsNullOrWhiteSpace(_apiBaseUrl))
            {
                return false;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(15);
                    var payload = new { ownerId, name };
                    var content = new StringContent(_json.Serialize(payload), Encoding.UTF8, "application/json");
                    var response = client.PostAsync(_apiBaseUrl + "/groups", content).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        return false;
                    }

                    var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    group = _json.Deserialize<Group>(body);
                    return group != null;
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
                        request.Content = new StringContent(_json.Serialize(payload), Encoding.UTF8, "application/json");
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
