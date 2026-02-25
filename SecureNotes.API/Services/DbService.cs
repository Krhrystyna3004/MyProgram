using MySql.Data.MySqlClient;
using SecureNotes.Api.Models;

namespace SecureNotes.Api.Services;

public sealed class DbService(IConfiguration config)
{
    private readonly string _cs = config.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection missing");

    private MySqlConnection OpenConnection()
    {
        var conn = new MySqlConnection(_cs);
        conn.Open();
        return conn;
    }

    public UserEntity? GetUserByUsername(string username)
    {
        using var conn = OpenConnection();
        using var cmd = new MySqlCommand("SELECT Id, Username, PasswordHash, PasswordSalt, PinHash, PinSalt FROM users WHERE Username=@u", conn);
        cmd.Parameters.AddWithValue("@u", username);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;

        return new UserEntity
        {
            Id = r.GetInt32("Id"),
            Username = r.GetString("Username"),
            PasswordHash = r.GetString("PasswordHash"),
            PasswordSalt = r.GetString("PasswordSalt"),
            PinHash = r.IsDBNull(r.GetOrdinal("PinHash")) ? null : r.GetString("PinHash"),
            PinSalt = r.IsDBNull(r.GetOrdinal("PinSalt")) ? null : r.GetString("PinSalt")
        };
    }

