using System;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace SecureNotes
{
    public class UserAccountService
    {
        private readonly DatabaseHelper _db;
        private readonly string _apiBaseUrl;

        public UserAccountService()
        {
            _db = new DatabaseHelper(AppConfig.ConnStr);

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            _apiBaseUrl = (config["Api:BaseUrl"] ?? string.Empty).Trim().TrimEnd('/');
        }

        public void UpdatePassword(int userId, string newPasswordHash, string newPasswordSalt)
        {
            if (TryUseApi("/users/change-password", "PATCH", $"{{\"userId\":{userId},\"newPasswordHash\":\"{Escape(newPasswordHash)}\",\"newPasswordSalt\":\"{Escape(newPasswordSalt)}\"}}"))
            {
                return;
            }

            _db.UpdateUserPassword(userId, newPasswordHash, newPasswordSalt);
        }

        public void UpdateTheme(int userId, string preferredTheme)
        {
            if (TryUseApi("/users/theme", "PATCH", $"{{\"userId\":{userId},\"preferredTheme\":\"{Escape(preferredTheme)}\"}}"))
            {
                return;
            }

            _db.UpdateUserTheme(userId, preferredTheme);
        }

        public void DeleteAccount(int userId)
        {
            if (TryUseApi($"/users/{userId}", "DELETE", null))
            {
                return;
            }

            _db.DeleteUser(userId);
        }

        private bool TryUseApi(string endpoint, string method, string jsonBody)
        {
            if (string.IsNullOrWhiteSpace(_apiBaseUrl))
            {
                return false;
            }

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                var request = new HttpRequestMessage(new HttpMethod(method), _apiBaseUrl + endpoint);

                if (!string.IsNullOrWhiteSpace(jsonBody))
                {
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                }

                var response = client.SendAsync(request).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                return true;
            }
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
