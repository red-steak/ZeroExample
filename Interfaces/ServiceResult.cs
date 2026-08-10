namespace Interfaces
{
    public class ServiceResult(bool isSuccess, string errorMessage, string response)
    {
        public bool IsSuccess { get; set; } = isSuccess;
        public string ErrorMessage { get; set; } = errorMessage;
        public string Response { get; set; } = response;
    }
}
