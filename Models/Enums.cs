namespace PersonalFinance.Models
{
    /// <summary>Тип финансовой операции.</summary>
    public enum TransactionType
    {
        Income,
        Expense
    }

    /// <summary>Категория доходов и расходов.</summary>
    public enum Category
    {
        Salary,
        Gift,
        Food,
        Transport,
        Housing,
        Utilities,
        Entertainment,
        Health,
        Education,
        Savings,
        Other
    }

    /// <summary>Тип счёта (кошелька).</summary>
    public enum AccountType
    {
        Cash,
        Card,
        BankAccount,
        Deposit,
        EWallet
    }

    /// <summary>Роль пользователя системы.</summary>
    public enum UserRole
    {
        Admin,
        User,
        Viewer
    }
}
