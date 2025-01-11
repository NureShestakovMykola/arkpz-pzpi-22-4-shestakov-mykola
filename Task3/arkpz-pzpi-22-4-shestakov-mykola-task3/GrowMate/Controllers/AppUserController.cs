using Core.Enums;
using Core.Helpers;
using Core.Models;
using DAL.Repositories;
using GrowMate.Models.AppUser;
using GrowMate.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Net.Mime.MediaTypeNames;

namespace GrowMate.Controllers
{
    [Route("api/[controller]")]
    public class AppUserController : GenericController
    {
        private const string SessionPasswordChangeCodeString = "PasswordChangeCode";
        private const string SessionPasswordChangeUserIdString = "PasswordChangeUserId";
        private readonly IFileRepository _fileRepository;
        private readonly EmailService _emailService;
        public AppUserController(UnitOfWork unitOfWork, ILogger<AppUserController> logger, 
            IFileRepository fileRepository, EmailService emailService)
            : base(unitOfWork, logger)
        {
            _fileRepository = fileRepository;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            if (model.Password.Trim() != model.RepeatPassword.Trim())
            {
                return BadRequest();
            }

            var user = new AppUser();
            user.MapFrom(model);
            user.PasswordHash = HashPassword(model.Password.Trim());
            user.Role = Core.Enums.UserRole.User;

            try
            {
                var existingUser = (await _userRepository.GetAsync
                    (u => u.Email == model.Email)).FirstOrDefault();

                if (existingUser != null)
                {
                    return NotFound("Email taken");
                }

                await _userRepository.AddAsync(user);
                await _unitOfWork.Save();

                HttpContext.Session.SetInt32(SessionUserIdString, user.Id);
                return Ok(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (await IsAuthorizedAsync())
            {
                return BadRequest("The session is still in progress");
            }

            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            try
            {
                var user = (await _userRepository.GetAsync
                    (u => u.Email == model.Email)).FirstOrDefault();
                
                if (user == null)
                {
                    return NotFound();
                }

                if (HashPassword(model.Password.Trim()) != user.PasswordHash)
                {
                    return BadRequest("Wrong password");
                }

                HttpContext.Session.SetInt32(SessionUserIdString, user.Id);
                return Ok(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (!await IsAuthorizedAsync())
            {
                return BadRequest("No sessions to terminate");
            }

            HttpContext.Session.Remove(SessionUserIdString);
            HttpContext.Items.Remove(SessionUserString);

            return Ok();
        }

        [HttpPut("edit")]
        public async Task<IActionResult> Edit([FromForm] EditUserModel model)
        {
            if (!await IsAuthorizedAsync())
            {
                return Unauthorized();
            }

            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            try
            {
                var user = await GetCurrentSessionUserAsync();

                if (user == null)
                {
                    return NotFound();
                }

                user.MapFrom(model);

                if (model.NewImage != null)
                {
                    if (user.ImageExtension != null)
                    {
                        await _fileRepository.DeleteFileAsync(Core.Enums.FileType.ProfileImage, $"{user.Id}{user.ImageExtension}");
                    }
                    
                    var extension = Path.GetExtension(model.NewImage.FileName);

                    await _fileRepository.SaveFileAsync(model.NewImage, Core.Enums.FileType.ProfileImage, $"{user.Id}{extension}");
                    user.ImageExtension = extension;
                }

                await _userRepository.UpdateAsync(user);
                await _unitOfWork.Save();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!await IsAuthorizedAsync())
            {
                return Unauthorized();
            }

            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            try
            {
                var user = await GetCurrentSessionUserAsync();

                if (user == null)
                {
                    return NotFound();
                }

                if (HashPassword(model.OldPassword.Trim()) != user.PasswordHash)
                {
                    return BadRequest("Wrong password");
                }

                user.PasswordHash = HashPassword(model.NewPassword.Trim());

                await _userRepository.UpdateAsync(user);
                await _unitOfWork.Save();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("get-change-password-code")]
        public async Task<IActionResult> GetChangePasswordCode(string email)
        {
            if (email.IsNullOrEmpty())
            {
                _logger.LogError("Email was not received");
                return BadRequest();
            }

            try
            {
                var user = (await _userRepository.GetAsync
                    (u => u.Email == email)).FirstOrDefault();

                if (user == null)
                {
                    return NotFound();
                }

                var rand = new Random();
                var code = rand.Next(10000, 99999);

                var result = await _emailService.SendPasswordResetEmailAsync
                    ($"{user.Name} {user.Surname}", code, user.Email);

                if (!result)
                {
                    return BadRequest("Failed to send password reset code");
                }

                HttpContext.Session.SetInt32(SessionPasswordChangeCodeString, code);
                HttpContext.Session.SetInt32(SessionPasswordChangeUserIdString, user.Id);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("get-user-from-code")]
        public async Task<IActionResult> GetUserFromCode(int code)
        {
            if (code == 0)
            {
                _logger.LogError("Code was not received");
                return BadRequest();
            }

            try
            {
                var sessionCode = HttpContext.Session.GetInt32(SessionPasswordChangeCodeString);
                if (!sessionCode.HasValue || sessionCode.Value != code)
                {
                    return BadRequest();
                }

                var userId = HttpContext.Session.GetInt32(SessionPasswordChangeUserIdString);
                if (!userId.HasValue)
                {
                    return BadRequest("Session expired");
                }

                var user = await _userRepository.GetByIdAsync(userId);

                return user == null ? NotFound() : Ok(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            try
            {
                var user = await _userRepository.GetByIdAsync(model.UserId);

                if (user == null)
                {
                    return NotFound();
                }

                user.PasswordHash = HashPassword(model.NewPassword.Trim());

                await _userRepository.UpdateAsync(user);
                await _unitOfWork.Save();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            if (!await IsAuthorizedAsync())
            {
                return Unauthorized();
            }

            try
            {
                var user = await GetCurrentSessionUserAsync();

                if (user == null)
                {
                    return NotFound();
                }

                var model = new UserModel();
                model.MapFrom(user);

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("image")]
        public async Task<IActionResult> GetCurrentUserImage()
        {
            if (!await IsAuthorizedAsync())
            {
                return Unauthorized();
            }

            try
            {
                var user = await GetCurrentSessionUserAsync();

                if (user == null || user.ImageExtension == null)
                {
                    return NotFound();
                }

                var file = await _fileRepository
                    .ReadFileAsync(Core.Enums.FileType.ProfileImage,
                    $"{user.Id}{user.ImageExtension}");

                if (file == null)
                {
                    return NotFound();
                }

                return File(file, "application/octet-stream", $"{user.Id}{user.ImageExtension}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("register-admin")]
        // for creating an admin or a database admin
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminModel model)
        {

            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            if (model.Role != UserRole.AdminDB 
                && model.Role != UserRole.Admin)
            {
                _logger.LogError("Incorrect role");
                return BadRequest();
            }

            try
            {
                var existingUser = (await _userRepository.GetAsync
                    (u => u.Email == model.Email)).FirstOrDefault();

                if (existingUser != null)
                {
                    return NotFound("Email taken");
                }

                // We can create the first admin user without being an admin
                var users = await _userRepository.GetAsync(u => u.Role == UserRole.Admin);

                if ((users != null && users.Count > 0 
                    || model.Role != UserRole.Admin)
                    && !await IsInRoleAsync(UserRole.Admin))
                {
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            var user = new AppUser();
            user.MapFrom(model);

            var tempPassword = Guid.NewGuid().ToString();
            user.PasswordHash = HashPassword(tempPassword);
            user.Role = model.Role;

            var result = await _emailService.SendNewAdminEmailAsync
                    ($"{user.Name} {user.Surname}", 
                    tempPassword, model.Role, user.Email);

            if (!result)
            {
                return BadRequest("Failed to send temporary password");
            }

            try
            {
                await _userRepository.AddAsync(user);
                await _unitOfWork.Save();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha256.ComputeHash(passwordBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
