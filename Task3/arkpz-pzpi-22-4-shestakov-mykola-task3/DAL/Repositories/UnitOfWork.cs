using Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class UnitOfWork : IDisposable
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<UnitOfWork> _logger;
        private readonly ApplicationContext _context;
        private bool disposed = false;

        private Repository<AppUser> _userRepository;
        private Repository<Device> _deviceRepository;
        private Repository<DeviceLog> _deviceLogRepository;
        private Repository<Notification> _notificationRepository;
        private Repository<Schedule> _scheduleRepository;
        private Repository<WateringLog> _wateringLogRepository;
        private Repository<ManualWateringRequest> _manualWateringRequestRepository;
        private Repository<Advice> _adviceRepository;
        private AdminRepository _adminRepository;

        public AdminRepository AdminRepository
        {
            get
            {
                _adminRepository ??= new AdminRepository(_context,
                        new Logger<AdminRepository>(_loggerFactory));

                return _adminRepository;
            }
        }
        public Repository<AppUser> UserRepository
        {
            get
            {
                _userRepository ??= new Repository<AppUser>(_context,
                        new Logger<Repository<AppUser>>(_loggerFactory));

                return _userRepository;
            }
        }

        public Repository<Device> DeviceRepository
        {
            get
            {
                _deviceRepository ??= new Repository<Device>(_context,
                        new Logger<Repository<Device>>(_loggerFactory));

                return _deviceRepository;
            }
        }

        public Repository<DeviceLog> DeviceLogRepository
        {
            get
            {
                _deviceLogRepository ??= new Repository<DeviceLog>(_context,
                        new Logger<Repository<DeviceLog>>(_loggerFactory));

                return _deviceLogRepository;
            }
        }

        public Repository<Notification> NotificationRepository
        {
            get
            {
                _notificationRepository ??= new Repository<Notification>(_context,
                        new Logger<Repository<Notification>>(_loggerFactory));

                return _notificationRepository;
            }
        }

        public Repository<Schedule> ScheduleRepository
        {
            get
            {
                _scheduleRepository ??= new Repository<Schedule>(_context,
                        new Logger<Repository<Schedule>>(_loggerFactory));

                return _scheduleRepository;
            }
        }

        public Repository<WateringLog> WateringLogRepository
        {
            get
            {
                _wateringLogRepository ??= new Repository<WateringLog>(_context,
                        new Logger<Repository<WateringLog>>(_loggerFactory));

                return _wateringLogRepository;
            }
        }

        public Repository<ManualWateringRequest> ManualWateringRequestRepository
        {
            get
            {
                _manualWateringRequestRepository ??= new Repository<ManualWateringRequest>(_context,
                        new Logger<Repository<ManualWateringRequest>>(_loggerFactory));

                return _manualWateringRequestRepository;
            }
        }

        public Repository<Advice> AdviceRepository
        {
            get
            {
                _adviceRepository ??= new Repository<Advice>(_context,
                        new Logger<Repository<Advice>>(_loggerFactory));

                return _adviceRepository;
            }
        }

        public UnitOfWork(ApplicationContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _loggerFactory = loggerFactory;
            _logger = new Logger<UnitOfWork>(_loggerFactory);
        }

        public async Task<int> Save()
        {
            try
            {
                _logger.LogInformation("Saving changes to the database");

                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save changes to the database! Error: {errorMessage}", ex.Message);

                throw new Exception($"Fail to save changes to the database: {ex.Message}");
            }
        }

        protected virtual async Task Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    await _context.DisposeAsync();
                }
            }

            this.disposed = true;
        }

        public async void Dispose()
        {
            await Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
