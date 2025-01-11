using Core.Models;
using DAL.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrowMate.Controllers
{
    [ApiController]
    public class GenericController : ControllerBase
    {
        protected readonly UnitOfWork _unitOfWork;
        protected readonly IRepository<AppUser> _userRepository;
        protected readonly ILogger _logger;
        public GenericController(UnitOfWork unitOfWork, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _userRepository = _unitOfWork.UserRepository;
            _logger = logger;
        }
    }
}
