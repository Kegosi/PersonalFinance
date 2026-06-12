using System.ComponentModel;
using PersonalFinance.Models;
using PersonalFinance.Services;

namespace PersonalFinance.Controllers
{
    /// <summary>Контроллер месячных бюджетов по категориям: CRUD и расчёт фактических расходов.</summary>
    public class BudgetController : BaseController
    {
        private readonly BindingList<Budget> _budgets = new();
        private int _nextId = 1;

        public BudgetController(DataService dataService) : base(dataService)
        {
            LoadData();
        }

        public BindingList<Budget> Budgets => _budgets;

        private void LoadData()
        {
            var loaded = DataService.LoadBudgets();
            _budgets.Clear();
            foreach (var budget in loaded) _budgets.Add(budget);
            _nextId = _budgets.Any() ? _budgets.Max(b => b.Id) + 1 : 1;
        }

        public void AddBudget(Budget budget)
        {
            budget.Id = _nextId++;
            _budgets.Add(budget);
            Save();
            OnStatusChanged($"Добавлен бюджет: {budget.Category} ({budget.Period})");
            OnDataChanged();
        }

        public void UpdateBudget(Budget budget)
        {
            var index = _budgets.ToList().FindIndex(b => b.Id == budget.Id);
            if (index >= 0)
            {
                _budgets[index] = budget;
                Save();
                OnStatusChanged($"Обновлён бюджет: {budget.Category} ({budget.Period})");
                OnDataChanged();
            }
        }

        public void DeleteBudget(int id)
        {
            var budget = _budgets.FirstOrDefault(b => b.Id == id);
            if (budget != null)
            {
                _budgets.Remove(budget);
                Save();
                OnStatusChanged($"Удалён бюджет: {budget.Category} ({budget.Period})");
                OnDataChanged();
            }
        }

        public Budget GetById(int id) => _budgets.FirstOrDefault(b => b.Id == id);

        public List<Budget> GetExceeded() => _budgets.Where(b => b.IsExceeded).ToList();

        /// <summary>Пересчитывает фактические расходы по каждому бюджету на основе операций.</summary>
        public void RecalculateSpent(IEnumerable<Transaction> transactions)
        {
            var list = transactions.ToList();
            foreach (var budget in _budgets)
            {
                budget.Spent = list
                    .Where(t => t.Type == TransactionType.Expense
                                && t.Category == budget.Category
                                && t.Date.Year == budget.Year
                                && t.Date.Month == budget.Month)
                    .Sum(t => t.Amount);
            }
            Save();
            OnDataChanged();
        }

        public void ReplaceAll(IEnumerable<Budget> budgets)
        {
            _budgets.Clear();
            foreach (var budget in budgets) _budgets.Add(budget);
            _nextId = _budgets.Any() ? _budgets.Max(b => b.Id) + 1 : 1;
            Save();
            OnDataChanged();
        }

        private void Save() => DataService.SaveBudgets(_budgets.ToList());
    }
}
