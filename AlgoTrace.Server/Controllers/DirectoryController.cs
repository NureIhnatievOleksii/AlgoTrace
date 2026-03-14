using System.Security.Claims;
using AlgoTrace.Server.Data;
using AlgoTrace.Server.Models;
using AlgoTrace.Server.Models.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        #region Folder Operations

        [HttpGet("all-folders")]
        public async Task<IActionResult> GetAllUserFolders()
        {
            var userId = GetUserId();
            var folders = await _context
                .Folders.Where(f => f.UserId == userId)
                .Select(f => new
                {
                    f.FolderId,
                    f.Name,
                    f.ParentId,
                })
                .ToListAsync();
            return Ok(folders);
        }

        [HttpGet("folder/{folderId?}")]
        public async Task<IActionResult> GetFolder(Guid? folderId)
        {
            var userId = GetUserId();

            if (folderId == null)
            {
                var rootFolders = await _context
                    .Folders.Where(f => f.UserId == userId && f.ParentId == null)
                    .Select(f => new { f.FolderId, f.Name })
                    .ToListAsync();

                var rootFiles = await _context
                    .Files.Where(f => f.UserId == userId && f.FolderId == null)
                    .Select(f => new { f.FileId, f.Name })
                    .ToListAsync();

                return Ok(
                    new
                    {
                        FolderId = (Guid?)null,
                        Name = "Мій диск",
                        Folders = rootFolders,
                        Files = rootFiles,
                    }
                );
            }

            var folder = await _context
                .Folders.Include(f => f.SubFolders)
                .Include(f => f.Files)
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FolderId == folderId);

            if (folder == null)
                return NotFound();

            return Ok(
                new
                {
                    folder.FolderId,
                    folder.Name,
                    folder.ParentId,
                    Folders = folder.SubFolders.Select(s => new { s.FolderId, s.Name }),
                    Files = folder.Files.Select(f => new { f.FileId, f.Name }),
                }
            );
        }

        [HttpPost("folder")]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderRequest model)
        {
            var newFolder = new Folder
            {
                Name = model.Name,
                ParentId = model.ParentId,
                UserId = GetUserId(),
            };
            _context.Folders.Add(newFolder);
            await _context.SaveChangesAsync();
            return Ok(newFolder);
        }

        [HttpPut("folder/{id}/rename")]
        public async Task<IActionResult> RenameFolder(Guid id, [FromBody] string newName)
        {
            var folder = await _context.Folders.FirstOrDefaultAsync(f =>
                f.FolderId == id && f.UserId == GetUserId()
            );
            if (folder == null)
                return NotFound();
            folder.Name = newName;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("folder/{id}")]
        public async Task<IActionResult> DeleteFolder(Guid id)
        {
            var userId = GetUserId();
            var folder = await _context
                .Folders.Include(f => f.SubFolders)
                .Include(f => f.Files)
                .FirstOrDefaultAsync(f => f.FolderId == id && f.UserId == userId);

            if (folder == null)
                return NotFound();

            // Рекурсивное удаление файлов из всех подпапок
            await DeleteFolderContentsRecursive(folder);

            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task DeleteFolderContentsRecursive(Folder folder)
        {
            var files = await _context
                .Files.Where(f => f.FolderId == folder.FolderId)
                .ToListAsync();
            foreach (var file in files)
            {
                var fullPath = Path.Combine(_storagePath, file.Path);
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
                _context.Files.Remove(file);
            }
            var subFolders = await _context
                .Folders.Where(f => f.ParentId == folder.FolderId)
                .ToListAsync();
            foreach (var sub in subFolders)
                await DeleteFolderContentsRecursive(sub);
        }

        #endregion

        #region File Operations

        [HttpPost("file/upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] Guid? folderId)
        {
            var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_storagePath, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var fileEntry = new AlgoTrace.Server.Models.File
            {
                Name = file.FileName,
                Path = uniqueName,
                UserId = GetUserId(),
                FolderId = folderId,
            };
            _context.Files.Add(fileEntry);
            await _context.SaveChangesAsync();
            return Ok(fileEntry);
        }

        [HttpGet("file/download/{fileId}")]
        public async Task<IActionResult> DownloadFile(Guid fileId)
        {
            var userId = GetUserId();
            var file = await _context.Files.FirstOrDefaultAsync(f =>
                f.FileId == fileId && f.UserId == userId
            );

            if (file == null)
                return NotFound();

            var filePath = Path.Combine(_storagePath, file.Path);
            if (!System.IO.File.Exists(filePath))
                return NotFound("Файл видалено з сервера.");

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, "application/octet-stream", file.Name);
        }

        [HttpDelete("file/{fileId}")]
        public async Task<IActionResult> DeleteFile(Guid fileId)
        {
            var userId = GetUserId();
            var file = await _context.Files.FirstOrDefaultAsync(f =>
                f.FileId == fileId && f.UserId == userId
            );

            if (file == null)
                return NotFound();

            var filePath = Path.Combine(_storagePath, file.Path);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
            return Ok();
        }

        #endregion
    }
}
