using System;

namespace SecureNotes
{
    public class UserAccountService
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

        public void UpdatePassword(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    api.ChangePassword(currentPassword, newPassword);
                    return;
                }
            }
            catch
            {
                // fallback to DB below
            }

            if (Program.CurrentUser == null)
            {
                throw new Exception("User session is not available.");
            }

            var currentHash = CryptoService.HashWithPBKDF2(currentPassword, Program.CurrentUser.PasswordSalt);
            if (!string.Equals(currentHash, Program.CurrentUser.PasswordHash, StringComparison.Ordinal))
            {
                throw new Exception("Current password is incorrect.");
            }

            var newSalt = CryptoService.GenerateSalt();
            var newHash = CryptoService.HashWithPBKDF2(newPassword, newSalt);
            Db.UpdateUserPassword(userId, newHash, newSalt);

            Program.CurrentUser.PasswordHash = newHash;
            Program.CurrentUser.PasswordSalt = newSalt;
        }

        public void UpdateTheme(int userId, string preferredTheme)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    api.UpdateTheme(preferredTheme);
                    return;
                }
            }
            catch
            {
                // fallback to DB below
            }

            Db.UpdateUserTheme(userId, preferredTheme);
        }

        public void DeleteAccount(int userId)
        {
            try
            {
                var api = CreateApiClient();
                if (api != null)
                {
                    api.DeleteMyAccount();
                    return;
                }
            }
            catch
            {
                // fallback to DB below
            }

            Db.DeleteUser(userId);
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
