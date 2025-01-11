using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationContext _context;
        private const string _databaseName = "GrowMate";
        private readonly ILogger<AdminRepository> _logger;

        public AdminRepository(ApplicationContext context, ILogger<AdminRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task BackupDatabaseAsync(string backupPath)
        {
            try
            {
                var sql = $@"
                USE master;
                BACKUP DATABASE [{_databaseName}]
                TO DISK = @backupPath
                WITH FORMAT, INIT;";

                await _context.Database.ExecuteSqlRawAsync(sql, 
                    new[] { new SqlParameter("@backupPath", backupPath) });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to backup the database.", ex);
            }
        }

        public async Task RestoreDatabaseAsync(string backupPath)
        {
            try
            {
                var checkDatabaseExistsSql = $@"
                USE master;
                SELECT COUNT(*) 
                FROM sys.databases 
                WHERE name = '{_databaseName}'";

                var databaseExists = await _context.Database.ExecuteSqlRawAsync(checkDatabaseExistsSql) > 0;

                if (databaseExists)
                {
                    var disableConnectionsSql = $@"
                    USE master;
                    ALTER DATABASE [{_databaseName}]
                    SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                    await _context.Database.ExecuteSqlRawAsync(disableConnectionsSql);
                }

                var restoreSql = $@"
                USE master;
                RESTORE DATABASE [{_databaseName}]
                FROM DISK = @backupPath
                WITH REPLACE;";
                await _context.Database.ExecuteSqlRawAsync(restoreSql, 
                    new[] { new SqlParameter("@backupPath", backupPath) });

                if (databaseExists)
                {
                    var enableConnectionsSql = $@"
                    USE master;
                    ALTER DATABASE [{_databaseName}]
                    SET MULTI_USER;";
                    await _context.Database.ExecuteSqlRawAsync(enableConnectionsSql);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to restore the database.", ex);
            }
        }
    }
}
