using Core.Enums;
using Core.Helpers;
using Core.Models;
using DAL.Repositories;
using GrowMate.Models.Notification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace GrowMate.Controllers
{
    [Route("api/[controller]")]
    public class NotificationController : GenericController
    {
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<Advice> _adviceRepository;
        public NotificationController(UnitOfWork unitOfWork, ILogger<NotificationController> logger) 
            : base(unitOfWork, logger)
        {
            _notificationRepository = unitOfWork.NotificationRepository;
            _adviceRepository = unitOfWork.AdviceRepository;
        }

        [HttpPost("post-advice-notification")]
        // must be authorized as administrator
        public async Task<IActionResult> PostAdviceNotification([FromBody] PostAdviceNotificationModel model)
        {
            if (!await IsInRoleAsync(UserRole.Admin))
            {
                return Unauthorized();
            }

            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            var notification = new Notification();
            notification.MapFrom(model);
            notification.Type = Core.Enums.NotificationType.Advice;
            notification.AdviceId = model.AdviceId;

            try
            {
                await _notificationRepository.AddAsync(notification);
                await _unitOfWork.Save();

                return Ok(notification.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("create-advice")]
        public async Task<IActionResult> CreateAdvice([FromBody] string adviceText)
        {
            if (!await IsInRoleAsync(UserRole.Admin))
            {
                return Unauthorized();
            }

            if (adviceText.IsNullOrEmpty())
            {
                _logger.LogError("adviceText was not received");
                return BadRequest();
            }

            var advice = new Advice();
            advice.Text = adviceText;

            try
            {
                await _adviceRepository.AddAsync(advice);
                await _unitOfWork.Save();

                return Ok(advice.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("edit-advise")]
        public async Task<IActionResult> EditAdvice([FromBody] AdviceModel model)
        {
            if (!await IsInRoleAsync(UserRole.Admin))
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
                var advice = await _adviceRepository.GetByIdAsync(model.Id);
                if (advice == null)
                {
                    return NotFound();
                }

                advice.MapFrom(model);

                await _adviceRepository.UpdateAsync(advice);
                await _unitOfWork.Save();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("all-advices")]
        public async Task<IActionResult> GetAllAdvices()
        {
            if (!await IsInRoleAsync(UserRole.Admin))
            {
                return Unauthorized();
            }

            try
            {
                var advices = await _adviceRepository.GetAsync();
                return Ok(advices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetNotifications()
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

                var models = new List<NotificationModel>();

                foreach (var notification in user.Devices.SelectMany(d => d.Notifications))
                {
                    var model = new NotificationModel()
                    {
                        DeviceName = notification.Device.Name
                    };
                    model.MapFrom(notification);

                    if (notification.Type == Core.Enums.NotificationType.WaterLevel)
                    {
                        model.DeviceCriticalMaxTemperature = notification
                            .Device.CriticalMaxTemperature;
                        model.DeviceCriticalMinTemperature = notification
                            .Device.CriticalMinTemperature;
                    }

                    models.Add(model);
                }

                var postedAdvices = await _notificationRepository.GetAsync
                    (a => a.Type == Core.Enums.NotificationType.Advice && a.Created < DateTime.Now);

                foreach (var advice in postedAdvices)
                {
                    var model = new NotificationModel();
                    model.MapFrom(advice);
                    model.AdviceText = advice.Advice.Text;

                    models.Add(model);
                }

                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
