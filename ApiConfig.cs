using Microsoft.Extensions.Configuration;

namespace SecureNotes
{
    public static class ApiConfig
    {
        public static string BaseUrl
        {
            get
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();

                var fromEnv = config["API_BASE_URL"];
                if (!string.IsNullOrWhiteSpace(fromEnv))
                {
                    return fromEnv.Trim().TrimEnd('/');
                }

                var fromConfig = (config["Api:BaseUrl"] ?? string.Empty).Trim().TrimEnd('/');
                return string.IsNullOrWhiteSpace(fromConfig) ? "https://securenotes-api-lg2i.onrender.com" : fromConfig;
            }
        }
    }
}