    public int CreateUser(string username, string passwordHash, string passwordSalt)
    {
        using var conn = OpenConnection();
        using var cmd = new MySqlCommand(@"
INSERT INTO users (Username, PasswordHash, PasswordSalt, PreferredTheme, CreatedAt)
VALUES (@un,@ph,@ps,'Light',NOW());
SELECT LAST_INSERT_ID();", conn);
        cmd.Parameters.AddWithValue("@un", username);
        cmd.Parameters.AddWithValue("@ph", passwordHash);
        cmd.Parameters.AddWithValue("@ps", passwordSalt);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public List<NoteEntity> GetNotesForUser(int userId)
    {
        var list = new List<NoteEntity>();
        using var conn = OpenConnection();
        using var cmd = new MySqlCommand(@"
SELECT Id, OwnerId, GroupId, Title, Content, Type, Color, Tags, IvBase64, Attachments, CreatedAt, UpdatedAt
FROM notes
WHERE OwnerId=@u OR GroupId IN (SELECT GroupId FROM groupmembers WHERE UserId=@u)
ORDER BY UpdatedAt DESC", conn);
        cmd.Parameters.AddWithValue("@u", userId);

        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new NoteEntity
            {
                Id = r.GetInt32("Id"),
                OwnerId = r.GetInt32("OwnerId"),
                GroupId = r.IsDBNull(r.GetOrdinal("GroupId")) ? null : r.GetInt32("GroupId"),
                Title = r.GetString("Title"),
                Content = r.GetString("Content"),
                Type = r.GetString("Type"),
                Color = r.IsDBNull(r.GetOrdinal("Color")) ? "#FFFFFF" : r.GetString("Color"),
                Tags = r.IsDBNull(r.GetOrdinal("Tags")) ? string.Empty : r.GetString("Tags"),
                IvBase64 = r.IsDBNull(r.GetOrdinal("IvBase64")) ? null : r.GetString("IvBase64"),
                Attachments = r.IsDBNull(r.GetOrdinal("Attachments")) ? string.Empty : r.GetString("Attachments"),
                CreatedAt = r.GetDateTime("CreatedAt"),
                UpdatedAt = r.GetDateTime("UpdatedAt")
            });
        }

        return list;
    }

    public int AddNote(int ownerId, UpsertNoteRequest note)
    {
        using var conn = OpenConnection();
        using var cmd = new MySqlCommand(@"
INSERT INTO notes (OwnerId, GroupId, Title, Content, Type, Color, Tags, IvBase64, Attachments, CreatedAt, UpdatedAt)
VALUES (@ownerId,@groupId,@title,@content,@type,@color,@tags,@iv,@attachments,NOW(),NOW());
SELECT LAST_INSERT_ID();", conn);

        cmd.Parameters.AddWithValue("@ownerId", ownerId);
        cmd.Parameters.AddWithValue("@groupId", note.GroupId.HasValue ? note.GroupId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@title", note.Title);
        cmd.Parameters.AddWithValue("@content", note.Content ?? string.Empty);
        cmd.Parameters.AddWithValue("@type", note.Type);
        cmd.Parameters.AddWithValue("@color", note.Color ?? "#FFFFFF");
        cmd.Parameters.AddWithValue("@tags", note.Tags ?? string.Empty);
        cmd.Parameters.AddWithValue("@iv", note.IvBase64 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@attachments", note.Attachments ?? string.Empty);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool UpdateNote(int noteId, int ownerId, UpsertNoteRequest note)
    {
        using var conn = OpenConnection();
        using var cmd = new MySqlCommand(@"
UPDATE notes SET
  GroupId=@groupId,
  Title=@title,
  Content=@content,
  Type=@type,
  Color=@color,
  Tags=@tags,
  IvBase64=@iv,
  Attachments=@attachments,
  UpdatedAt=NOW()
WHERE Id=@id AND OwnerId=@ownerId", conn);

        cmd.Parameters.AddWithValue("@id", noteId);
        cmd.Parameters.AddWithValue("@ownerId", ownerId);
        cmd.Parameters.AddWithValue("@groupId", note.GroupId.HasValue ? note.GroupId.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@title", note.Title);
        cmd.Parameters.AddWithValue("@content", note.Content ?? string.Empty);
        cmd.Parameters.AddWithValue("@type", note.Type);
        cmd.Parameters.AddWithValue("@color", note.Color ?? "#FFFFFF");
        cmd.Parameters.AddWithValue("@tags", note.Tags ?? string.Empty);
        cmd.Parameters.AddWithValue("@iv", note.IvBase64 ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@attachments", note.Attachments ?? string.Empty);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool DeleteNote(int noteId, int ownerId)
    {
        using var conn = OpenConnection();
        using var cmd = new MySqlCommand("DELETE FROM notes WHERE Id=@id AND OwnerId=@ownerId", conn);
        cmd.Parameters.AddWithValue("@id", noteId);
        cmd.Parameters.AddWithValue("@ownerId", ownerId);
        return cmd.ExecuteNonQuery() > 0;
    }

    // ---------------- GROUPS ----------------
    public GroupEntity CreateGroup(int ownerId, string name = "Моя група")
    {
        var inviteCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        using var conn = OpenConnection();
        using var cmd = new MySqlCommand(@"
INSERT INTO usergroups (Name, InviteCode, CreatedBy, CreatedAt)
VALUES (@n,@code,@owner,NOW());
SELECT LAST_INSERT_ID();", conn);

        cmd.Parameters.AddWithValue("@n", string.IsNullOrWhiteSpace(name) ? "Моя група" : name.Trim());
        cmd.Parameters.AddWithValue("@code", inviteCode);
        cmd.Parameters.AddWithValue("@owner", ownerId);

        var id = Convert.ToInt32(cmd.ExecuteScalar());

        AddMember(id, ownerId, "edit");

        return new GroupEntity
        {
            Id = id,
            OwnerId = ownerId,
            InviteCode = inviteCode,
            Name = string.IsNullOrWhiteSpace(name) ? "Моя група" : name.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public GroupEntity? GetGroupByInvite(string code)
    {
        using var conn = OpenConnection();
        using var cmd = new MySqlCommand("SELECT Id, Name, InviteCode, CreatedBy, CreatedAt FROM usergroups WHERE InviteCode=@c", conn);
        cmd.Parameters.AddWithValue("@c", code);

        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;

        return new GroupEntity
        {
            Id = r.GetInt32("Id"),
            Name = r.GetString("Name"),
            InviteCode = r.GetString("InviteCode"),
            OwnerId = r.GetInt32("CreatedBy"),
            CreatedAt = r.GetDateTime("CreatedAt")
        };
    }

    public List<GroupEntity> GetGroupsForUser(int userId)
    {
        var list = new List<GroupEntity>();

        using var conn = OpenConnection();
        using var cmd = new MySqlCommand(@"
SELECT g.Id, g.Name, g.InviteCode, g.CreatedBy, g.CreatedAt
FROM usergroups g
JOIN groupmembers gm ON g.Id=gm.GroupId
WHERE gm.UserId=@uid", conn);

        cmd.Parameters.AddWithValue("@uid", userId);

        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new GroupEntity
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
        using var conn = OpenConnection();

        using (var check = new MySqlCommand("SELECT COUNT(1) FROM groupmembers WHERE GroupId=@g AND UserId=@u", conn))
        {
            check.Parameters.AddWithValue("@g", groupId);
            check.Parameters.AddWithValue("@u", userId);
            var exists = Convert.ToInt32(check.ExecuteScalar()) > 0;
            if (exists) return;
        }

        using var insert = new MySqlCommand("INSERT INTO groupmembers (GroupId, UserId, Role) VALUES (@g,@u,@p)", conn);
        insert.Parameters.AddWithValue("@g", groupId);
        insert.Parameters.AddWithValue("@u", userId);
        insert.Parameters.AddWithValue("@p", role);
        insert.ExecuteNonQuery();
    }

    public void LeaveGroup(int groupId, int userId)
    {
        using var conn = OpenConnection();
        using var cmd = new MySqlCommand("DELETE FROM groupmembers WHERE GroupId=@g AND UserId=@u", conn);
        cmd.Parameters.AddWithValue("@g", groupId);
        cmd.Parameters.AddWithValue("@u", userId);
        cmd.ExecuteNonQuery();
    }

    public void DeleteGroup(int groupId, int userId)
    {
        using var conn = OpenConnection();

        using (var check = new MySqlCommand("SELECT CreatedBy FROM usergroups WHERE Id=@g", conn))
        {
            check.Parameters.AddWithValue("@g", groupId);
            var ownerObj = check.ExecuteScalar();
            if (ownerObj == null || ownerObj == DBNull.Value)
                throw new Exception("Групу не знайдено.");

            var ownerId = Convert.ToInt32(ownerObj);
            if (ownerId != userId)
                throw new Exception("Тільки власник може видалити групу.");
        }

        using (var delNotes = new MySqlCommand("DELETE FROM notes WHERE GroupId=@g", conn))
        {
            delNotes.Parameters.AddWithValue("@g", groupId);
            delNotes.ExecuteNonQuery();
        }

        using (var delMembers = new MySqlCommand("DELETE FROM groupmembers WHERE GroupId=@g", conn))
        {
            delMembers.Parameters.AddWithValue("@g", groupId);
            delMembers.ExecuteNonQuery();
        }

        using (var delGroup = new MySqlCommand("DELETE FROM usergroups WHERE Id=@g", conn))
        {
            delGroup.Parameters.AddWithValue("@g", groupId);
            delGroup.ExecuteNonQuery();
        }
    }

    // ---------------- USER SETTINGS ----------------
    public void UpdateUserTheme(int userId, string theme)
    {
        using var conn = OpenConnection();
        using var cmd = new MySqlCommand("UPDATE users SET PreferredTheme=@t WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@t", theme);
        cmd.Parameters.AddWithValue("@id", userId);
        cmd.ExecuteNonQuery();
    }

    public void UpdateUserPassword(int userId, string newPasswordHash, string newPasswordSalt)
    {
        using var conn = OpenConnection();
        using var cmd = new MySqlCommand("UPDATE users SET PasswordHash=@h, PasswordSalt=@s WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@h", newPasswordHash);
        cmd.Parameters.AddWithValue("@s", newPasswordSalt);
        cmd.Parameters.AddWithValue("@id", userId);
        cmd.ExecuteNonQuery();
    }

    public void DeleteUser(int userId)
    {
        using var conn = OpenConnection();

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

    // ---------------- PIN ----------------
    public (string? PinHash, string? PinSalt) GetPinData(int userId)
    {
        using var conn = OpenConnection();
        using var cmd = new MySqlCommand("SELECT PinHash, PinSalt FROM users WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@id", userId);

        using var r = cmd.ExecuteReader();
        if (!r.Read()) return (null, null);

        var pinHash = r.IsDBNull(r.GetOrdinal("PinHash")) ? null : r.GetString("PinHash");
        var pinSalt = r.IsDBNull(r.GetOrdinal("PinSalt")) ? null : r.GetString("PinSalt");
        return (pinHash, pinSalt);
    }

    public void UpdateUserPin(int userId, string newPinHash, string newPinSalt)
    {
        using var conn = OpenConnection();
        using var cmd = new MySqlCommand("UPDATE users SET PinHash=@h, PinSalt=@s WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@h", newPinHash);
        cmd.Parameters.AddWithValue("@s", newPinSalt);
        cmd.Parameters.AddWithValue("@id", userId);
        cmd.ExecuteNonQuery();
    }

    public void UpdateUserPinWithCheck(int userId, string oldPinHashCheck, string newPinHash, string newPinSalt)
    {
        using var conn = OpenConnection();

        string? currentHash = null;
        using (var cmdGet = new MySqlCommand("SELECT PinHash FROM users WHERE Id=@id", conn))
        {
            cmdGet.Parameters.AddWithValue("@id", userId);
            var obj = cmdGet.ExecuteScalar();
            currentHash = obj == null || obj == DBNull.Value ? null : (string)obj;
        }

        if (!string.IsNullOrEmpty(currentHash) && currentHash != oldPinHashCheck)
            throw new Exception("Невірний старий PIN.");

        using var cmd = new MySqlCommand("UPDATE users SET PinHash=@h, PinSalt=@s WHERE Id=@id", conn);
        cmd.Parameters.AddWithValue("@h", newPinHash);
        cmd.Parameters.AddWithValue("@s", newPinSalt);
        cmd.Parameters.AddWithValue("@id", userId);
        cmd.ExecuteNonQuery();
    }


}

