using PersonalFinance.Controllers;
using PersonalFinance.Models;
using PersonalFinance.Services;

namespace PersonalFinance.Forms
{
    /// <summary>
    /// Главное окно приложения. Связывает контроллеры с представлением (DataGridView),
    /// реализует операции CRUD, фильтрацию, отчёты, статистику, отмену/повтор и автосохранение.
    /// </summary>
    public class MainForm : Form
    {
        private readonly User _user;
        private readonly DataService _dataService;
        private readonly AccountController _accountController;
        private readonly BudgetController _budgetController;
        private readonly TransactionController _transactionController;
        private readonly ExportService _exportService;
        private readonly UndoManager _undoManager;
        private readonly FinanceState _state;

        private DataGridView _transactionGrid, _accountGrid, _budgetGrid;
        private TextBox _searchBox;
        private ComboBox _categoryFilter, _typeFilter;
        private ToolStripStatusLabel _statusLabel, _balanceLabel;
        private System.Windows.Forms.Timer _autoSaveTimer;

        public MainForm(User user)
        {
            _user = user;
            _dataService = new DataService();
            _accountController = new AccountController(_dataService);
            _budgetController = new BudgetController(_dataService);
            _transactionController = new TransactionController(_dataService, _accountController, _budgetController);
            _exportService = new ExportService(_accountController, _transactionController, _budgetController);
            _state = new FinanceState(_accountController, _transactionController, _budgetController);
            _undoManager = new UndoManager(_state);

            BuildUi();
            BindData();

            _accountController.StatusChanged += (s, msg) => SetStatus(msg);
            _budgetController.StatusChanged += (s, msg) => SetStatus(msg);
            _transactionController.StatusChanged += (s, msg) => SetStatus(msg);

            _accountController.DataChanged += (s, e) => UpdateBalance();
            _transactionController.DataChanged += (s, e) => UpdateBalance();

            UpdateBalance();
            StartAutoSave();
        }

        private void BuildUi()
        {
            Text = $"Учёт личных финансов — пользователь: {_user.Login} ({_user.Role})";
            WindowState = FormWindowState.Maximized;
            Font = new Font("Segoe UI", 9f);
            KeyPreview = true;
            KeyDown += MainForm_KeyDown;

            // ----- Меню -----
            var menu = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("Файл");
            fileMenu.DropDownItems.Add("Сформировать отчёт...", null, (s, e) => OpenReports());
            fileMenu.DropDownItems.Add("Сохранить", null, (s, e) => SaveAll());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Выход", null, (s, e) => Close());

            var editMenu = new ToolStripMenuItem("Правка");
            editMenu.DropDownItems.Add("Отменить (Ctrl+Z)", null, (s, e) => Undo());
            editMenu.DropDownItems.Add("Повторить (Ctrl+Y)", null, (s, e) => Redo());

            var viewMenu = new ToolStripMenuItem("Вид");
            viewMenu.DropDownItems.Add("Статистика расходов", null, (s, e) => new StatisticsForm(_transactionController).ShowDialog());

            var helpMenu = new ToolStripMenuItem("Справка");
            helpMenu.DropDownItems.Add("О программе", null, (s, e) => new AboutForm().ShowDialog());

            menu.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, viewMenu, helpMenu });

