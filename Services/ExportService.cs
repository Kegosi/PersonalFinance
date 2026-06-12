using System.Data;
using PersonalFinance.Controllers;

namespace PersonalFinance.Services
{
    /// <summary>
    /// Формирует наборы данных (DataTable) для отчётов и сохраняет их в файл
    /// через фабрику отчётов <see cref="ReportFactory"/>.
    /// </summary>
    public class ExportService
    {
        private readonly AccountController _accounts;
        private readonly TransactionController _transactions;
        private readonly BudgetController _budgets;

        public ExportService(AccountController accounts, TransactionController transactions, BudgetController budgets)
        {
            _accounts = accounts;
            _transactions = transactions;
            _budgets = budgets;
        }

        public DataTable BuildTransactionsTable()
        {
            var table = NewTable("ID", "Дата", "Тип", "Категория", "Счёт", "Сумма", "Описание");
            foreach (var t in _transactions.Transactions)
                table.Rows.Add(t.Id, t.Date.ToString("dd.MM.yyyy"), t.TypeName, t.Category,
                    t.AccountName, t.Amount.ToString("0.00"), t.Description);
            return table;
        }

        public DataTable BuildAccountsTable()
        {
            var table = NewTable("ID", "Название", "Тип", "Валюта", "Начальный баланс", "Текущий баланс", "Активен");
            foreach (var a in _accounts.Accounts)
                table.Rows.Add(a.Id, a.Name, a.Type, a.Currency,
                    a.InitialBalance.ToString("0.00"), a.Balance.ToString("0.00"), a.IsActive ? "да" : "нет");
            return table;
        }

        public DataTable BuildBudgetsTable()
        {
            var table = NewTable("ID", "Категория", "Период", "Лимит", "Потрачено", "Остаток", "Использовано, %");
            foreach (var b in _budgets.Budgets)
                table.Rows.Add(b.Id, b.Category, b.Period, b.Limit.ToString("0.00"),
                    b.Spent.ToString("0.00"), b.Remaining.ToString("0.00"), b.UsagePercent);
            return table;
        }

        public void Export(DataTable data, string format, string filePath, string title)
        {
            var report = ReportFactory.CreateReport(format);
            report.Generate(data, filePath, title);
            Logger.LogInfo($"Сформирован отчёт «{title}» в формате {format}: {filePath}");
        }

        private static DataTable NewTable(params string[] columns)
        {
            var table = new DataTable();
            foreach (var name in columns) table.Columns.Add(name);
            return table;
        }
    }
}
