using Interfaces;
using File = System.IO.File;

namespace Services
{
    public class ReadFileService : IReadService
    {
        public async Task<ServiceResult> ReadAsync(string filePath)
        {
            var errorMessage = string.Empty;
            var response = string.Empty;

            if (File.Exists(filePath) && !string.IsNullOrEmpty(filePath))
            {
                try
                {
                    response = await File.ReadAllTextAsync(filePath);
                    return new ServiceResult(true, string.Empty, response);
                }
                catch (UnauthorizedAccessException unauthAccessEx)
                {
                    errorMessage = $"Error: Unauthorized access to file: {filePath}. {unauthAccessEx.Message}";
                    return new ServiceResult(false, errorMessage, string.Empty);
                }
                catch (IOException ioEx)
                {
                    errorMessage = $"Error: IO exception while reading file: {filePath}. {ioEx.Message}";
                    return new ServiceResult(false, errorMessage, string.Empty);
                }
                catch (Exception ex)
                {
                    errorMessage = $"Error: An unexpected error occurred while opening file: {filePath}. {ex.Message}";
                    return new ServiceResult(false, errorMessage, string.Empty);
                }                
            }

            errorMessage = "File does not exist or path is invalid!";
            return new ServiceResult(false, errorMessage, string.Empty);
        }
    }
}
