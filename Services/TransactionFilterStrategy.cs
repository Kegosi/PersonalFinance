using PersonalFinance.Models;

namespace PersonalFinance.Services
{
    /// <summary>
    /// Паттерн «Стратегия». Каждая стратегия инкапсулирует один критерий
    /// фильтрации списка операций; контекст объединяет произвольный набор стратегий.
    /// </summary>
    public interface ITransactionFilter
    {
        IEnumerable<Transaction> Filter(IEnumerable<Transaction> transactions);
        string Description { get; }
    }

    /// <summary>Фильтр по подстроке в описании, категории или счёте.</summary>
    public class SearchTextFilter : ITransactionFilter
    {
        private readonly string _text;
        public SearchTextFilter(string text) => _text = (text ?? string.Empty).ToLowerInvariant();

        public IEnumerable<Transaction> Filter(IEnumerable<Transaction> transactions)
        {
            if (string.IsNullOrWhiteSpace(_text)) return transactions;
            return transactions.Where(t =>
                t.Description.ToLowerInvariant().Contains(_text) ||
                t.AccountName.ToLowerInvariant().Contains(_text) ||
                t.Category.ToString().ToLowerInvariant().Contains(_text));
        }

        public string Description => $"Поиск: \"{_text}\"";
    }

    /// <summary>Фильтр по категории операции.</summary>
    public class CategoryFilter : ITransactionFilter
    {
        private readonly Category _category;
        public CategoryFilter(Category category) => _category = category;

        public IEnumerable<Transaction> Filter(IEnumerable<Transaction> transactions)
            => transactions.Where(t => t.Category == _category);

        public string Description => $"Категория: {_category}";
    }

    /// <summary>Фильтр по типу операции (доход или расход).</summary>
    public class TypeFilter : ITransactionFilter
    {
        private readonly TransactionType _type;
        public TypeFilter(TransactionType type) => _type = type;

        public IEnumerable<Transaction> Filter(IEnumerable<Transaction> transactions)
            => transactions.Where(t => t.Type == _type);

        public string Description => $"Тип: {(_type == TransactionType.Income ? "Доход" : "Расход")}";
    }

    /// <summary>Фильтр по диапазону дат.</summary>
    public class DateRangeFilter : ITransactionFilter
    {
        private readonly DateTime _from;
        private readonly DateTime _to;
        public DateRangeFilter(DateTime from, DateTime to)
        {
            _from = from.Date;
            _to = to.Date;
        }

        public IEnumerable<Transaction> Filter(IEnumerable<Transaction> transactions)
            => transactions.Where(t => t.Date.Date >= _from && t.Date.Date <= _to);

        public string Description => $"Период: {_from:dd.MM.yyyy}–{_to:dd.MM.yyyy}";
    }

    /// <summary>Контекст фильтрации: применяет последовательность стратегий.</summary>
    public class TransactionFilterContext
    {
        private readonly List<ITransactionFilter> _filters = new();

        public void AddFilter(ITransactionFilter filter) => _filters.Add(filter);

        public void ClearFilters() => _filters.Clear();

        public IEnumerable<Transaction> ApplyFilters(IEnumerable<Transaction> transactions)
        {
            var result = transactions;
            foreach (var filter in _filters)
                result = filter.Filter(result);
            return result;
        }

        public string GetFilterDescription()
            => _filters.Count == 0 ? "Без фильтров" : string.Join(" + ", _filters.Select(f => f.Description));
    }
}
