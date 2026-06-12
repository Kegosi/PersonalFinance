using PersonalFinance.Models;
using PersonalFinance.Services;

namespace PersonalFinance.Forms
{
    /// <summary>Форма добавления и редактирования счёта (кошелька).</summary>
    public class AccountForm : Form
    {
        private TextBox _name, _currency, _note;
        private NumericUpDown _initialBalance;
        private ComboBox _type;
        private CheckBox _isActive;

        public Account Account { get; private set; }

        public AccountForm(Account account = null)
        {
            Account = account?.Clone();
            BuildUi();
            if (Account != null) FillFields();
        }

        private void BuildUi()
        {
            Text = Account == null ? "Новый счёт" : "Редактирование счёта";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(440, 300);
            Font = new Font("Segoe UI", 9f);

            int y = 20;
            _name = AddText("Название:", ref y);

            _type = AddCombo("Тип счёта:", ref y);
            _type.DataSource = Enum.GetValues(typeof(AccountType));

            _currency = AddText("Валюта:", ref y);
            _currency.Text = "руб.";

            _initialBalance = AddNumeric("Начальный баланс:", ref y, -1000000000, 1000000000, 2);
            _note = AddText("Примечание:", ref y);

            _isActive = new CheckBox { Text = "Активен", Location = new Point(180, y), AutoSize = true, Checked = true };
            Controls.Add(_isActive);
            y += 35;

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
            _name.Text = Account.Name;
            _type.SelectedItem = Account.Type;
            _currency.Text = Account.Currency;
            _initialBalance.Value = Account.InitialBalance;
            _note.Text = Account.Note;
            _isActive.Checked = Account.IsActive;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            var account = Account?.Clone() ?? new Account();
            account.Name = _name.Text.Trim();
            account.Type = (AccountType)_type.SelectedItem;
            account.Currency = _currency.Text.Trim();
            account.InitialBalance = _initialBalance.Value;
            account.Note = _note.Text.Trim();
            account.IsActive = _isActive.Checked;

            var errors = ValidationService.ValidateAccount(account);
            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors), "Проверка данных",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Account = account;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
