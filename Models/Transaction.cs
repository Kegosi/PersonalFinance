using System.Text.Json.Serialization;

namespace PersonalFinance.Models
{
    /// <summary>Финансовая операция — доход или расход по счёту.</summary>
    public class Transaction
    {
        public int Id { get; set; }
        public TransactionType Type { get; set; }
        public Category Category { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;

        // Денормализованное поле для отображения в таблице (заполняется контроллером).
        public string AccountName { get; set; } = string.Empty;

        /// <summary>Сумма со знаком: расход уменьшает, доход увеличивает баланс.</summary>
        [JsonIgnore]
        public decimal SignedAmount => Type == TransactionType.Expense ? -Amount : Amount;

        [JsonIgnore]
        public string TypeName => Type == TransactionType.Income ? "Доход" : "Расход";

        public Transaction Clone() => (Transaction)MemberwiseClone();

        public override string ToString() => $"{Date:dd.MM.yyyy} {TypeName} {Amount:0.00}";
    }
}
