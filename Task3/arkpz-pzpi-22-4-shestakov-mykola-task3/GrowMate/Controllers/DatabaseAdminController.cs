using Core.Enums;
using DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrowMate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseAdminController : GenericController
    {
        private readonly IAdminRepository _adminRepository;

        public DatabaseAdminController(UnitOfWork unitOfWork, 
            ILogger<DatabaseAdminController> logger)
            : base(unitOfWork, logger)
        {
            _adminRepository = _unitOfWork.AdminRepository;
        }

        [HttpPost("backup-database")]
        public async Task<IActionResult> Backup(string path)
        {
            try
            {
                if (!await IsInRoleAsync(UserRole.AdminDB))
                {
                    return Unauthorized();
                }

                await _adminRepository.BackupDatabaseAsync(path);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("restore-database")]
        public async Task<IActionResult> Restore(string path)
        {
            try
            {
                if (!await IsInRoleAsync(UserRole.AdminDB))
                {
                    return Unauthorized();
                }

                await _adminRepository.RestoreDatabaseAsync(path);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
