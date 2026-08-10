namespace Interfaces
{
    public interface IReadService
    {
        Task<ServiceResult> ReadAsync(string filePath);
    }
}
