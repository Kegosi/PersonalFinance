using PersonalFinance.Models;

namespace PersonalFinance.Services
{
    /// <summary>Проверка корректности данных моделей перед сохранением.</summary>
    public static class ValidationService
    {
        public static List<string> ValidateTransaction(Transaction transaction)
        {
            var errors = new List<string>();
            if (transaction.Amount <= 0)
                errors.Add("Сумма операции должна быть больше нуля.");
            if (transaction.AccountId <= 0)
                errors.Add("Не выбран счёт операции.");
            if (string.IsNullOrWhiteSpace(transaction.Description))
                errors.Add("Не указано описание операции.");
            if (transaction.Date > DateTime.Today.AddDays(1))
                errors.Add("Дата операции не может быть в будущем.");
            return errors;
        }

        public static List<string> ValidateAccount(Account account)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(account.Name))
                errors.Add("Не указано название счёта.");
            if (string.IsNullOrWhiteSpace(account.Currency))
                errors.Add("Не указана валюта счёта.");
            return errors;
        }

        public static List<string> ValidateBudget(Budget budget)
        {
            var errors = new List<string>();
            if (budget.Limit <= 0)
                errors.Add("Лимит бюджета должен быть больше нуля.");
            if (budget.Month < 1 || budget.Month > 12)
                errors.Add("Месяц должен быть в диапазоне от 1 до 12.");
            if (budget.Year < 2000 || budget.Year > 2100)
                errors.Add("Указан некорректный год.");
            return errors;
        }
    }
}
