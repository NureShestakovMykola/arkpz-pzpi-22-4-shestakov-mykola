using Core.Helpers;
using Core.Models;
using DAL.Repositories;
using GrowMate.Models.Device;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrowMate.Controllers
{
    [Route("api/[controller]")]
    public class DeviceController : GenericController
    {
        private readonly IRepository<Device> _deviceRepository;
        private readonly IRepository<DeviceLog> _deviceLogRepository;
        private readonly IRepository<WateringLog> _wateringLogRepository;
        private readonly IRepository<ManualWateringRequest> _manualWateringRequestRepository;
        private readonly IFileRepository _fileRepository;
        public DeviceController(UnitOfWork unitOfWork, ILogger<GenericController> logger, IFileRepository fileRepository) 
            : base(unitOfWork, logger)
        {
            _deviceRepository = unitOfWork.DeviceRepository;
            _deviceLogRepository = unitOfWork.DeviceLogRepository;
            _wateringLogRepository = unitOfWork.WateringLogRepository;
            _manualWateringRequestRepository = unitOfWork.ManualWateringRequestRepository;
            _fileRepository = fileRepository;
        }

        [HttpPost("create")]
        // must be authorized
        public async Task<IActionResult> CreateDevise([FromBody] CreateDeviceModel model)
        {
            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            var device = new Device();
            device.MapFrom(model);

            try
            {
                var user = (await _userRepository.GetAsync())
                    .FirstOrDefault(); // temp - will be replaced with getting user from session

                if (user == null)
                {
                    return NotFound();
                }

                device.UserId = user.Id;

                await _deviceRepository.AddAsync(device);
                await _unitOfWork.Save();

                // TODO: connect to physical device

                return Ok(device.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("edit")]
        // must be authorized
        public async Task<IActionResult> EditDevise([FromForm] EditDeviceModel model)
        {
            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            try
            {
                var device = await _deviceRepository.GetByIdAsync(model.Id);
                if (device == null)
                {
                    return NotFound();
                }

                device.MapFrom(model);

                if (model.NewImage != null)
                {
                    if (device.ImageExtension != null)
                    {
                        await _fileRepository.DeleteFileAsync(Core.Enums.FileType.PlantImage, $"{device.Id}{device.ImageExtension}");
                    }

                    var extension = Path.GetExtension(model.NewImage.FileName);

                    await _fileRepository.SaveFileAsync(model.NewImage, Core.Enums.FileType.PlantImage, $"{device.Id}{extension}");
                    device.ImageExtension = extension;
                }

                await _deviceRepository.UpdateAsync(device);
                await _unitOfWork.Save();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("automatic-watering-on")]
        // must be authorized
        public async Task<IActionResult> TurnOnAutomaticWatering(int deviceId)
        {
            if (deviceId == 0)
            {
                _logger.LogError("deviceId was not received");
                return BadRequest();
            }

            try
            {
                var device = await _deviceRepository.GetByIdAsync(deviceId);
                if (device == null)
                {
                    return NotFound();
                }

                device.AutomaticWatering = true;

                await _deviceRepository.UpdateAsync(device);
                await _unitOfWork.Save();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("automatic-watering-off")]
        // must be authorized
        public async Task<IActionResult> TurnOffAutomaticWatering(int deviceId)
        {
            if (deviceId == 0)
            {
                _logger.LogError("deviceId was not received");
                return BadRequest();
            }

            try
            {
                var device = await _deviceRepository.GetByIdAsync(deviceId);
                if (device == null)
                {
                    return NotFound();
                }

                device.AutomaticWatering = false;

                await _deviceRepository.UpdateAsync(device);
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
        // must be authorized
        public async Task<IActionResult> GetDevice(int id)
        {
            if (id == 0)
            {
                _logger.LogError("id was not received");
                return BadRequest();
            }

            try
            {
                var device = await _deviceRepository.GetByIdAsync(id);
                if (device == null)
                {
                    return NotFound();
                }

                var model = new DeviceModel();
                model.MapFrom(device);
                
                var lastLog = device.DeviceLogs.MaxBy(d => d.LogDateTime);
                model.WaterLevel = lastLog != null ? lastLog.WaterLevel : 0;
                
                model.LastWatering = device.WateringLogs.Count > 0 
                    ? device.WateringLogs.Max(d => d.LogDateTime) : null;

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}/image")]
        // must be authorized
        public async Task<IActionResult> GetDeviceImage(int id)
        {
            if (id == 0)
            {
                _logger.LogError("id was not received");
                return BadRequest();
            }

            try
            {
                var device = await _deviceRepository.GetByIdAsync(id);
                if (device == null || device.ImageExtension == null)
                {
                    return NotFound();
                }

                var file = await _fileRepository
                    .ReadFileAsync(Core.Enums.FileType.PlantImage,
                    $"{device.Id}{device.ImageExtension}");

                if (file == null)
                {
                    return NotFound();
                }

                return File(file, "application/octet-stream", $"{device.Id}{device.ImageExtension}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("moisture-data")]
        public async Task<IActionResult> GetMoistureData(int deviceId, int logsCount)
        {
            if (deviceId == 0)
            {
                _logger.LogError("deviceId was not received");
                return BadRequest();
            }

            if (logsCount == 0)
            {
                _logger.LogError("logsCount was not received");
                return BadRequest();
            }

            try
            {
                var device = await _deviceRepository.GetByIdAsync(deviceId);
                if (device == null)
                {
                    return NotFound();
                }

                var models = new List<MoistureData>();
                foreach (var data in device.DeviceLogs
                    .OrderByDescending(l => l.LogDateTime)
                    .Take(logsCount))
                {
                    var model = new MoistureData();
                    model.MapFrom(data);

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

        [HttpGet("watering-data")]
        public async Task<IActionResult> GetWateringData(int deviceId)
        {
            if (deviceId == 0)
            {
                _logger.LogError("deviceId was not received");
                return BadRequest();
            }

            try
            {
                var device = await _deviceRepository.GetByIdAsync(deviceId);
                if (device == null)
                {
                    return NotFound();
                }

                var models = device.WateringLogs
                    .Select(l => l.LogDateTime)
                    .OrderDescending();

                return Ok(models);
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
            try
            {
                var user = (await _userRepository.GetAsync())
                    .FirstOrDefault(); // temp - will be replaced with getting user from session

                if (user == null)
                {
                    return NotFound();
                }

                var models = new List<DeviceListItemModel>();

                foreach (var device in user.Devices)
                {
                    var model = new DeviceListItemModel();
                    model.MapFrom(device);
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

        [HttpGet("for-schedule/{scheduleId}")]
        public async Task<IActionResult> GetForSchedule(int scheduleId)
        {
            if (scheduleId == 0)
            {
                _logger.LogError("scheduleId was not received");
                return BadRequest();
            }

            try
            {
                var devices = await _deviceRepository.GetAsync(d => d.ScheduleId == scheduleId);

                var models = new List<DeviceListItemModel>();

                foreach (var device in devices)
                {
                    var model = new DeviceListItemModel();
                    model.MapFrom(device);
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

        [HttpPost("log-device")]
        public async Task<IActionResult> LogDeviceData([FromBody] CreateDeviceLogModel model)
        {
            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            var deviceLog = new DeviceLog();
            deviceLog.MapFrom(model);
            deviceLog.LogDateTime = DateTime.Now;

            try
            {
                // check critical values
                // create notifications if needed

                await _deviceLogRepository.AddAsync(deviceLog);
                await _unitOfWork.Save();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("log-watering")]
        public async Task<IActionResult> LogWateringData(int deviceId)
        {
            if (deviceId == 0)
            {
                _logger.LogError("deviceId was not received");
                return BadRequest();
            }

            var wateringLog = new WateringLog();
            wateringLog.DeviceId = deviceId;
            wateringLog.LogDateTime = DateTime.Now;

            try
            {
                // create notification

                await _wateringLogRepository.AddAsync(wateringLog);
                await _unitOfWork.Save();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("manual-watering-request")]
        public async Task<IActionResult> ManualWateringRequest([FromBody] ManualWateringRequestModel model)
        {
            if (model == null)
            {
                _logger.LogError("Model was not received");
                return BadRequest();
            }

            var request = new ManualWateringRequest();
            request.MapFrom(model);
            request.Created = DateTime.Now;

            try
            {
                await _manualWateringRequestRepository
                    .AddAsync(request);
                await _unitOfWork.Save();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("check-needs-watering")]
        public async Task<IActionResult> CheckNeedsWatering(int deviceId)
        {
            if (deviceId == 0)
            {
                _logger.LogError("deviceId was not received");
                return BadRequest();
            }

            try
            {
                // logic to check if the device needs to be turned on or off

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        // must be authorized
        public async Task<IActionResult> DeleteDevice(int id)
        {
            if (id == 0)
            {
                _logger.LogError("id was not received");
                return BadRequest();
            }

            try
            {
                var device = await _deviceRepository.GetByIdAsync(id);

                await _deviceRepository.DeleteAsync(device);
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
