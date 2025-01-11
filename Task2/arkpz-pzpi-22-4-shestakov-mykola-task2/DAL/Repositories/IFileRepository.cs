using Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public interface IFileRepository
    {
        Task SaveFileAsync(IFormFile file, FileType fileType, string fileName);
        Task DeleteFileAsync(FileType fileType, string fileName);
        Task<byte[]> ReadFileAsync(FileType fileType, string fileName);
    }
}
