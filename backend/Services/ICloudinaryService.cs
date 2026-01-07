namespace MatchISportsAPI.Services;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file);
}
