using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace MatchISportsAPI.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        // Only initialize if credentials are provided
        if (!string.IsNullOrEmpty(cloudName) && 
            cloudName != "YOUR_CLOUD_NAME" &&
            !string.IsNullOrEmpty(apiKey) && 
            !string.IsNullOrEmpty(apiSecret))
        {
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (_cloudinary == null)
            throw new InvalidOperationException("Cloudinary is not configured. Please add credentials to appsettings.json");

        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Transformation = new Transformation().Width(500).Height(500).Crop("fill")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
            throw new Exception(uploadResult.Error.Message);

        return uploadResult.SecureUrl.ToString();
    }
}
