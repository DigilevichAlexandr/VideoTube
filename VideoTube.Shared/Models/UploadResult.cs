namespace VideoTube.Shared.Models;

public class UploadResult
{
    public bool Success { get; set; }
    public string? Url { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
} 