using System.Security.Claims;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?
                    .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return Guid.TryParse(userId, out var id) ? id : null;
            }
        }
    }
}
