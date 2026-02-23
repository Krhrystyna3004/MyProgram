using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace SecureNotes
{
    public class DatabaseHelper
    {
        private readonly string _cs;

        // Конструктор без параметрів: бере рядок підключення з appsettings.json + змінних середовища
        public DatabaseHelper()
        {
            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            _cs = config.GetConnectionString("DefaultConnection");
        }

        // Конструктор з параметром: можна передати рядок вручну
        public DatabaseHelper(string connStr)
        {
            _cs = connStr;
        }

        public MySqlConnection GetConnection() => new MySqlConnection(_cs);

        // ---------------- USERS ----------------
        public User GetUserByUsername(string username)
        {
            using var conn = GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM users WHERE Username=@u", conn);
            cmd.Parameters.AddWithValue("@u", username);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new User
            {
                Id = r.GetInt32("Id"),
                Username = r.GetString("Username"),
                PasswordHash = r.GetString("PasswordHash"),
                PasswordSalt = r.GetString("PasswordSalt"),
                PinHash = r.IsDBNull(r.GetOrdinal("PinHash")) ? null : r.GetString("PinHash"),
                PinSalt = r.IsDBNull(r.GetOrdinal("PinSalt")) ? null : r.GetString("PinSalt"),
                PreferredTheme = r.IsDBNull(r.GetOrdinal("PreferredTheme")) ? "Light" : r.GetString("PreferredTheme"),
                CreatedAt = r.GetDateTime("CreatedAt")
            };
        }

        public int CreateUser(User u)
        {
            using var conn = GetConnection();
            conn.Open();

            var cmd = new MySqlCommand(
                @"INSERT INTO users (Username, PasswordHash, PasswordSalt, PinHash, PinSalt, PreferredTheme, CreatedAt)
                  VALUES (@un,@ph,@ps,@pinh,@pins,@theme,NOW());", conn);

            cmd.Parameters.AddWithValue("@un", u.Username);
            cmd.Parameters.AddWithValue("@ph", u.PasswordHash);
            cmd.Parameters.AddWithValue("@ps", u.PasswordSalt);
            cmd.Parameters.AddWithValue("@pinh", string.IsNullOrEmpty(u.PinHash) ? (object)DBNull.Value : u.PinHash);
            cmd.Parameters.AddWithValue("@pins", string.IsNullOrEmpty(u.PinSalt) ? (object)DBNull.Value : u.PinSalt);
            cmd.Parameters.AddWithValue("@theme", u.PreferredTheme ?? "Light");

            cmd.ExecuteNonQuery();

            var idCmd = new MySqlCommand("SELECT LAST_INSERT_ID();", conn);
            return Convert.ToInt32(idCmd.ExecuteScalar());
        }

        public void UpdateUserPin(int userId, string newPinHash, string newPinSalt)
        {
            using var conn = GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("UPDATE users SET PinHash=@h, PinSalt=@s WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@h", newPinHash);
            cmd.Parameters.AddWithValue("@s", newPinSalt);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
        }

        public void UpdateUserPin(int userId, string oldPinHashCheck, string newPinHash, string newPinSalt)
        {
            using var conn = GetConnection();
            conn.Open();

            string currentHash = null;
            using (var cmdGet = new MySqlCommand("SELECT PinHash FROM users WHERE Id=@id", conn))
            {
                cmdGet.Parameters.AddWithValue("@id", userId);
                var obj = cmdGet.ExecuteScalar();
                currentHash = obj == null || obj == DBNull.Value ? null : (string)obj;
            }

            if (!string.IsNullOrEmpty(currentHash) && currentHash != oldPinHashCheck)
                throw new Exception("Невірний старий PIN.");

            using (var cmd = new MySqlCommand("UPDATE users SET PinHash=@h, PinSalt=@s WHERE Id=@id", conn))
            {
                cmd.Parameters.AddWithValue("@h", newPinHash);
                cmd.Parameters.AddWithValue("@s", newPinSalt);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateUserTheme(int userId, string theme)
        {
            using var conn = GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("UPDATE users SET PreferredTheme=@t WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@t", theme);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
        }

        public void DeleteUser(int userId)
        {
            using var conn = GetConnection();
            conn.Open();

            using (var cmdNotes = new MySqlCommand("DELETE FROM notes WHERE OwnerId=@id", conn))
            {
                cmdNotes.Parameters.AddWithValue("@id", userId);
                cmdNotes.ExecuteNonQuery();
            }
            using (var cmdMembers = new MySqlCommand("DELETE FROM groupmembers WHERE UserId=@id", conn))
            {
                cmdMembers.Parameters.AddWithValue("@id", userId);
                cmdMembers.ExecuteNonQuery();
            }
            using (var cmdUser = new MySqlCommand("DELETE FROM users WHERE Id=@id", conn))
            {
                cmdUser.Parameters.AddWithValue("@id", userId);
                cmdUser.ExecuteNonQuery();
            }
        }


        // ---------------- GROUPS ----------------
        public Group CreateGroup(int ownerId, string name = "Моя група")
        {
            var inviteCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            using var conn = new MySqlConnection(_cs);
            conn.Open();
            var cmd = new MySqlCommand(
                @"INSERT INTO usergroups (Name, InviteCode, CreatedBy, CreatedAt)
                  VALUES (@n,@code,@owner,NOW());
                  SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@code", inviteCode);
            cmd.Parameters.AddWithValue("@owner", ownerId);
            var id = Convert.ToInt32(cmd.ExecuteScalar());

            AddMember(id, ownerId, "edit");

            return new Group
            {
                Id = id,
                OwnerId = ownerId,
                InviteCode = inviteCode,
                Name = name,
                CreatedAt = DateTime.Now
            };
        }

        public Group GetGroupByInvite(string code)
        {
            using var conn = new MySqlConnection(_cs);
            conn.Open();
            var cmd = new MySqlCommand("SELECT * FROM usergroups WHERE InviteCode=@c", conn);
            cmd.Parameters.AddWithValue("@c", code);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new Group
            {
                Id = r.GetInt32("Id"),
                Name = r.GetString("Name"),
                InviteCode = r.GetString("InviteCode"),
                OwnerId = r.GetInt32("CreatedBy"),
                CreatedAt = r.GetDateTime("CreatedAt")
            };
        }

        public List<Group> GetGroupsForUser(int userId)
        {
            var list = new List<Group>();
            using var conn = new MySqlConnection(_cs);
            conn.Open();
            var cmd = new MySqlCommand(
                @"SELECT g.* FROM usergroups g 
                  JOIN groupmembers gm ON g.Id=gm.GroupId 
                  WHERE gm.UserId=@uid", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Group
                {
                    Id = r.GetInt32("Id"),
                    Name = r.GetString("Name"),
                    InviteCode = r.GetString("InviteCode"),
                    OwnerId = r.GetInt32("CreatedBy"),
                    CreatedAt = r.GetDateTime("CreatedAt")
                });
            }
            return list;
        }

        public void AddMember(int groupId, int userId, string role = "edit")
        {
            using var conn = new MySqlConnection(_cs);
            conn.Open();

            using (var check = new MySqlCommand("SELECT COUNT(1) FROM groupmembers WHERE GroupId=@g AND UserId=@u", conn))
            {
                check.Parameters.AddWithValue("@g", groupId);
                check.Parameters.AddWithValue("@u", userId);
                var exists = Convert.ToInt32(check.ExecuteScalar()) > 0;
                if (exists) return;
            }

            using (var insert = new MySqlCommand("INSERT INTO groupmembers (GroupId, UserId, Role) VALUES (@g,@u,@p)", conn))
            {
                insert.Parameters.AddWithValue("@g", groupId);
                insert.Parameters.AddWithValue("@u", userId);
                insert.Parameters.AddWithValue("@p", role);
                insert.ExecuteNonQuery();
            }
        }


        public void DeleteGroup(int groupId, int userId)
        {
            using var conn = new MySqlConnection(_cs);
            conn.Open();

            // Перевіряємо чи користувач є власником групи
            using (var check = new MySqlCommand("SELECT CreatedBy FROM usergroups WHERE Id=@g", conn))
            {
                check.Parameters.AddWithValue("@g", groupId);
                var ownerId = Convert.ToInt32(check.ExecuteScalar());
                if (ownerId != userId)
                    throw new Exception("Тільки власник може видалити групу.");
            }

            // Видаляємо нотатки групи
            using (var delNotes = new MySqlCommand("DELETE FROM notes WHERE GroupId=@g", conn))
            {
                delNotes.Parameters.AddWithValue("@g", groupId);
                delNotes.ExecuteNonQuery();
            }

            // Видаляємо учасників
            using (var delMembers = new MySqlCommand("DELETE FROM groupmembers WHERE GroupId=@g", conn))
            {
                delMembers.Parameters.AddWithValue("@g", groupId);
                delMembers.ExecuteNonQuery();
            }

            // Видаляємо групу
            using (var delGroup = new MySqlCommand("DELETE FROM usergroups WHERE Id=@g", conn))
            {
                delGroup.Parameters.AddWithValue("@g", groupId);
                delGroup.ExecuteNonQuery();
            }
        }

        public void LeaveGroup(int groupId, int userId)
        {
            using var conn = new MySqlConnection(_cs);
            conn.Open();

            using var cmd = new MySqlCommand("DELETE FROM groupmembers WHERE GroupId=@g AND UserId=@u", conn);
            cmd.Parameters.AddWithValue("@g", groupId);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.ExecuteNonQuery();
        }
        // ---------------- NOTES ----------------
        public int AddNote(Note note)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(@"
            INSERT INTO notes (OwnerId, GroupId, Title, Content, Type, Color, Tags, IvBase64, Attachments, CreatedAt, UpdatedAt)
            VALUES (@ownerId, @groupId, @title, @content, @type, @color, @tags, @iv, @attachments, @created, @updated);
            SELECT LAST_INSERT_ID();", conn);

                cmd.Parameters.AddWithValue("@ownerId", note.OwnerId);
                cmd.Parameters.AddWithValue("@groupId", note.GroupId.HasValue ? (object)note.GroupId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@title", note.Title);
                cmd.Parameters.AddWithValue("@content", note.Content ?? "");
                cmd.Parameters.AddWithValue("@type", note.Type);
                cmd.Parameters.AddWithValue("@color", note.Color ?? "#FFFFFF");
                cmd.Parameters.AddWithValue("@tags", note.Tags ?? "");
                cmd.Parameters.AddWithValue("@iv", note.IvBase64 ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@attachments", note.Attachments ?? "");
                cmd.Parameters.AddWithValue("@created", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated", DateTime.Now);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void UpdateNote(Note note)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(@"
            UPDATE notes SET 
                Title = @title, 
                Content = @content, 
                Type = @type, 
                Color = @color, 
                Tags = @tags, 
                GroupId = @groupId, 
                IvBase64 = @iv,
                Attachments = @attachments,
                UpdatedAt = @updated
            WHERE Id = @id", conn);

                cmd.Parameters.AddWithValue("@title", note.Title);
                cmd.Parameters.AddWithValue("@content", note.Content ?? "");
                cmd.Parameters.AddWithValue("@type", note.Type);
                cmd.Parameters.AddWithValue("@color", note.Color ?? "#FFFFFF");
                cmd.Parameters.AddWithValue("@tags", note.Tags ?? "");
                cmd.Parameters.AddWithValue("@groupId", note.GroupId.HasValue ? (object)note.GroupId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@iv", note.IvBase64 ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@attachments", note.Attachments ?? "");
                cmd.Parameters.AddWithValue("@updated", DateTime.Now);
                cmd.Parameters.AddWithValue("@id", note.Id);

                cmd.ExecuteNonQuery();
            }
        }

        public Note GetNoteById(int id)
        {
            using var conn = new MySqlConnection(_cs);
            conn.Open();
            var cmd = new MySqlCommand(
                @"SELECT Id, OwnerId, GroupId, Title, Content, Type, Color, Tags, IvBase64, Attachments, CreatedAt, UpdatedAt
          FROM notes WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new Note
            {
                Id = r.GetInt32("Id"),
                OwnerId = r.GetInt32("OwnerId"),
                GroupId = r.IsDBNull(r.GetOrdinal("GroupId")) ? (int?)null : r.GetInt32("GroupId"),
                Title = r.GetString("Title"),
                Content = r.GetString("Content"),
                Type = r.GetString("Type"),
                Color = r.IsDBNull(r.GetOrdinal("Color")) ? "#FFFFFF" : r.GetString("Color"),
                Tags = r.IsDBNull(r.GetOrdinal("Tags")) ? "" : r.GetString("Tags"),
                IvBase64 = r.IsDBNull(r.GetOrdinal("IvBase64")) ? null : r.GetString("IvBase64"),
                Attachments = r.IsDBNull(r.GetOrdinal("Attachments")) ? "" : r.GetString("Attachments"),  // ДОДАНО
                CreatedAt = r.GetDateTime("CreatedAt"),
                UpdatedAt = r.GetDateTime("UpdatedAt")
            };
        }

        public List<Note> GetNotesForUser(int userId)
        {
            var list = new List<Note>();
            using var conn = new MySqlConnection(_cs);
            conn.Open();
            var cmd = new MySqlCommand(
                @"SELECT Id, OwnerId, GroupId, Title, Content, Type, Color, Tags, IvBase64, Attachments, CreatedAt, UpdatedAt
          FROM notes
          WHERE OwnerId=@u
             OR GroupId IN (SELECT GroupId FROM groupmembers WHERE UserId=@u)
          ORDER BY UpdatedAt DESC", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Note
                {
                    Id = r.GetInt32("Id"),
                    OwnerId = r.GetInt32("OwnerId"),
                    GroupId = r.IsDBNull(r.GetOrdinal("GroupId")) ? (int?)null : r.GetInt32("GroupId"),
                    Title = r.GetString("Title"),
                    Content = r.GetString("Content"),
                    Type = r.GetString("Type"),
                    Color = r.IsDBNull(r.GetOrdinal("Color")) ? "#FFFFFF" : r.GetString("Color"),
                    Tags = r.IsDBNull(r.GetOrdinal("Tags")) ? "" : r.GetString("Tags"),
                    IvBase64 = r.IsDBNull(r.GetOrdinal("IvBase64")) ? null : r.GetString("IvBase64"),
                    Attachments = r.IsDBNull(r.GetOrdinal("Attachments")) ? "" : r.GetString("Attachments"),  // ДОДАНО
                    CreatedAt = r.GetDateTime("CreatedAt"),
                    UpdatedAt = r.GetDateTime("UpdatedAt")
                });
            }
            return list;
        }


        public List<Note> GetNotesForGroup(int groupId)
        {
            var list = new List<Note>();
            using var conn = new MySqlConnection(_cs);
            conn.Open();
            var cmd = new MySqlCommand(
                @"SELECT Id, OwnerId, GroupId, Title, Content, Type, Color, Tags, IvBase64, Attachments, CreatedAt, UpdatedAt
          FROM notes WHERE GroupId=@g ORDER BY UpdatedAt DESC", conn);
            cmd.Parameters.AddWithValue("@g", groupId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Note
                {
                    Id = r.GetInt32("Id"),
                    OwnerId = r.GetInt32("OwnerId"),
                    GroupId = r.IsDBNull(r.GetOrdinal("GroupId")) ? (int?)null : r.GetInt32("GroupId"),
                    Title = r.GetString("Title"),
                    Content = r.GetString("Content"),
                    Type = r.GetString("Type"),
                    Color = r.IsDBNull(r.GetOrdinal("Color")) ? "#FFFFFF" : r.GetString("Color"),
                    Tags = r.IsDBNull(r.GetOrdinal("Tags")) ? "" : r.GetString("Tags"),
                    IvBase64 = r.IsDBNull(r.GetOrdinal("IvBase64")) ? null : r.GetString("IvBase64"),
                    Attachments = r.IsDBNull(r.GetOrdinal("Attachments")) ? "" : r.GetString("Attachments"),  // ДОДАНО
                    CreatedAt = r.GetDateTime("CreatedAt"),
                    UpdatedAt = r.GetDateTime("UpdatedAt")
                });
            }
            return list;
        }

        public void UpdateUserPassword(int userId, string newPasswordHash, string newPasswordSalt)
        {
            using var conn = GetConnection();
            conn.Open();
            var cmd = new MySqlCommand("UPDATE users SET PasswordHash=@h, PasswordSalt=@s WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@h", newPasswordHash);
            cmd.Parameters.AddWithValue("@s", newPasswordSalt);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
        }

        public void DeleteNote(int id)
        {
            using var conn = new MySqlConnection(_cs);
            conn.Open();
            var cmd = new MySqlCommand("DELETE FROM notes WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
