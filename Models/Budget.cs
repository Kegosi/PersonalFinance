using System.Text.Json.Serialization;

namespace PersonalFinance.Models
{
    /// <summary>Месячный лимит расходов по выбранной категории.</summary>
    public class Budget
    {
        public int Id { get; set; }
        public Category Category { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Limit { get; set; }
        public string Note { get; set; } = string.Empty;

        // Денормализованное поле (заполняется контроллером по фактическим расходам).
        public decimal Spent { get; set; }

        [JsonIgnore]
        public decimal Remaining => Limit - Spent;

        [JsonIgnore]
        public bool IsExceeded => Spent > Limit;

        [JsonIgnore]
        public int UsagePercent => Limit <= 0 ? 0 : (int)Math.Round(Spent / Limit * 100);

        [JsonIgnore]
        public string Period => $"{Month:00}.{Year}";

        public Budget Clone() => (Budget)MemberwiseClone();

        public override string ToString() => $"{Category} {Period}";
    }
}
