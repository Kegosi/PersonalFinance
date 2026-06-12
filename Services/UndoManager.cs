using PersonalFinance.Controllers;
using PersonalFinance.Models;

namespace PersonalFinance.Services
{
    /// <summary>
    /// Паттерн «Снимок» (Memento). Хранит копию состояния счетов, операций и бюджетов
    /// на определённый момент времени.
    /// </summary>
    public class FinanceMemento
    {
        public List<Account> Accounts { get; }
        public List<Transaction> Transactions { get; }
        public List<Budget> Budgets { get; }
        public DateTime Timestamp { get; }

        public FinanceMemento(IEnumerable<Account> accounts, IEnumerable<Transaction> transactions, IEnumerable<Budget> budgets)
        {
            Accounts = accounts.Select(a => a.Clone()).ToList();
            Transactions = transactions.Select(t => t.Clone()).ToList();
            Budgets = budgets.Select(b => b.Clone()).ToList();
            Timestamp = DateTime.Now;
        }
    }

    /// <summary>Originator: создаёт и восстанавливает снимок состояния по контроллерам.</summary>
    public class FinanceState
    {
        private readonly AccountController _accounts;
        private readonly TransactionController _transactions;
        private readonly BudgetController _budgets;

        public FinanceState(AccountController accounts, TransactionController transactions, BudgetController budgets)
        {
            _accounts = accounts;
            _transactions = transactions;
            _budgets = budgets;
        }

        public FinanceMemento Save() => new(_accounts.Accounts, _transactions.Transactions, _budgets.Budgets);

        public void Restore(FinanceMemento memento)
        {
            _accounts.ReplaceAll(memento.Accounts);
            _transactions.ReplaceAll(memento.Transactions);
            _budgets.ReplaceAll(memento.Budgets);
        }
    }

    /// <summary>Caretaker: управляет историей снимков для операций отмены и повтора.</summary>
    public class UndoManager
    {
        private Stack<FinanceMemento> _undo = new();
        private readonly Stack<FinanceMemento> _redo = new();
        private readonly FinanceState _state;
        private const int MaxHistory = 50;

        public UndoManager(FinanceState state) => _state = state;

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        /// <summary>Сохраняет текущее состояние перед изменением данных.</summary>
        public void SaveState()
        {
            _undo.Push(_state.Save());
            _redo.Clear();

            if (_undo.Count > MaxHistory)
            {
                var list = _undo.ToList();
                list.RemoveAt(list.Count - 1);
                list.Reverse();
                _undo = new Stack<FinanceMemento>(list);
            }
        }

        public bool Undo()
        {
            if (_undo.Count == 0) return false;
            _redo.Push(_state.Save());
            _state.Restore(_undo.Pop());
            return true;
        }

        public bool Redo()
        {
            if (_redo.Count == 0) return false;
            _undo.Push(_state.Save());
            _state.Restore(_redo.Pop());
            return true;
        }
    }
}
