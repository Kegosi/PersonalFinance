using PersonalFinance.Services;

namespace PersonalFinance.Controllers
{
    /// <summary>
    /// Базовый контроллер — общая основа для всех контроллеров приложения.
    /// Хранит ссылку на сервис данных и реализует механизм уведомления
    /// представления об изменениях (паттерн «Наблюдатель»).
    /// </summary>
    public abstract class BaseController
    {
        protected readonly DataService DataService;

        protected BaseController(DataService dataService)
        {
            DataService = dataService;
        }

        /// <summary>Возникает при изменении данных контроллера.</summary>
        public event EventHandler DataChanged;

        /// <summary>Возникает при изменении строки состояния.</summary>
        public event EventHandler<string> StatusChanged;

        protected virtual void OnDataChanged() => DataChanged?.Invoke(this, EventArgs.Empty);

        protected virtual void OnStatusChanged(string message) => StatusChanged?.Invoke(this, message);
    }
}
