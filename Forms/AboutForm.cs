namespace PersonalFinance.Forms
{
    /// <summary>Окно «О программе».</summary>
    public class AboutForm : Form
    {
        public AboutForm()
        {
            Text = "О программе";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(460, 230);
            Font = new Font("Segoe UI", 9f);

            var title = new Label
            {
                Text = "Учёт личных финансов",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var info = new Label
            {
                Text = "Итоговый проект по ПМ.02 «Осуществление интеграции программных модулей».\n\n" +
                       "Архитектура: Model–View–Controller (MVC).\n" +
                       "Паттерны: Наблюдатель, Фабричный метод, Стратегия, Снимок (Memento).\n" +
                       "Технологии: C#, .NET 9, Windows Forms, System.Text.Json.\n\n" +
                       "Выполнил: Маслов Д.В., группа ИСс-31.\n" +
                       "Версия 1.0",
                Location = new Point(20, 55),
                Size = new Size(420, 150)
            };

            var ok = new Button { Text = "OK", Location = new Point(360, 195), Width = 80, DialogResult = DialogResult.OK };
            AcceptButton = ok;
            Controls.AddRange(new Control[] { title, info, ok });
        }
    }
}
