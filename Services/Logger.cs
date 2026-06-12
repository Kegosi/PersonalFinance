namespace PersonalFinance.Services
{
    /// <summary>Простейший файловый логгер приложения.</summary>
    public static class Logger
    {
        private static readonly object _lock = new();
        private static string _logFile = string.Empty;

        public static void Initialize()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            Directory.CreateDirectory(dir);
            _logFile = Path.Combine(dir, $"finance_{DateTime.Now:yyyy-MM-dd}.log");
        }

        public static void LogInfo(string message) => Write("INFO", message);

        public static void LogWarning(string message) => Write("WARN", message);

        public static void LogError(string message, Exception ex = null)
        {
            var text = ex == null ? message : $"{message}: {ex.Message}\n{ex.StackTrace}";
            Write("ERROR", text);
        }

        private static void Write(string level, string message)
        {
            if (string.IsNullOrEmpty(_logFile)) Initialize();
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logFile,
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}");
                }
            }
            catch
            {
                // Логирование не должно ронять приложение.
            }
        }
    }
}
