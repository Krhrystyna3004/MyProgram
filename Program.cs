using System;
using System.IO;
using System.Windows.Forms;

namespace SecureNotes
{
    static class Program
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SecureNotes",
            "settings.ini"
        );

        public static bool RestartRequested { get; set; } = false;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            do
            {
                RestartRequested = false;
                LoadSavedTheme();

                User logged = null;
                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                    logged = login.LoggedInUser;
                }

                if (logged == null) return;

                CurrentUser = logged;

                if (!string.IsNullOrEmpty(logged.PreferredTheme))
                {
                    CurrentTheme = ThemeManager.FromString(logged.PreferredTheme);
                }

                LastActivity = DateTime.Now;
                SessionKey = null;

                Application.ApplicationExit += (s, e) => SaveCurrentTheme();

                using (var mainForm = new MainForm())
                {
                    Application.Run(mainForm);
                }

            } while (RestartRequested);
        }

        public static User CurrentUser { get; set; }
        public static Theme CurrentTheme { get; set; } = Theme.Light;
        public static DateTime LastActivity { get; set; } = DateTime.Now;
        public static byte[] SessionKey { get; set; }

        public static void TouchActivity()
        {
            LastActivity = DateTime.Now;
        }

        private static void LoadSavedTheme()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var lines = File.ReadAllLines(SettingsPath);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("Theme="))
                        {
                            var themeName = line.Substring(6).Trim();
                            CurrentTheme = ThemeManager.FromString(themeName);
                            break;
                        }
                    }
                }
            }
            catch { }
        }

        public static void SaveCurrentTheme()
        {
            try
            {
                var dir = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(SettingsPath, $"Theme={ThemeManager.GetThemeName(CurrentTheme)}");
            }
            catch { }
        }

        // ВИПРАВЛЕНО: Метод для зміни акаунту
        public static void SwitchAccount()
        {
            RestartRequested = true;
            CurrentUser = null;
            SessionKey = null;
            // НЕ викликаємо Application.Exit() тут - це робиться закриттям MainForm
        }
    }
}
