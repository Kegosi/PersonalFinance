using System.Text.Json.Serialization;

namespace PersonalFinance.Models
{
    /// <summary>Счёт (кошелёк) пользователя — источник или приёмник денежных средств.</summary>
    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public string Currency { get; set; } = "руб.";
        public decimal InitialBalance { get; set; }
        public decimal Balance { get; set; }
        public string Note { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }

        [JsonIgnore]
        public bool IsNegative => Balance < 0;

        public Account Clone() => (Account)MemberwiseClone();

        public override string ToString() => $"{Name} ({Balance:0.00} {Currency})";
    }
}
