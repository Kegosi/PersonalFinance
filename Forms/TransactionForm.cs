using PersonalFinance.Controllers;
using PersonalFinance.Models;
using PersonalFinance.Services;

namespace PersonalFinance.Forms
{
    /// <summary>Форма добавления и редактирования финансовой операции.</summary>
    public class TransactionForm : Form
    {
        private readonly AccountController _accountController;
        private ComboBox _type, _category, _account;
        private NumericUpDown _amount;
        private DateTimePicker _date;
        private TextBox _description, _note;

        public Transaction Transaction { get; private set; }

        public TransactionForm(AccountController accountController, Transaction transaction = null)
        {
            _accountController = accountController;
            Transaction = transaction?.Clone();
            BuildUi();
            if (Transaction != null) FillFields();
        }

        private void BuildUi()
        {
            Text = Transaction == null ? "Новая операция" : "Редактирование операции";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(440, 320);
            Font = new Font("Segoe UI", 9f);

            int y = 20;
            _type = AddCombo("Тип операции:", ref y);
            _type.DataSource = Enum.GetValues(typeof(TransactionType));
            _type.Format += (s, e) => e.Value = ((TransactionType)e.ListItem) == TransactionType.Income ? "Доход" : "Расход";

            _category = AddCombo("Категория:", ref y);
            _category.DataSource = Enum.GetValues(typeof(Category));

            _account = AddCombo("Счёт:", ref y);
            _account.DataSource = _accountController.Accounts.Where(a => a.IsActive).ToList();
            _account.DisplayMember = "Name";
            _account.ValueMember = "Id";

            _amount = AddNumeric("Сумма:", ref y, 0, 1000000000, 2);

            Controls.Add(new Label { Text = "Дата:", Location = new Point(20, y + 3), AutoSize = true });
            _date = new DateTimePicker { Location = new Point(180, y), Width = 240, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            Controls.Add(_date);
            y += 33;

            _description = AddText("Описание:", ref y);
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
            _type.SelectedItem = Transaction.Type;
            _category.SelectedItem = Transaction.Category;
            _account.SelectedValue = Transaction.AccountId;
            _amount.Value = Transaction.Amount;
            _date.Value = Transaction.Date == default ? DateTime.Today : Transaction.Date;
            _description.Text = Transaction.Description;
            _note.Text = Transaction.Note;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (_account.SelectedItem is not Account account)
            {
                MessageBox.Show("Выберите счёт операции.", "Проверка данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var transaction = Transaction?.Clone() ?? new Transaction();
            transaction.Type = (TransactionType)_type.SelectedItem;
            transaction.Category = (Category)_category.SelectedItem;
            transaction.AccountId = account.Id;
            transaction.AccountName = account.Name;
            transaction.Amount = _amount.Value;
            transaction.Date = _date.Value.Date;
            transaction.Description = _description.Text.Trim();
            transaction.Note = _note.Text.Trim();

            var errors = ValidationService.ValidateTransaction(transaction);
            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors), "Проверка данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Transaction = transaction;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
