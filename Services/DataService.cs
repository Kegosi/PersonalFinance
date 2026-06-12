using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using PersonalFinance.Models;

namespace PersonalFinance.Services
{
    /// <summary>
    /// Сервис хранения данных. Отвечает за сериализацию и десериализацию
    /// коллекций предметной области в JSON-файлы каталога Data.
    /// </summary>
    public class DataService
    {
        private readonly string _dataDir;
        private readonly JsonSerializerOptions _options;

        public DataService()
        {
            _dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(_dataDir);

            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        private string PathFor(string name) => Path.Combine(_dataDir, name);

        public bool HasData() => File.Exists(PathFor("transactions.json"));

        // ----- Счета -----
        public List<Account> LoadAccounts() => Load<Account>("accounts.json");
        public void SaveAccounts(List<Account> accounts) => Save("accounts.json", accounts);

        // ----- Операции -----
        public List<Transaction> LoadTransactions() => Load<Transaction>("transactions.json");
        public void SaveTransactions(List<Transaction> transactions) => Save("transactions.json", transactions);

        // ----- Бюджеты -----
        public List<Budget> LoadBudgets() => Load<Budget>("budgets.json");
        public void SaveBudgets(List<Budget> budgets) => Save("budgets.json", budgets);

        // ----- Пользователи -----
        public List<User> LoadUsers() => Load<User>("users.json");
        public void SaveUsers(List<User> users) => Save("users.json", users);

        private List<T> Load<T>(string fileName)
        {
            var path = PathFor(fileName);
            if (!File.Exists(path)) return new List<T>();

            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<List<T>>(json, _options) ?? new List<T>();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Ошибка чтения {fileName}", ex);
                return new List<T>();
            }
        }

        private void Save<T>(string fileName, List<T> items)
        {
            try
            {
                var json = JsonSerializer.Serialize(items, _options);
                File.WriteAllText(PathFor(fileName), json);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Ошибка записи {fileName}", ex);
            }
        }

