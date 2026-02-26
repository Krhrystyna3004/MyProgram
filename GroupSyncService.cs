using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureNotes
{
    public class GroupSyncService
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

        public Group CreateGroup(int ownerId, string name)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    return api.CreateGroup(name);
                }
            }
            catch
            {
                // fallback to DB below
            }

            return Db.CreateGroup(ownerId, name);
        }

        public Group JoinGroupByInvite(string inviteCode)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    return api.JoinGroup(inviteCode);
                }
            }
            catch
            {
                // fallback to DB below
            }

            var group = Db.GetGroupByInvite(inviteCode);
            if (group != null)
            {
                Db.AddMember(group.Id, Program.CurrentUser.Id, "edit");
            }

            return group;
        }

        public Group GetGroupByInvite(string inviteCode)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    return api.GetGroups().FirstOrDefault(g =>
                        string.Equals(g.InviteCode, inviteCode, StringComparison.OrdinalIgnoreCase));
                }
            }
            catch
            {
                // fallback to DB below
            }

            return Db.GetGroupByInvite(inviteCode);
        }

        public List<Group> GetGroupsForUser(int userId)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    return api.GetGroups();
                }
            }
            catch
            {
                // fallback to DB below
            }

            return Db.GetGroupsForUser(userId);
        }

        public void AddMember(int groupId, int userId, string role = "edit")
        {
            Db.AddMember(groupId, userId, role);
        }

        public void LeaveGroup(int groupId, int userId)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    api.LeaveGroup(groupId);
                    return;
                }
            }
            catch
            {
                // fallback to DB below
            }

            Db.LeaveGroup(groupId, userId);
        }

        public void DeleteGroup(int groupId, int userId)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    api.DeleteGroup(groupId);
                    return;
                }
            }
            catch
            {
                // fallback to DB below
            }

            Db.DeleteGroup(groupId, userId);
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
