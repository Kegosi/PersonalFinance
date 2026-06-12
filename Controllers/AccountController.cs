using System.ComponentModel;
using PersonalFinance.Models;
using PersonalFinance.Services;

namespace PersonalFinance.Controllers
{
    /// <summary>Контроллер счетов: CRUD-операции и уведомления представления (паттерн «Наблюдатель»).</summary>
    public class AccountController : BaseController
    {
        private readonly BindingList<Account> _accounts = new();
        private int _nextId = 1;

        public AccountController(DataService dataService) : base(dataService)
        {
            LoadData();
        }

        public BindingList<Account> Accounts => _accounts;

        private void LoadData()
        {
            var loaded = DataService.LoadAccounts();
            _accounts.Clear();
            foreach (var account in loaded) _accounts.Add(account);
            _nextId = _accounts.Any() ? _accounts.Max(a => a.Id) + 1 : 1;
        }

        public void AddAccount(Account account)
        {
            account.Id = _nextId++;
            if (account.CreatedDate == default) account.CreatedDate = DateTime.Now;
            account.Balance = account.InitialBalance;
            _accounts.Add(account);
            Save();
            OnStatusChanged($"Добавлен счёт: {account.Name}");
            OnDataChanged();
        }

        public void UpdateAccount(Account account)
        {
            var index = _accounts.ToList().FindIndex(a => a.Id == account.Id);
            if (index >= 0)
            {
                _accounts[index] = account;
                Save();
                OnStatusChanged($"Обновлён счёт: {account.Name}");
                OnDataChanged();
            }
        }

        public void DeleteAccount(int id)
        {
            var account = _accounts.FirstOrDefault(a => a.Id == id);
            if (account != null)
            {
                _accounts.Remove(account);
                Save();
                OnStatusChanged($"Удалён счёт: {account.Name}");
                OnDataChanged();
            }
        }

        public Account GetById(int id) => _accounts.FirstOrDefault(a => a.Id == id);

        public decimal TotalBalance => _accounts.Sum(a => a.Balance);

        /// <summary>Изменяет баланс счёта на указанную сумму (со знаком).</summary>
        public void ApplyAmount(int accountId, decimal signedAmount)
        {
            var account = GetById(accountId);
            if (account != null)
            {
                account.Balance += signedAmount;
                Save();
                OnDataChanged();
            }
        }

        public void ReplaceAll(IEnumerable<Account> accounts)
        {
            _accounts.Clear();
            foreach (var account in accounts) _accounts.Add(account);
            _nextId = _accounts.Any() ? _accounts.Max(a => a.Id) + 1 : 1;
            Save();
            OnDataChanged();
        }

        private void Save() => DataService.SaveAccounts(_accounts.ToList());
    }
}