            // ----- Панель инструментов -----
            var toolStrip = new ToolStrip();
            toolStrip.Items.Add(new ToolStripButton("Добавить", null, (s, e) => AddCurrent()));
            toolStrip.Items.Add(new ToolStripButton("Изменить", null, (s, e) => EditCurrent()));
            toolStrip.Items.Add(new ToolStripButton("Удалить", null, (s, e) => DeleteCurrent()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Статистика", null, (s, e) => new StatisticsForm(_transactionController).ShowDialog()));
            toolStrip.Items.Add(new ToolStripButton("Отчёт", null, (s, e) => OpenReports()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Отменить", null, (s, e) => Undo()));
            toolStrip.Items.Add(new ToolStripButton("Повторить", null, (s, e) => Redo()));

            // ----- Панель фильтра -----
            var filterPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            filterPanel.Controls.Add(new Label { Text = "Поиск:", Location = new Point(8, 12), AutoSize = true });
            _searchBox = new TextBox { Location = new Point(60, 9), Width = 200 };
            _searchBox.TextChanged += (s, e) => ApplyTransactionFilter();
            filterPanel.Controls.Add(_searchBox);

            filterPanel.Controls.Add(new Label { Text = "Категория:", Location = new Point(280, 12), AutoSize = true });
            _categoryFilter = new ComboBox { Location = new Point(350, 9), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            _categoryFilter.Items.Add("Все");
            foreach (var c in Enum.GetValues(typeof(Category))) _categoryFilter.Items.Add(c);
            _categoryFilter.SelectedIndex = 0;
            _categoryFilter.SelectedIndexChanged += (s, e) => ApplyTransactionFilter();
            filterPanel.Controls.Add(_categoryFilter);

            filterPanel.Controls.Add(new Label { Text = "Тип:", Location = new Point(520, 12), AutoSize = true });
            _typeFilter = new ComboBox { Location = new Point(560, 9), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            _typeFilter.Items.AddRange(new object[] { "Все", "Доход", "Расход" });
            _typeFilter.SelectedIndex = 0;
            _typeFilter.SelectedIndexChanged += (s, e) => ApplyTransactionFilter();
            filterPanel.Controls.Add(_typeFilter);

            // ----- Вкладки с таблицами -----
            var tabs = new TabControl { Dock = DockStyle.Fill };

            var transactionTab = new TabPage("Операции");
            _transactionGrid = CreateGrid();
            _transactionGrid.CellDoubleClick += (s, e) => EditCurrent();
            _transactionGrid.CellFormatting += TransactionGrid_CellFormatting;
            transactionTab.Controls.Add(_transactionGrid);
            transactionTab.Controls.Add(filterPanel);

            var accountTab = new TabPage("Счета");
            _accountGrid = CreateGrid();
            _accountGrid.CellDoubleClick += (s, e) => EditCurrent();
            _accountGrid.CellFormatting += AccountGrid_CellFormatting;
            accountTab.Controls.Add(_accountGrid);

            var budgetTab = new TabPage("Бюджеты");
            _budgetGrid = CreateGrid();
            _budgetGrid.CellDoubleClick += (s, e) => EditCurrent();
            _budgetGrid.CellFormatting += BudgetGrid_CellFormatting;
            budgetTab.Controls.Add(_budgetGrid);

            tabs.TabPages.AddRange(new[] { transactionTab, accountTab, budgetTab });

            // ----- Строка состояния -----
            var statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Готово") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _balanceLabel = new ToolStripStatusLabel("Общий баланс: 0,00");
            statusStrip.Items.Add(_statusLabel);
            statusStrip.Items.Add(_balanceLabel);

            Controls.Add(tabs);
            Controls.Add(toolStrip);
            Controls.Add(menu);
            Controls.Add(statusStrip);
            MainMenuStrip = menu;
        }

        private static DataGridView CreateGrid() => new()
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None
        };

        private void BindData()
        {
            _transactionGrid.DataSource = _transactionController.Transactions;
            _accountGrid.DataSource = _accountController.Accounts;
            _budgetGrid.DataSource = _budgetController.Budgets;

            SetHeaders();
        }

        private void SetHeaders()
        {
            void Set(DataGridView grid, string col, string header)
            {
                if (grid.Columns.Contains(col)) grid.Columns[col].HeaderText = header;
            }
            void Hide(DataGridView grid, params string[] cols)
            {
                foreach (var c in cols) if (grid.Columns.Contains(c)) grid.Columns[c].Visible = false;
            }

            Set(_transactionGrid, "Date", "Дата");
            Set(_transactionGrid, "TypeName", "Тип");
            Set(_transactionGrid, "Category", "Категория");
            Set(_transactionGrid, "AccountName", "Счёт");
            Set(_transactionGrid, "Amount", "Сумма");
            Set(_transactionGrid, "Description", "Описание");
            Hide(_transactionGrid, "Id", "Type", "AccountId", "Note", "SignedAmount");

            Set(_accountGrid, "Name", "Название");
            Set(_accountGrid, "Type", "Тип");
            Set(_accountGrid, "Currency", "Валюта");
            Set(_accountGrid, "InitialBalance", "Начальный баланс");
            Set(_accountGrid, "Balance", "Текущий баланс");
            Set(_accountGrid, "IsActive", "Активен");
            Hide(_accountGrid, "Id", "Note", "CreatedDate", "IsNegative");

            Set(_budgetGrid, "Category", "Категория");
            Set(_budgetGrid, "Period", "Период");
            Set(_budgetGrid, "Limit", "Лимит");
            Set(_budgetGrid, "Spent", "Потрачено");
            Set(_budgetGrid, "Remaining", "Остаток");
            Set(_budgetGrid, "UsagePercent", "Использовано, %");
            Hide(_budgetGrid, "Id", "Year", "Month", "Note", "IsExceeded");
        }

        private void TransactionGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (_transactionGrid.Rows[e.RowIndex].DataBoundItem is not Transaction transaction) return;
            e.CellStyle.ForeColor = transaction.Type == TransactionType.Income ? Color.Green : Color.IndianRed;
        }

        private void AccountGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (_accountGrid.Rows[e.RowIndex].DataBoundItem is not Account account) return;
            if (account.IsNegative) e.CellStyle.ForeColor = Color.Red;
            else if (!account.IsActive) e.CellStyle.ForeColor = Color.Gray;
        }

        private void BudgetGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (_budgetGrid.Rows[e.RowIndex].DataBoundItem is not Budget budget) return;
            if (budget.IsExceeded) e.CellStyle.ForeColor = Color.Red;
            else if (budget.UsagePercent >= 80) e.CellStyle.ForeColor = Color.DarkOrange;
        }

        // ----- Определение активной вкладки -----
        private TabControl Tabs => Controls.OfType<TabControl>().First();
        private int ActiveTab => Tabs.SelectedIndex;

        private void ApplyTransactionFilter()
        {
            Category? category = _categoryFilter.SelectedItem is Category c ? c : null;
            TransactionType? type = _typeFilter.SelectedIndex switch
            {
                1 => TransactionType.Income,
                2 => TransactionType.Expense,
                _ => null
            };
            var result = _transactionController.Search(_searchBox.Text, category, type);
            _transactionGrid.DataSource = new System.ComponentModel.BindingList<Transaction>(result);
            SetHeaders();
            SetStatus($"Найдено операций: {result.Count}");
        }

        private void AddCurrent()
        {
            if (ActiveTab == 0)
            {
                if (!_accountController.Accounts.Any(a => a.IsActive))
                {
                    MessageBox.Show("Сначала добавьте хотя бы один счёт на вкладке «Счета».", "Новая операция",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                using var form = new TransactionForm(_accountController);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _undoManager.SaveState();
                    _transactionController.AddTransaction(form.Transaction);
                }
            }
            else if (ActiveTab == 1)
            {
                using var form = new AccountForm();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _undoManager.SaveState();
                    _accountController.AddAccount(form.Account);
                }
            }
            else
            {
                using var form = new BudgetForm();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _undoManager.SaveState();
                    _budgetController.AddBudget(form.Budget);
                    _budgetController.RecalculateSpent(_transactionController.Transactions);
                }
            }
        }

