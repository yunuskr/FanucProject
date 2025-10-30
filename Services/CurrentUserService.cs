using System.Security.Claims;

namespace FanucRelease.Services
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? Username { get; }
        string? FullName { get; }
        string? Role { get; }
        void SetFromClaims(ClaimsPrincipal user);
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly object _lock = new();
        private int? _userId;
        private string? _username;
        private string? _fullName;
        private string? _role;

        public int? UserId { get { lock (_lock) return _userId; } }
        public string? Username { get { lock (_lock) return _username; } }
        public string? FullName { get { lock (_lock) return _fullName; } }
        public string? Role { get { lock (_lock) return _role; } }

        public void SetFromClaims(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true) return;
            lock (_lock)
            {
                _userId = int.TryParse(user.FindFirst("UserId")?.Value, out var id) ? id : null;
                _username = user.FindFirst("Username")?.Value;
                _fullName = user.Identity?.Name;
                _role = user.FindFirst(ClaimTypes.Role)?.Value;
            }
        }
    }
}
