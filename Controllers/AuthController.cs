using PersonalFinance.Models;
using PersonalFinance.Services;

namespace PersonalFinance.Controllers
{
    /// <summary>Контроллер авторизации пользователей.</summary>
    public class AuthController
    {
        private readonly DataService _dataService;
        private readonly List<User> _users;

        public AuthController(DataService dataService)
        {
            _dataService = dataService;
            _users = _dataService.LoadUsers();
        }

        public User CurrentUser { get; private set; }

        /// <summary>Проверяет логин и пароль. Возвращает пользователя или null.</summary>
        public User Login(string login, string password)
        {
            var user = _users.FirstOrDefault(u =>
                u.IsActive && string.Equals(u.Login, login, StringComparison.OrdinalIgnoreCase));

            if (user != null && AuthService.Verify(password, user.PasswordHash))
            {
                user.LastLogin = DateTime.Now;
                _dataService.SaveUsers(_users);
                CurrentUser = user;
                Logger.LogInfo($"Вход пользователя: {user.Login} ({user.Role}).");
                return user;
            }

            Logger.LogWarning($"Неудачная попытка входа: {login}.");
            return null;
        }
    }
}
