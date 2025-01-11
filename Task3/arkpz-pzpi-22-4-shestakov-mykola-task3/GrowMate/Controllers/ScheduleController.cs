using Core.Helpers;
using Core.Models;
using DAL.Repositories;
using GrowMate.Models.Schedule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrowMate.Controllers
{
    [Route("api/[controller]")]
    public class ScheduleController : GenericController
    {
        private readonly IRepository<Schedule> _scheduleRepository;
        public ScheduleController(UnitOfWork unitOfWork, ILogger<ScheduleController> logger) 
            : base(unitOfWork, logger)
        {
            _scheduleRepository = unitOfWork.ScheduleRepository;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSchedule([FromBody] ScheduleModel model)
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

            var schedule = new Schedule();
            schedule.MapFrom(model);

            if (schedule.ScheduleType == Core.Enums.ScheduleType.Weekly)
            {
                if (model.Days.Count == 0 || model.Days.Count > 7
                    || model.Days.Any(d => d > 6) || model.Days.Any(d => d < 0)
                    || model.Days.Distinct().Count() != model.Days.Count)
                {
                    return BadRequest();
                }

                // Days of week start from sunday and 0
                var days = 0;

                foreach (var day in model.Days)
                {
                    days += 1 << (int)day;
                }

                schedule.Days = days;
            }

            try
            {
                var user = await GetCurrentSessionUserAsync();

                if (user == null)
                {
                    return NotFound();
                }

                schedule.UserId = user.Id;

                await _scheduleRepository.AddAsync(schedule);
                await _unitOfWork.Save();

                return Ok(schedule.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditSchedule([FromBody] EditScheduleModel model)
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
                var schedule = await _scheduleRepository.GetByIdAsync(model.Id);
                if (schedule == null)
                {
                    return NotFound();
                }

                schedule.MapFrom(model);

                if (schedule.ScheduleType == Core.Enums.ScheduleType.Weekly)
                {
                    if (model.Days.Count == 0 || model.Days.Count > 7
                        || model.Days.Any(d => d > 6) || model.Days.Any(d => d < 0)
                        || model.Days.Distinct().Count() != model.Days.Count)
                    {
                        return BadRequest();
                    }

                    var days = 0;

                    foreach (var day in model.Days)
                    {
                        days += 1 << (int)day;
                    }

                    schedule.Days = days;
                }

                await _scheduleRepository.UpdateAsync(schedule);
                await _unitOfWork.Save();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSchedule(int id)
        {
            if (!await IsAuthorizedAsync())
            {
                return Unauthorized();
            }

            if (id == 0)
            {
                _logger.LogError("id was not received");
                return BadRequest();
            }

            try
            {
                var schedule = await _scheduleRepository.GetByIdAsync(id);
                if (schedule == null)
                {
                    return NotFound();
                }

                var model = new ScheduleModel();
                model.MapFrom(schedule);

                // Days of week start from sunday and 0
                if (schedule.ScheduleType == Core.Enums.ScheduleType.Weekly)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if ((schedule.Days & (1 << i)) != 0)
                        {
                            model.Days.Add(i);
                        }
                    }
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
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

                var models = new List<ScheduleListItemModel>();

                foreach (var schedule in user.Schedules)
                {
                    var model = new ScheduleListItemModel();
                    model.MapFrom(schedule);
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
