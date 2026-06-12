using PersonalFinance.Forms;
using PersonalFinance.Services;
using QuestPDF.Infrastructure;

namespace PersonalFinance
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            QuestPDF.Settings.License = LicenseType.Community;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                Logger.LogError("Необработанное исключение в потоке UI", e.Exception);
                MessageBox.Show($"Произошла ошибка: {e.Exception.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Logger.LogError("Критическое необработанное исключение", e.ExceptionObject as Exception);

            Logger.Initialize();
            Logger.LogInfo("Приложение запущено.");

            try
            {
                var dataService = new DataService();
                if (!dataService.HasData())
                {
                    Logger.LogInfo("Инициализация демонстрационных данных...");
                    dataService.InitializeSampleData();
                }
                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                Logger.LogError("Критическая ошибка при запуске", ex);
                MessageBox.Show($"Критическая ошибка: {ex.Message}\n\nПодробности записаны в лог-файл.",
                    "Ошибка запуска", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
