using System.ComponentModel;
using PersonalFinance.Models;
using PersonalFinance.Services;

namespace PersonalFinance.Controllers
{
    /// <summary>
    /// Контроллер финансовых операций. Реализует CRUD, фильтрацию (паттерн «Стратегия»)
    /// и координирует контроллеры счетов и бюджетов: операция изменяет баланс счёта
    /// и учитывается в фактических расходах бюджета.
    /// </summary>
    public class TransactionController : BaseController
    {
        private readonly BindingList<Transaction> _transactions = new();
        private readonly AccountController _accountController;
        private readonly BudgetController _budgetController;
        private int _nextId = 1;

        public TransactionController(DataService dataService, AccountController accountController, BudgetController budgetController) : base(dataService)
        {
            _accountController = accountController;
            _budgetController = budgetController;
            LoadData();
            _budgetController.RecalculateSpent(_transactions);
        }

        public BindingList<Transaction> Transactions => _transactions;

        private void LoadData()
        {
            var loaded = DataService.LoadTransactions();
            _transactions.Clear();
            foreach (var transaction in loaded) _transactions.Add(transaction);
            _nextId = _transactions.Any() ? _transactions.Max(t => t.Id) + 1 : 1;
        }

        public void AddTransaction(Transaction transaction)
        {
            transaction.Id = _nextId++;
            var account = _accountController.GetById(transaction.AccountId);
            if (account != null) transaction.AccountName = account.Name;

            _transactions.Add(transaction);
            _accountController.ApplyAmount(transaction.AccountId, transaction.SignedAmount);
            Save();
            _budgetController.RecalculateSpent(_transactions);
            OnStatusChanged($"Добавлена операция: {transaction.TypeName} {transaction.Amount:0.00} ({transaction.Description})");
            OnDataChanged();
        }

        public void UpdateTransaction(Transaction transaction)
        {
            var index = _transactions.ToList().FindIndex(t => t.Id == transaction.Id);
            if (index < 0) return;

            // Откат влияния прежней операции на баланс счёта.
            var old = _transactions[index];
            _accountController.ApplyAmount(old.AccountId, -old.SignedAmount);

            var account = _accountController.GetById(transaction.AccountId);
            if (account != null) transaction.AccountName = account.Name;

            _transactions[index] = transaction;
            _accountController.ApplyAmount(transaction.AccountId, transaction.SignedAmount);
            Save();
            _budgetController.RecalculateSpent(_transactions);
            OnStatusChanged($"Обновлена операция: {transaction.Description}");
            OnDataChanged();
        }

        public void DeleteTransaction(int id)
        {
            var transaction = _transactions.FirstOrDefault(t => t.Id == id);
            if (transaction != null)
            {
                _accountController.ApplyAmount(transaction.AccountId, -transaction.SignedAmount);
                _transactions.Remove(transaction);
                Save();
                _budgetController.RecalculateSpent(_transactions);
                OnStatusChanged($"Удалена операция: {transaction.Description}");
                OnDataChanged();
            }
        }

        public Transaction GetById(int id) => _transactions.FirstOrDefault(t => t.Id == id);

        public List<Transaction> Search(string searchText, Category? category = null,
            TransactionType? type = null)
        {
            var context = new TransactionFilterContext();
            if (!string.IsNullOrWhiteSpace(searchText)) context.AddFilter(new SearchTextFilter(searchText));
            if (category.HasValue) context.AddFilter(new CategoryFilter(category.Value));
            if (type.HasValue) context.AddFilter(new TypeFilter(type.Value));
            return context.ApplyFilters(_transactions).ToList();
        }

        public decimal TotalIncome => _transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        public decimal TotalExpense => _transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        public void ReplaceAll(IEnumerable<Transaction> transactions)
        {
            _transactions.Clear();
            foreach (var transaction in transactions) _transactions.Add(transaction);
            _nextId = _transactions.Any() ? _transactions.Max(t => t.Id) + 1 : 1;
            Save();
            _budgetController.RecalculateSpent(_transactions);
            OnDataChanged();
        }

        private void Save() => DataService.SaveTransactions(_transactions.ToList());
    }
}
