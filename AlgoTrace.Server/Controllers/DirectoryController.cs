using AlgoTrace.Server.Data;
using AlgoTrace.Server.Models;
using AlgoTrace.Server.Models.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AlgoTrace.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DirectoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _storagePath;

        public DirectoryController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _storagePath = Path.Combine(env.ContentRootPath, "Storage");
            if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        #region Folder Operations

        [HttpGet("all-folders")]
        public async Task<IActionResult> GetAllUserFolders()
        {
            var userId = GetUserId();
            var folders = await _context.Folders
                .Where(f => f.UserId == userId)
                .Select(f => new { f.FolderId, f.Name, f.ParentId })
                .ToListAsync();
            return Ok(folders);
        }

        [HttpGet("folder/{folderId?}")]
        public async Task<IActionResult> GetFolder(int? folderId)
        {
            var userId = GetUserId();

            if (folderId == null)
            {
                var rootFolders = await _context.Folders
                    .Where(f => f.UserId == userId && f.ParentId == null)
                    .Select(f => new { f.FolderId, f.Name }).ToListAsync();

                var rootFiles = await _context.Files
                    .Include(f => f.Folder)
                    .Where(f => f.FolderId == 0 || (f.Folder != null && f.Folder.UserId == userId && f.Folder.ParentId == null))
                    .Select(f => new { f.FileId, f.Name }).ToListAsync();

                return Ok(new { FolderId = (int?)null, Name = "Мій диск", Folders = rootFolders, Files = rootFiles });
            }

            var folder = await _context.Folders
                .Include(f => f.SubFolders).Include(f => f.Files)
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FolderId == folderId);

            if (folder == null) return NotFound();

            return Ok(new
            {
                folder.FolderId,
                folder.Name,
                folder.ParentId,
                Folders = folder.SubFolders.Select(s => new { s.FolderId, s.Name }),
                Files = folder.Files.Select(f => new { f.FileId, f.Name })
            });
        }

        [HttpPost("folder")]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderRequest model)
        {
            var newFolder = new Folder
            {
                Name = model.Name,
                ParentId = model.ParentId == 0 ? null : model.ParentId,
                UserId = GetUserId()
            };
            _context.Folders.Add(newFolder);
            await _context.SaveChangesAsync();
            return Ok(newFolder);
        }

        [HttpPut("folder/{id}/rename")]
        public async Task<IActionResult> RenameFolder(int id, [FromBody] string newName)
        {
            var folder = await _context.Folders.FirstOrDefaultAsync(f => f.FolderId == id && f.UserId == GetUserId());
            if (folder == null) return NotFound();
            folder.Name = newName;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("folder/{id}")]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var userId = GetUserId();
            var folder = await _context.Folders
                .Include(f => f.SubFolders).Include(f => f.Files)
                .FirstOrDefaultAsync(f => f.FolderId == id && f.UserId == userId);

            if (folder == null) return NotFound();

            // Рекурсивное удаление файлов из всех подпапок
            await DeleteFolderContentsRecursive(folder);

            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task DeleteFolderContentsRecursive(Folder folder)
        {
            foreach (var file in _context.Files.Where(f => f.FolderId == folder.FolderId))
            {
                var fullPath = Path.Combine(_storagePath, file.Path);
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            }
            var subFolders = await _context.Folders.Where(f => f.ParentId == folder.FolderId).ToListAsync();
            foreach (var sub in subFolders) await DeleteFolderContentsRecursive(sub);
        }

        #endregion

        #region File Operations

        [HttpPost("file/upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] int? folderId)
        {
            var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_storagePath, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create)) await file.CopyToAsync(stream);

            var fileEntry = new AlgoTrace.Server.Models.File
            {
                Name = file.FileName,
                Path = uniqueName,
                FolderId = folderId ?? 0
            };
            _context.Files.Add(fileEntry);
            await _context.SaveChangesAsync();
            return Ok(fileEntry);
        }

        [HttpGet("file/download/{fileId}")]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            var userId = GetUserId();
            var file = await _context.Files
                .Include(f => f.Folder)
                .FirstOrDefaultAsync(f => f.FileId == fileId && (f.Folder != null ? f.Folder.UserId == userId : true));

            if (file == null) return NotFound();

            var filePath = Path.Combine(_storagePath, file.Path);
            if (!System.IO.File.Exists(filePath)) return NotFound("Файл видалено з сервера.");

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, "application/octet-stream", file.Name);
        }

        [HttpDelete("file/{fileId}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            var userId = GetUserId();
            var file = await _context.Files
                .Include(f => f.Folder)
                .FirstOrDefaultAsync(f => f.FileId == fileId);

            if (file == null) return NotFound();

            var filePath = Path.Combine(_storagePath, file.Path);
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
            return Ok();
        }

        #endregion
    }
}