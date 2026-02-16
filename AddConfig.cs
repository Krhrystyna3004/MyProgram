using System;

namespace SecureNotes
{
    public static class AppConfig
    {
        public static string ConnStr
        {
            get
            {
                var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

                if (string.IsNullOrEmpty(password))
                    throw new Exception("DB_PASSWORD environment variable not set.");

                return $"Server=securenotes1-khrystynanovak3004-69ca.c.aivencloud.com;" +
                       $"Port=13671;" +
                       $"Database=defaultdb;" +
                       $"Uid=avnadmin;" +
                       $"Password={password};" +
                       $"SslMode=Required;";
            }
        }
    }
}
