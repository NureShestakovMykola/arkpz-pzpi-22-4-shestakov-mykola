using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public interface IAdminRepository
    {
        Task BackupDatabaseAsync(string backupPath);
        Task RestoreDatabaseAsync(string backupPath);
        Task SetupDatabaseAsync();
    }
}
