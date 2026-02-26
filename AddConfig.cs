using System;
using Microsoft.Extensions.Configuration;

namespace SecureNotes
{
    public static class AppConfig
    {
        public static string ConnStr
        {
            get
            {
                var config = new ConfigurationBuilder()
                  .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                  .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                  .AddEnvironmentVariables()
                  .Build();

                var fullConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
                if (!string.IsNullOrWhiteSpace(fullConnectionString))
                {
                    return fullConnectionString;
                }

                var connectionString = config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new Exception("ConnectionStrings:DefaultConnection is not set.");

                const string passwordPlaceholder = "${DB_PASSWORD}";
                if (connectionString.IndexOf(passwordPlaceholder, StringComparison.Ordinal) >= 0)
                {
                    var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
                    if (string.IsNullOrWhiteSpace(password))
                    {
                        throw new Exception("DB_PASSWORD environment variable not set.");
                    }

                    return connectionString.Replace(passwordPlaceholder, password);
                }

                return connectionString;
            }
        }
    }
}
