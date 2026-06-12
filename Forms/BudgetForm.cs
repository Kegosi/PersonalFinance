using PersonalFinance.Models;
using PersonalFinance.Services;

namespace PersonalFinance.Forms
{
    /// <summary>Форма добавления и редактирования месячного бюджета по категории.</summary>
    public class BudgetForm : Form
    {
        private ComboBox _category;
        private NumericUpDown _year, _month, _limit;
        private TextBox _note;

        public Budget Budget { get; private set; }

        public BudgetForm(Budget budget = null)
        {
            Budget = budget?.Clone();
            BuildUi();
            if (Budget != null) FillFields();
        }

        private void BuildUi()
        {
            Text = Budget == null ? "Новый бюджет" : "Редактирование бюджета";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(440, 280);
            Font = new Font("Segoe UI", 9f);

            int y = 20;
            _category = AddCombo("Категория:", ref y);
            _category.DataSource = Enum.GetValues(typeof(Category));

            _year = AddNumeric("Год:", ref y, 2000, 2100, 0);
            _year.Value = DateTime.Today.Year;

            _month = AddNumeric("Месяц:", ref y, 1, 12, 0);
            _month.Value = DateTime.Today.Month;

            _limit = AddNumeric("Лимит:", ref y, 0, 1000000000, 2);
            _note = AddText("Примечание:", ref y);

            var ok = new Button { Text = "Сохранить", Location = new Point(230, y + 10), Width = 90 };
            ok.Click += Save_Click;
            var cancel = new Button { Text = "Отмена", Location = new Point(330, y + 10), Width = 90, DialogResult = DialogResult.Cancel };
            AcceptButton = ok;
            CancelButton = cancel;
            Controls.AddRange(new Control[] { ok, cancel });
        }

        private TextBox AddText(string label, ref int y)
        {
            Controls.Add(new Label { Text = label, Location = new Point(20, y + 3), AutoSize = true });
            var box = new TextBox { Location = new Point(180, y), Width = 240 };
            Controls.Add(box);
            y += 33;
            return box;
        }

        private NumericUpDown AddNumeric(string label, ref int y, decimal min, decimal max, int decimals)
        {
            Controls.Add(new Label { Text = label, Location = new Point(20, y + 3), AutoSize = true });
            var num = new NumericUpDown { Location = new Point(180, y), Width = 240, Minimum = min, Maximum = max, DecimalPlaces = decimals };
            Controls.Add(num);
            y += 33;
            return num;
        }

        private ComboBox AddCombo(string label, ref int y)
        {
            Controls.Add(new Label { Text = label, Location = new Point(20, y + 3), AutoSize = true });
            var combo = new ComboBox { Location = new Point(180, y), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            Controls.Add(combo);
            y += 33;
            return combo;
        }

        private void FillFields()
        {
            _category.SelectedItem = Budget.Category;
            _year.Value = Budget.Year;
            _month.Value = Budget.Month;
            _limit.Value = Budget.Limit;
            _note.Text = Budget.Note;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            var budget = Budget?.Clone() ?? new Budget();
            budget.Category = (Category)_category.SelectedItem;
            budget.Year = (int)_year.Value;
            budget.Month = (int)_month.Value;
            budget.Limit = _limit.Value;
            budget.Note = _note.Text.Trim();

            var errors = ValidationService.ValidateBudget(budget);
            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors), "Проверка данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Budget = budget;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
