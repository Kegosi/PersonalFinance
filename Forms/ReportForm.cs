using System.Data;
using PersonalFinance.Services;

namespace PersonalFinance.Forms
{
    /// <summary>Форма формирования и экспорта отчётов в форматах PDF, Excel и CSV.</summary>
    public class ReportForm : Form
    {
        private readonly ExportService _exportService;
        private ComboBox _reportType, _format;

        public ReportForm(ExportService exportService)
        {
            _exportService = exportService;
            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Формирование отчёта";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(400, 190);
            Font = new Font("Segoe UI", 9f);

            Controls.Add(new Label { Text = "Тип отчёта:", Location = new Point(20, 25), AutoSize = true });
            _reportType = new ComboBox { Location = new Point(150, 22), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            _reportType.Items.AddRange(new object[] { "Операции", "Счета", "Бюджеты" });
            _reportType.SelectedIndex = 0;
            Controls.Add(_reportType);

            Controls.Add(new Label { Text = "Формат:", Location = new Point(20, 65), AutoSize = true });
            _format = new ComboBox { Location = new Point(150, 62), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            _format.Items.AddRange(ReportFactory.GetAvailableFormats().Cast<object>().ToArray());
            _format.SelectedIndex = 0;
            Controls.Add(_format);

            var export = new Button { Text = "Экспортировать", Location = new Point(150, 120), Width = 130 };
            export.Click += Export_Click;
            var cancel = new Button { Text = "Закрыть", Location = new Point(290, 120), Width = 80, DialogResult = DialogResult.Cancel };
            AcceptButton = export;
            CancelButton = cancel;
            Controls.AddRange(new Control[] { export, cancel });
        }

        private void Export_Click(object sender, EventArgs e)
        {
            string format = _format.SelectedItem.ToString();
            DataTable data;
            string title;

            switch (_reportType.SelectedIndex)
            {
                case 1:
                    data = _exportService.BuildAccountsTable();
                    title = "Отчёт: Счета";
                    break;
                case 2:
                    data = _exportService.BuildBudgetsTable();
                    title = "Отчёт: Бюджеты";
                    break;
                default:
                    data = _exportService.BuildTransactionsTable();
                    title = "Отчёт: Операции";
                    break;
            }

            var report = ReportFactory.CreateReport(format);
            using var dialog = new SaveFileDialog
            {
                FileName = "report" + report.FileExtension,
                Filter = $"{report.Description} (*{report.FileExtension})|*{report.FileExtension}"
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                _exportService.Export(data, format, dialog.FileName, title);
                MessageBox.Show($"Отчёт сохранён:\n{dialog.FileName}", "Готово",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка формирования отчёта", ex);
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