        /// <summary>Заполняет хранилище демонстрационными данными при первом запуске.</summary>
        public void InitializeSampleData()
        {
            var accounts = new List<Account>
            {
                new Account { Id = 1, Name = "Наличные", Type = AccountType.Cash, Currency = "руб.", InitialBalance = 5000, Balance = 5000, Note = "Кошелёк", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-6) },
                new Account { Id = 2, Name = "Карта Сбербанк", Type = AccountType.Card, Currency = "руб.", InitialBalance = 45000, Balance = 45000, Note = "Зарплатная карта", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-6) },
                new Account { Id = 3, Name = "Накопительный счёт", Type = AccountType.Deposit, Currency = "руб.", InitialBalance = 120000, Balance = 120000, Note = "Подушка безопасности", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-12) }
            };

            var today = DateTime.Today;
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, Type = TransactionType.Income, Category = Category.Salary, AccountId = 2, AccountName = "Карта Сбербанк", Amount = 65000, Date = new DateTime(today.Year, today.Month, 5), Description = "Заработная плата" },
                new Transaction { Id = 2, Type = TransactionType.Expense, Category = Category.Housing, AccountId = 2, AccountName = "Карта Сбербанк", Amount = 18000, Date = new DateTime(today.Year, today.Month, 6), Description = "Аренда квартиры" },
                new Transaction { Id = 3, Type = TransactionType.Expense, Category = Category.Food, AccountId = 1, AccountName = "Наличные", Amount = 3200, Date = new DateTime(today.Year, today.Month, 8), Description = "Продукты на неделю" },
                new Transaction { Id = 4, Type = TransactionType.Expense, Category = Category.Transport, AccountId = 2, AccountName = "Карта Сбербанк", Amount = 1500, Date = new DateTime(today.Year, today.Month, 9), Description = "Проездной билет" },
                new Transaction { Id = 5, Type = TransactionType.Expense, Category = Category.Entertainment, AccountId = 2, AccountName = "Карта Сбербанк", Amount = 2400, Date = new DateTime(today.Year, today.Month, 10), Description = "Кино и кафе" },
                new Transaction { Id = 6, Type = TransactionType.Expense, Category = Category.Utilities, AccountId = 2, AccountName = "Карта Сбербанк", Amount = 4100, Date = new DateTime(today.Year, today.Month, 11), Description = "Коммунальные услуги" },
                new Transaction { Id = 7, Type = TransactionType.Income, Category = Category.Gift, AccountId = 1, AccountName = "Наличные", Amount = 5000, Date = new DateTime(today.Year, today.Month, 12), Description = "Подарок на день рождения" },
                new Transaction { Id = 8, Type = TransactionType.Expense, Category = Category.Food, AccountId = 1, AccountName = "Наличные", Amount = 2750, Date = new DateTime(today.Year, today.Month, 13), Description = "Продукты в супермаркете" },
                new Transaction { Id = 9, Type = TransactionType.Expense, Category = Category.Health, AccountId = 2, AccountName = "Карта Сбербанк", Amount = 1800, Date = new DateTime(today.Year, today.Month, 14), Description = "Аптека и витамины" },
                new Transaction { Id = 10, Type = TransactionType.Expense, Category = Category.Education, AccountId = 2, AccountName = "Карта Сбербанк", Amount = 4500, Date = new DateTime(today.Year, today.Month, 15), Description = "Онлайн-курс по программированию" },
                new Transaction { Id = 11, Type = TransactionType.Expense, Category = Category.Transport, AccountId = 1, AccountName = "Наличные", Amount = 900, Date = new DateTime(today.Year, today.Month, 16), Description = "Поездка на такси" },
                new Transaction { Id = 12, Type = TransactionType.Income, Category = Category.Other, AccountId = 2, AccountName = "Карта Сбербанк", Amount = 12000, Date = new DateTime(today.Year, today.Month, 17), Description = "Подработка (фриланс)" },
                new Transaction { Id = 13, Type = TransactionType.Expense, Category = Category.Food, AccountId = 1, AccountName = "Наличные", Amount = 1600, Date = new DateTime(today.Year, today.Month, 18), Description = "Кофейня и обед" },
                new Transaction { Id = 14, Type = TransactionType.Expense, Category = Category.Entertainment, AccountId = 2, AccountName = "Карта Сбербанк", Amount = 3200, Date = new DateTime(today.Year, today.Month, 19), Description = "Подписки и игры" },
                new Transaction { Id = 15, Type = TransactionType.Expense, Category = Category.Savings, AccountId = 2, AccountName = "Карта Сбербанк", Amount = 10000, Date = new DateTime(today.Year, today.Month, 20), Description = "Пополнение накопительного счёта" },
                new Transaction { Id = 16, Type = TransactionType.Expense, Category = Category.Utilities, AccountId = 2, AccountName = "Карта Сбербанк", Amount = 1200, Date = new DateTime(today.Year, today.Month, 21), Description = "Мобильная связь и интернет" },
                new Transaction { Id = 17, Type = TransactionType.Expense, Category = Category.Health, AccountId = 2, AccountName = "Карта Сбербанк", Amount = 2600, Date = new DateTime(today.Year, today.Month, 22), Description = "Приём у врача" },
                new Transaction { Id = 18, Type = TransactionType.Income, Category = Category.Salary, AccountId = 2, AccountName = "Карта Сбербанк", Amount = 8000, Date = new DateTime(today.Year, today.Month, 25), Description = "Премия за проект" }
            };

            // Приведение начальных балансов к итоговым с учётом операций.
            foreach (var account in accounts)
            {
                var delta = transactions.Where(t => t.AccountId == account.Id).Sum(t => t.SignedAmount);
                account.Balance = account.InitialBalance + delta;
            }

            var budgets = new List<Budget>
            {
                new Budget { Id = 1, Category = Category.Food, Year = today.Year, Month = today.Month, Limit = 15000, Note = "Продукты и питание" },
                new Budget { Id = 2, Category = Category.Transport, Year = today.Year, Month = today.Month, Limit = 3000, Note = "Проезд и такси" },
                new Budget { Id = 3, Category = Category.Entertainment, Year = today.Year, Month = today.Month, Limit = 5000, Note = "Развлечения" },
                new Budget { Id = 4, Category = Category.Utilities, Year = today.Year, Month = today.Month, Limit = 6000, Note = "Коммунальные услуги и связь" },
                new Budget { Id = 5, Category = Category.Health, Year = today.Year, Month = today.Month, Limit = 5000, Note = "Здоровье и аптека" },
                new Budget { Id = 6, Category = Category.Education, Year = today.Year, Month = today.Month, Limit = 6000, Note = "Обучение и курсы" }
            };

            var users = new List<User>
            {
                new User { Id = 1, Login = "admin", PasswordHash = AuthService.HashPassword("admin"), Role = UserRole.Admin, CreatedAt = DateTime.Now },
                new User { Id = 2, Login = "user", PasswordHash = AuthService.HashPassword("123456"), Role = UserRole.User, CreatedAt = DateTime.Now }
            };

            SaveAccounts(accounts);
            SaveTransactions(transactions);
            SaveBudgets(budgets);
            SaveUsers(users);
            Logger.LogInfo("Демонстрационные данные инициализированы.");
        }
    }
}
