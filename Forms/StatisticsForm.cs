using PersonalFinance.Controllers;
using PersonalFinance.Models;

namespace PersonalFinance.Forms
{
    /// <summary>Окно статистики: распределение расходов по категориям (отрисовка GDI+).</summary>
    public class StatisticsForm : Form
    {
        private readonly TransactionController _transactionController;

        public StatisticsForm(TransactionController transactionController)
        {
            _transactionController = transactionController;
            Text = "Статистика расходов по категориям";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(680, 480);
            Font = new Font("Segoe UI", 9f);
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var groups = _transactionController.Transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .Select(grp => new { Category = grp.Key, Sum = grp.Sum(t => t.Amount) })
                .Where(x => x.Sum > 0)
                .OrderByDescending(x => x.Sum)
                .ToList();

            using var titleFont = new Font("Segoe UI", 13f, FontStyle.Bold);
            g.DrawString("Расходы по категориям", titleFont, Brushes.Black, 20, 15);

            decimal totalIncome = _transactionController.TotalIncome;
            decimal totalExpense = _transactionController.TotalExpense;
            using var infoFont = new Font("Segoe UI", 9.5f);
            g.DrawString($"Доходы: {totalIncome:0.00}    Расходы: {totalExpense:0.00}    Баланс: {totalIncome - totalExpense:0.00}",
                infoFont, Brushes.DimGray, 20, 44);

            if (groups.Count == 0)
            {
                g.DrawString("Нет данных для отображения.", Font, Brushes.Gray, 20, 80);
                return;
            }

            decimal max = groups.Max(x => x.Sum);
            int top = 80;
            int barHeight = 30;
            int gap = 14;
            int labelWidth = 150;
            int valueWidth = 110;
            int chartWidth = ClientSize.Width - labelWidth - valueWidth - 40;
            Color[] palette = { Color.SteelBlue, Color.SeaGreen, Color.IndianRed, Color.Goldenrod, Color.MediumPurple, Color.Teal, Color.SlateGray, Color.Chocolate };

            for (int i = 0; i < groups.Count; i++)
            {
                var item = groups[i];
                int y = top + i * (barHeight + gap);
                int barWidth = max == 0 ? 0 : (int)((double)(item.Sum / max) * chartWidth);

                using var brush = new SolidBrush(palette[i % palette.Length]);
                g.DrawString(item.Category.ToString(), Font, Brushes.Black, 20, y + 6);
                g.FillRectangle(brush, labelWidth, y, Math.Max(barWidth, 2), barHeight);
                g.DrawString(item.Sum.ToString("0.00"), Font, Brushes.Black, labelWidth + barWidth + 8, y + 6);
            }
        }
    }
}