        private void EditCurrent()
        {
            if (ActiveTab == 0 && _transactionGrid.CurrentRow?.DataBoundItem is Transaction transaction)
            {
                using var form = new TransactionForm(_accountController, transaction);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _undoManager.SaveState();
                    _transactionController.UpdateTransaction(form.Transaction);
                }
            }
            else if (ActiveTab == 1 && _accountGrid.CurrentRow?.DataBoundItem is Account account)
            {
                using var form = new AccountForm(account);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _undoManager.SaveState();
                    _accountController.UpdateAccount(form.Account);
                }
            }
            else if (ActiveTab == 2 && _budgetGrid.CurrentRow?.DataBoundItem is Budget budget)
            {
                using var form = new BudgetForm(budget);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _undoManager.SaveState();
                    _budgetController.UpdateBudget(form.Budget);
                    _budgetController.RecalculateSpent(_transactionController.Transactions);
                }
            }
        }

        private void DeleteCurrent()
        {
            if (MessageBox.Show("Удалить выбранную запись?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            if (ActiveTab == 0 && _transactionGrid.CurrentRow?.DataBoundItem is Transaction transaction)
            {
                _undoManager.SaveState();
                _transactionController.DeleteTransaction(transaction.Id);
            }
            else if (ActiveTab == 1 && _accountGrid.CurrentRow?.DataBoundItem is Account account)
            {
                _undoManager.SaveState();
                _accountController.DeleteAccount(account.Id);
            }
            else if (ActiveTab == 2 && _budgetGrid.CurrentRow?.DataBoundItem is Budget budget)
            {
                _undoManager.SaveState();
                _budgetController.DeleteBudget(budget.Id);
            }
        }

        private void OpenReports()
        {
            using var form = new ReportForm(_exportService);
            form.ShowDialog();
        }

        private void Undo()
        {
            if (_undoManager.Undo()) { SetStatus("Операция отменена."); SetHeaders(); UpdateBalance(); }
            else SetStatus("Нечего отменять.");
        }

        private void Redo()
        {
            if (_undoManager.Redo()) { SetStatus("Операция повторена."); SetHeaders(); UpdateBalance(); }
            else SetStatus("Нечего повторять.");
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z) { Undo(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.Y) { Redo(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.S) { SaveAll(); e.Handled = true; }
        }

        private void SaveAll()
        {
            _dataService.SaveAccounts(_accountController.Accounts.ToList());
            _dataService.SaveTransactions(_transactionController.Transactions.ToList());
            _dataService.SaveBudgets(_budgetController.Budgets.ToList());
            SetStatus($"Данные сохранены ({DateTime.Now:HH:mm:ss}).");
        }

        private void StartAutoSave()
        {
            _autoSaveTimer = new System.Windows.Forms.Timer { Interval = 30000 };
            _autoSaveTimer.Tick += (s, e) => SaveAll();
            _autoSaveTimer.Start();
        }

        private void UpdateBalance()
            => _balanceLabel.Text = $"Общий баланс: {_accountController.TotalBalance:0.00}    " +
                                    $"Доходы: {_transactionController.TotalIncome:0.00}    " +
                                    $"Расходы: {_transactionController.TotalExpense:0.00}";

        private void SetStatus(string message) => _statusLabel.Text = message;

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveAll();
            Logger.LogInfo("Приложение завершает работу.");
            base.OnFormClosing(e);
        }
    }
}
