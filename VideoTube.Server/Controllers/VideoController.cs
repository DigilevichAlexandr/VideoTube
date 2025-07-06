using Microsoft.AspNetCore.Mvc;
using VideoTube.Server.Models;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[RequestSizeLimit(524288000)] // 500MB
[RequestFormLimits(MultipartBodyLengthLimit = 524288000)]
public class VideoController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<VideoController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly VideoTubeDbContext _db;

    public VideoController(IWebHostEnvironment env, ILogger<VideoController> logger, IHttpContextAccessor httpContextAccessor, VideoTubeDbContext db)
    {
        _env = env;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _db = db;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(524288000)] // 500MB
    [RequestFormLimits(MultipartBodyLengthLimit = 524288000)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        _logger.LogInformation("Upload request received");
        
        if (file == null)
        {
            _logger.LogWarning("File is null");
            return BadRequest("File not selected");
        }
        
        if (file.Length == 0)
        {
            _logger.LogWarning("File is empty");
            return BadRequest("File is empty");
        }

        try
        {
            var uploads = Path.Combine(_env.WebRootPath, "videos");
            Directory.CreateDirectory(uploads);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploads, fileName);
            
            _logger.LogInformation($"Saving file to: {filePath}");
            
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var videoUrl = $"{baseUrl}/videos/{fileName}";
            _logger.LogInformation($"File uploaded successfully: {videoUrl}");
            
            return Ok(new { Url = videoUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, "Error uploading file");
        }
    }

    [HttpPost("upload-multiple")]
    [RequestSizeLimit(524288000)] // 500MB
    [RequestFormLimits(MultipartBodyLengthLimit = 524288000)]
    public async Task<IActionResult> UploadMultiple(IFormFileCollection files)
    {
        _logger.LogInformation($"Upload multiple request received. Files count: {files?.Count ?? 0}");
        
        if (files == null || files.Count == 0)
        {
            _logger.LogWarning("No files provided");
            return BadRequest("No files selected");
        }

        var uploadedFiles = new List<object>();
        var errors = new List<string>();
        var totalSize = 0L;

        foreach (var file in files)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    errors.Add($"File '{file?.FileName ?? "unknown"}' is empty or not selected");
                    continue;
                }

                // Validate file size (500MB limit per file)
                if (file.Length > 524288000)
                {
                    errors.Add($"File '{file.FileName}' is too large. Maximum size is 500MB.");
                    continue;
                }

                // Validate file extension
                var extension = Path.GetExtension(file.FileName).ToLower();
                var allowedExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".flv", ".webm" };
                if (!allowedExtensions.Contains(extension))
                {
                    errors.Add($"File '{file.FileName}' has unsupported format. Allowed formats: MP4, AVI, MOV, MKV, WMV, FLV, WEBM");
                    continue;
                }

                var uploads = Path.Combine(_env.WebRootPath, "videos");
                Directory.CreateDirectory(uploads);

                var fileName = Guid.NewGuid() + extension;
                var filePath = Path.Combine(uploads, fileName);
                
                _logger.LogInformation($"Saving file '{file.FileName}' to: {filePath}");
                
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var videoUrl = $"{baseUrl}/videos/{fileName}";
                
                uploadedFiles.Add(new { 
                    OriginalName = file.FileName,
                    Url = videoUrl,
                    Size = file.Length,
                    FileName = fileName,
                    ContentType = GetContentType(extension)
                });
                
                totalSize += file.Length;
                _logger.LogInformation($"File '{file.FileName}' uploaded successfully: {videoUrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file: {file?.FileName}");
                errors.Add($"Error uploading file '{file?.FileName}': {ex.Message}");
            }
        }

        var result = new
        {
            UploadedFiles = uploadedFiles,
            Errors = errors,
            TotalFiles = files.Count,
            SuccessCount = uploadedFiles.Count,
            ErrorCount = errors.Count,
            TotalSize = totalSize
        };

        _logger.LogInformation($"Upload multiple completed. Success: {uploadedFiles.Count}, Errors: {errors.Count}, Total size: {totalSize} bytes");
        return Ok(result);
    }

    [HttpGet("list")]
    public IActionResult List()
    {
        var dir = Path.Combine(_env.WebRootPath, "videos");
        if (!Directory.Exists(dir)) return Ok(new List<object>());

        var request = _httpContextAccessor.HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";

        var files = Directory.GetFiles(dir)
            .Select(f =>
            {
                var fileInfo = new FileInfo(f);
                var fileName = Path.GetFileName(f);
                var extension = Path.GetExtension(f).ToLower();
                
                return new
                {
                    Url = $"{baseUrl}/videos/{fileName}",
                    FileName = fileName,
                    OriginalName = fileName, // Since we use GUID, original name is lost
                    Size = fileInfo.Length,
                    ContentType = GetContentType(extension),
                    UploadDate = fileInfo.CreationTime,
                    LastModified = fileInfo.LastWriteTime
                };
            })
            .OrderByDescending(f => f.UploadDate)
            .ToList();

        return Ok(files);
    }

    [HttpGet("download/{fileName}")]
    public IActionResult Download(string fileName)
    {
        try
        {
            var dir = Path.Combine(_env.WebRootPath, "videos");
            var filePath = Path.Combine(dir, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found");
            }

            var fileInfo = new FileInfo(filePath);
            var contentType = GetContentType(fileInfo.Extension);

            _logger.LogInformation($"Downloading file: {fileName}");

            // Read file to memory and return as FileContentResult
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            return StatusCode(500, "Error downloading file");
        }
    }

    [HttpDelete("delete/{fileName}")]
    public IActionResult Delete(string fileName)
    {
        try
        {
            var dir = Path.Combine(_env.WebRootPath, "videos");
            var filePath = Path.Combine(dir, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found");
            }

            // Check if file is not locked
            try
            {
                using var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                stream.Close();
            }
            catch (IOException)
            {
                return BadRequest("File is being used by another process. Try again later.");
            }

            System.IO.File.Delete(filePath);
            _logger.LogInformation($"Deleted file: {fileName}");

            return Ok(new { message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(500, "Error deleting file");
        }
    }

    [HttpPost("like/{videoFileName}")]
    public async Task<IActionResult> Like(string videoFileName)
    {
        if (string.IsNullOrWhiteSpace(videoFileName))
            return BadRequest("Invalid video file name");

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var exists = await _db.Likes.AnyAsync(l => l.VideoFileName == videoFileName && l.IP == ip);
        if (exists)
            return Ok(new { success = false, message = "Already liked" });

        var like = new Like { VideoFileName = videoFileName, IP = ip };
        _db.Likes.Add(like);
        await _db.SaveChangesAsync();
        return Ok(new { success = true });
    }

    [HttpPost("unlike/{videoFileName}")]
    public async Task<IActionResult> Unlike(string videoFileName)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var like = await _db.Likes.FirstOrDefaultAsync(l => l.VideoFileName == videoFileName && l.IP == ip);
        if (like != null)
        {
            _db.Likes.Remove(like);
            await _db.SaveChangesAsync();
        }
        return Ok(new { success = true });
    }

    [HttpGet("likes/{videoFileName}")]
    public async Task<IActionResult> GetLikes(string videoFileName)
    {
        var count = await _db.Likes.CountAsync(l => l.VideoFileName == videoFileName);
        return Ok(new { count });
    }

    private string GetContentType(string extension)
    {
        return extension.ToLower() switch
        {
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".mkv" => "video/x-matroska",
            ".wmv" => "video/x-ms-wmv",
            ".flv" => "video/x-flv",
            ".webm" => "video/webm",
            _ => "application/octet-stream"
        };
    }
}
