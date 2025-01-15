using Core.Enums;
using DAL.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrowMate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetupController : ControllerBase
    {
        private readonly IAdminRepository _adminRepository;

        public SetupController(UnitOfWork unitOfWork)
        {
            _adminRepository = unitOfWork.AdminRepository;
        }

        [HttpPost("setup-database")]
        public async Task<IActionResult> SetupDatabase()
        {
            try
            {
                await _adminRepository.SetupDatabaseAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
