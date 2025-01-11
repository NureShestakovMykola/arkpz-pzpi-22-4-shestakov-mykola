using Core.Enums;
using Core.Models;
using DAL.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrowMate.Controllers
{
    [ApiController]
    public class GenericController : ControllerBase
    {
        protected const string SessionUserIdString = "UserId";
        protected const string SessionUserString = "User";

        protected readonly UnitOfWork _unitOfWork;
        protected readonly IRepository<AppUser> _userRepository;
        protected readonly ILogger _logger;

        public GenericController(UnitOfWork unitOfWork, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _userRepository = _unitOfWork.UserRepository;
            _logger = logger;
        }

        protected async Task<bool> IsAuthorizedAsync()
        {
            var userId = HttpContext.Session.GetInt32(SessionUserIdString);

            return userId.HasValue;
        }

        protected async Task<bool> IsInRoleAsync(UserRole role)
        {
            var user = await GetCurrentSessionUserAsync();
            if (user == null)
            {
                return false;
            }

            return user.Role == role;
        }

        protected async Task<AppUser> GetCurrentSessionUserAsync()
        {
            var userId = HttpContext.Session.GetInt32(SessionUserIdString);
            if (!userId.HasValue)
            {
                return null;
            }

            if (HttpContext.Items[SessionUserString] != null
                && (HttpContext.Items[SessionUserString] as AppUser).Id == userId.Value)
            {
                return HttpContext.Items[SessionUserString] as AppUser;
            }            

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user != null)
            {
                HttpContext.Items[SessionUserString] = user;
                return user;
            }

            return null;
        }
    }
}
