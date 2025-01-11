using Core.Helpers;
using Core.Models;
using DAL.Repositories;
using GrowMate.Models.AppUser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using static System.Net.Mime.MediaTypeNames;

namespace GrowMate.Controllers
{
    [Route("api/[controller]")]
    public class AppUserController : GenericController
    {
        private readonly IFileRepository _fileRepository;
        public AppUserController(UnitOfWork unitOfWork, ILogger<AppUserController> logger, IFileRepository fileRepository)
            : base(unitOfWork, logger)
        {
            _fileRepository = fileRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            // check passwords
            // generate password hash

            var user = new AppUser();
            user.MapFrom(model);
            user.PasswordHash = model.Password; // temp
            user.Role = Core.Enums.UserRole.User;

            try
            {
                await _userRepository.AddAsync(user);
                await _unitOfWork.Save();

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

                // generate password hash
                // check passwords
                // start session

                return Ok(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("logout")]
        // must be authorized
        public async Task<IActionResult> Logout()
        {
            // end session

            return Ok();
        }

        [HttpPut("edit")]
        // must be authorized
        public async Task<IActionResult> Edit([FromForm] EditUserModel model)
        {
            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            try
            {
                var user = (await _userRepository.GetAsync())
                    .FirstOrDefault(); // temp - will be replaced with getting user from session

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
        // must be authorized
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            try
            {
                var user = (await _userRepository.GetAsync())
                    .FirstOrDefault(); // temp - will be replaced with getting user from session

                if (user == null)
                {
                    return NotFound();
                }

                // check passwords
                // generate password hash

                user.PasswordHash = model.NewPassword;

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

                // generate code
                // send code to user
                // save code and userId as session variables

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("get-user-from-code")]
        public async Task<IActionResult> GetUserFromCode(string code)
        {
            if (code.IsNullOrEmpty())
            {
                _logger.LogError("Code was not received");
                return BadRequest();
            }

            try
            {
                // check code in session variables
                // retrieve userId from session variable

                var user = (await _userRepository.GetAsync())
                    .FirstOrDefault(); // temp

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
                // generate password hash

                user.PasswordHash = model.NewPassword;

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
        // must be authorized
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = (await _userRepository.GetAsync())
                    .FirstOrDefault(); // temp - will be replaced with getting user from session

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
        // must be authorized
        public async Task<IActionResult> GetCurrentUserImage()
        {
            try
            {
                var user = (await _userRepository.GetAsync())
                    .FirstOrDefault(); // temp - will be replaced with getting user from session

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
        // must be authorized as admin
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminModel model)
        {
            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            var user = new AppUser();
            user.MapFrom(model);


            // generate temp password
            // generate temp password hash
            user.PasswordHash = "1"; // temp
            user.Role = Core.Enums.UserRole.Admin;

            // send temporary password to email

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
    }
}
